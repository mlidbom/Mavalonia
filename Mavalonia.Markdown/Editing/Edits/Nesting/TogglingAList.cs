using Mavalonia.Markdown.Editing.Changes;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;
using Mavalonia.Markdown.Rendering.Containers;

namespace Mavalonia.Markdown.Editing.Edits.Nesting;

///<summary>
/// Makes each paragraph and heading of a range, such as what is selected or where the caret is, an item of a list of a <see cref="ListKind"/>.<br/>
/// When each of them is one already, it takes them out of their lists instead.
///</summary>
///<remarks>
/// An item of a list of the other kind turns its whole list into one of the kind: the items of a list in markdown are all of one kind.<br/>
/// A paragraph or heading that follows an item of a list of the kind becomes the next item of that list.<br/>
/// One inside a list item, after the first block of the item, is no item of its own, as no marker is written before it: it becomes the first item of a list inside that item.
///</remarks>
class TogglingAList
{
   readonly RenderedMarkdown _rendered;
   readonly ListKind _kind;
   readonly IReadOnlyList<RenderedBlock> _blocks;

   internal TogglingAList(RenderedRange range, ListKind kind)
   {
      _rendered = range.Rendered;
      _kind = kind;
      _blocks = [..range.Blocks.Where(it => it is Paragraph or Heading)];
   }

   ///<summary>Whether the range has a paragraph or a heading to make an item.</summary>
   internal bool IsPossible => _blocks.Count > 0;

   ///<summary>Whether each paragraph and heading of the range is an item of a list of the kind already, so that they are taken out of their lists.</summary>
   internal bool TakesThemOutOfTheirLists => IsPossible && _blocks.All(IsAnItemOfTheKind);

   ///<summary>The changes to make, one after the other: each is made to what is rendered after the one before it. The blocks keep their numbers.</summary>
   internal IEnumerable<Func<RenderedMarkdown, SourceChange?>> Changes() =>
      TakesThemOutOfTheirLists
         ? _blocks.Select(_rendered.NumberOf).Select<int, Func<RenderedMarkdown, SourceChange?>>(number => rendered => TakeOutOfItsList(rendered.Blocks[number], rendered))
         : BlocksToMakeItems().Select(_rendered.NumberOf).Select<int, Func<RenderedMarkdown, SourceChange?>>(number => rendered => MakeAnItem(rendered.Blocks[number], rendered));

   bool IsAnItemOfTheKind(RenderedBlock block) => ItemStartedBy(block, _rendered)?.Kind == _kind;

   ///<summary>The list item whose marker is written before <paramref name="block"/>: the one that it's the first block of. Null when there is none.</summary>
   static ListItem? ItemStartedBy(RenderedBlock block, RenderedMarkdown rendered) =>
      block.Containers.Innermost is ListItem item && rendered.IsTheFirstBlockInside(item, block) ? item : null;

   static SourceChange TakeOutOfItsList(RenderedBlock block, RenderedMarkdown rendered) =>
      new LeavingAContainer(new Caret(rendered, RenderedPosition.AtTheStartOf(block).Offset)).Change();

   ///<summary>Those that aren't items of a list of the kind, with, for each item of a list of the other kind, the first blocks of all the items of its list. In the order of the markdown.</summary>
   IEnumerable<RenderedBlock> BlocksToMakeItems() =>
      _blocks.Where(it => !IsAnItemOfTheKind(it))
             .SelectMany(it => ItemStartedBy(it, _rendered) is {} item ? FirstBlocksOfTheItemsOfTheListOf(item, it) : [it])
             .Distinct()
             .OrderBy(_rendered.NumberOf);

   ///<summary>The block that starts each item of the list that <paramref name="item"/>, which <paramref name="block"/> starts, is in.</summary>
   IEnumerable<RenderedBlock> FirstBlocksOfTheItemsOfTheListOf(ListItem item, RenderedBlock block)
   {
      var containersOfTheList = block.Containers.WithoutTheInnermost;
      var first = _rendered.NumberOf(block);
      while(first > 0 && IsInAnItemOfTheList(_rendered.Blocks[first - 1])) first--;
      var last = _rendered.NumberOf(block);
      while(last < _rendered.Blocks.Count - 1 && IsInAnItemOfTheList(_rendered.Blocks[last + 1])) last++;

      return _rendered.Blocks.Take(last + 1).Skip(first).Where(it => it.Containers.Count == containersOfTheList.Count + 1 && ItemStartedBy(it, _rendered) is not null);

      bool IsInAnItemOfTheList(RenderedBlock other) =>
         other.Containers.SharedWith(containersOfTheList).Count == containersOfTheList.Count
      && other.Containers.OutermostFirst.ElementAtOrDefault(containersOfTheList.Count) is ListItem itemOfTheOther
      && itemOfTheOther.LooksLike(item);
   }

   ///<summary>Makes <paramref name="block"/> the first block of an item of a list of the kind: in place of the item it starts, if any, or else inside the containers it is in.</summary>
   SourceChange MakeAnItem(RenderedBlock block, RenderedMarkdown rendered) =>
      ItemStartedBy(block, rendered) is null
         ? new BlockMove(rendered, block, block.Containers.Inside(NewItem(block, block.Containers, rendered))).Change()
         : new ChangingTheMarkerOfAListItem(rendered, block, NewItem(block, block.Containers.WithoutTheInnermost, rendered)).Change();

   ///<summary>The next item of the list of the kind that the block before <paramref name="block"/> is in, inside <paramref name="containersOfTheList"/>. The first item of a new one, when there is no such list.</summary>
   ListItem NewItem(RenderedBlock block, BlockContainers containersOfTheList, RenderedMarkdown rendered) =>
      rendered.BlockBefore(block) is {} before
   && before.Containers.SharedWith(containersOfTheList).Count == containersOfTheList.Count
   && before.Containers.OutermostFirst.ElementAtOrDefault(containersOfTheList.Count) is ListItem { Kind: var kind } itemBefore
   && kind == _kind
         ? itemBefore.NewItemAfter()
         : ListItem.FirstOf(_kind);
}
