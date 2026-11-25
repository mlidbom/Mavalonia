using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Editing.Edits.Nesting;

///<summary>Makes a list item an item of a list inside the item before it.</summary>
class IndentingAListItem
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedPosition _position;
   readonly ListItem _item;
   readonly ListItem _itemBefore;

   IndentingAListItem(RenderedMarkdown rendered, RenderedPosition position, ListItem item, ListItem itemBefore)
   {
      _rendered = rendered;
      _position = position;
      _item = item;
      _itemBefore = itemBefore;
   }

   ///<summary>Indents the list item that <paramref name="position"/> is in. Null when it isn't in a list item that has another before it.</summary>
   internal static IndentingAListItem? At(RenderedMarkdown rendered, RenderedPosition position)
   {
      if(position.Block.Containers.Innermost is not ListItem item) return null;
      if(rendered.BlockBefore(FirstBlockOf(item, rendered)) is not {} blockBefore) return null;

      return ItemAround(blockBefore, ofTheSameListAs: position.Block) is {} itemBefore ? new IndentingAListItem(rendered, position, item, itemBefore) : null;
   }

   static RenderedBlock FirstBlockOf(ListItem item, RenderedMarkdown rendered) => rendered.Blocks.First(it => it.IsInside(item));

   static ListItem? ItemAround(RenderedBlock blockBefore, RenderedBlock ofTheSameListAs)
   {
      var depthOfTheList = ofTheSameListAs.Containers.Count - 1;
      var isInTheSameContainers = blockBefore.Containers.SharedWith(ofTheSameListAs.Containers).Count == depthOfTheList;
      return isInTheSameContainers ? blockBefore.Containers.OutermostFirst.ElementAtOrDefault(depthOfTheList) as ListItem : null;
   }

   RenderedBlock Block => _position.Block;
   RenderedBlock FirstBlock => FirstBlockOf(_item, _rendered);
   RenderedBlock LastBlock => _rendered.Blocks.Last(it => it.IsInside(_item));
   RenderedBlock BlockBefore => _rendered.BlockBefore(FirstBlock)!;
   BlockContainers ContainersOfTheList => Block.Containers.WithoutTheInnermost;

   ///<summary>Without a blank line between them, the item may become a part of the text before it.</summary>
   internal SourceChange Change()
   {
      var onTheNextLine = WithTheLineBreakBefore(_rendered.Source.LineBreak);
      if(IsInsideTheItemBefore(onTheNextLine)) return onTheNextLine;

      var afterABlankLine = WithTheLineBreakBefore(new LineBreaksInside(_rendered, ContainersOfTheList).BlankLine + _rendered.Source.LineBreak);
      return IsInsideTheItemBefore(afterABlankLine) ? afterABlankLine : onTheNextLine;
   }

   bool IsInsideTheItemBefore(SourceChange change) =>
      _rendered.NumberOf(Block) is var number && number < change.After.Blocks.Count && change.After.Blocks[number].Containers.Count == Block.Containers.Count + 1;

   SourceChange WithTheLineBreakBefore(string lineBreak) =>
      SourceChange.Replace(_rendered, new SourceRange(BlockBefore.Source.End, LastBlock.Source.End), lineBreak + FirstLine() + OtherLines())
                  .WithTheCaretAt(_rendered.NumbersOf(_position));

   string FirstLine() => ContainersOfTheList.PrefixOfTheirLines + _itemBefore.PrefixOfItsOtherLines + NewItem().PrefixOfItsFirstLine + _rendered.Source[FirstBlock.Source.Start..EndOfLineAt(FirstBlock.Source.Start)];

   ///<summary>The next item of the list that is inside the item before already, or the first item of a new one.</summary>
   ListItem NewItem()
   {
      var itemInsideAlready = BlockBefore.Containers.OutermostFirst.SkipWhile(it => it != _itemBefore).Skip(1).FirstOrDefault() as ListItem;
      return itemInsideAlready?.NewItemAfter() ?? _item.AsTheFirstOfItsList();
   }

   ///<summary>The lines after the one with the item's marker, each with the line break before it, and as far in as the text of the item before.</summary>
   string OtherLines()
   {
      var lines = "";
      for(var startOfLine = StartOfLineAfter(FirstBlock.Source.Start); startOfLine < LastBlock.Source.End; startOfLine = StartOfLineAfter(startOfLine)) lines += _rendered.Source.LineBreak + IndentedLineAt(startOfLine);

      return lines;
   }

   string IndentedLineAt(int startOfLine)
   {
      var endOfPrefix = ContainersOfTheList.EndOfTheirPrefixes(_rendered.Source, startOfLine);
      var line = _rendered.Source[endOfPrefix..EndOfLineAt(startOfLine)];
      return _rendered.Source[startOfLine..endOfPrefix] + (line.Length == 0 ? "" : _itemBefore.PrefixOfItsOtherLines + line);
   }

   int StartOfLineAfter(int offset) => _rendered.Source.StartOfLineAfter(offset) ?? LastBlock.Source.End;
   int EndOfLineAt(int offset) => Math.Min(LastBlock.Source.End, _rendered.Source.EndOfLineAt(offset));
}
