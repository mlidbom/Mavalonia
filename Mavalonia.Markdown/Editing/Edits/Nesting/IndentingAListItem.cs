using Mavalonia.Markdown.Editing.Changes;
using Mavalonia.Markdown.Editing.Writing;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;
using Mavalonia.Markdown.Rendering.Containers;

namespace Mavalonia.Markdown.Editing.Edits.Nesting;

///<summary>Makes a list item an item of a list inside the item before it.</summary>
class IndentingAListItem
{
   readonly Caret _caret;
   readonly ListItem _item;
   readonly ListItem _itemBefore;

   IndentingAListItem(Caret caret, ListItem item, ListItem itemBefore)
   {
      _caret = caret;
      _item = item;
      _itemBefore = itemBefore;
   }

   ///<summary>Indents the list item that <paramref name="caret"/> is in. Null when it isn't in a list item that has another before it.</summary>
   internal static IndentingAListItem? At(Caret caret)
   {
      if(caret.Block.Containers.Innermost is not ListItem item) return null;
      if(caret.Rendered.BlockBefore(FirstBlockOf(item, caret.Rendered)) is not {} blockBefore) return null;

      return ItemAround(blockBefore, ofTheSameListAs: caret.Block) is {} itemBefore ? new IndentingAListItem(caret, item, itemBefore) : null;
   }

   static RenderedBlock FirstBlockOf(ListItem item, RenderedMarkdown rendered) => rendered.Blocks.First(it => it.IsInside(item));

   static ListItem? ItemAround(RenderedBlock blockBefore, RenderedBlock ofTheSameListAs)
   {
      var depthOfTheList = ofTheSameListAs.Containers.Count - 1;
      var isInTheSameContainers = blockBefore.Containers.SharedWith(ofTheSameListAs.Containers).Count == depthOfTheList;
      return isInTheSameContainers ? blockBefore.Containers.OutermostFirst.ElementAtOrDefault(depthOfTheList) as ListItem : null;
   }

   RenderedMarkdown Rendered => _caret.Rendered;
   RenderedBlock Block => _caret.Block;
   RenderedBlock FirstBlock => FirstBlockOf(_item, Rendered);
   RenderedBlock LastBlock => Rendered.Blocks.Last(it => it.IsInside(_item));
   RenderedBlock BlockBefore => Rendered.BlockBefore(FirstBlock)!;
   BlockContainers ContainersOfTheList => Block.Containers.WithoutTheInnermost;

   ///<summary>Without a blank line between them, the item may become a part of the text before it.</summary>
   internal SourceChange Change()
   {
      var onTheNextLine = WithTheLineBreakBefore(Rendered.Source.LineBreak);
      if(IsInsideTheItemBefore(onTheNextLine)) return onTheNextLine;

      var afterABlankLine = WithTheLineBreakBefore(new LineBreaksInside(Rendered, ContainersOfTheList).BlankLine + Rendered.Source.LineBreak);
      return IsInsideTheItemBefore(afterABlankLine) ? afterABlankLine : onTheNextLine;
   }

   bool IsInsideTheItemBefore(SourceChange change) =>
      Rendered.NumberOf(Block) is var number && number < change.After.Blocks.Count && change.After.Blocks[number].Containers.Count == Block.Containers.Count + 1;

   SourceChange WithTheLineBreakBefore(string lineBreak) =>
      SourceChange.Replace(Rendered, new SourceRange(BlockBefore.Source.End, LastBlock.Source.End), lineBreak + FirstLine() + OtherLines())
                  .WithTheCaretAt(Rendered.NumbersOf(_caret.Position));

   string FirstLine() => ContainersOfTheList.PrefixOfTheirLines + _itemBefore.PrefixOfItsOtherLines + NewItem().PrefixOfItsFirstLine + Rendered.Source[FirstBlock.Source.Start..EndOfLineAt(FirstBlock.Source.Start)];

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
      for(var startOfLine = StartOfLineAfter(FirstBlock.Source.Start); startOfLine < LastBlock.Source.End; startOfLine = StartOfLineAfter(startOfLine)) lines += Rendered.Source.LineBreak + IndentedLineAt(startOfLine);

      return lines;
   }

   string IndentedLineAt(int startOfLine)
   {
      var endOfPrefix = ContainersOfTheList.EndOfTheirPrefixes(Rendered.Source, startOfLine);
      var line = Rendered.Source[endOfPrefix..EndOfLineAt(startOfLine)];
      return Rendered.Source[startOfLine..endOfPrefix] + (line.Length == 0 ? "" : _itemBefore.PrefixOfItsOtherLines + line);
   }

   int StartOfLineAfter(int offset) => Rendered.Source.StartOfLineAfter(offset) ?? LastBlock.Source.End;
   int EndOfLineAt(int offset) => Math.Min(LastBlock.Source.End, Rendered.Source.EndOfLineAt(offset));
}
