using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

///<summary>Makes a list item an item of a list inside the item before it.</summary>
class IndentingAListItem
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedPosition _position;

   internal IndentingAListItem(RenderedMarkdown rendered, RenderedPosition position)
   {
      _rendered = rendered;
      _position = position;
   }

   RenderedBlock Block => _position.Block;

   ///<summary>Null when the caret isn't in a list item that has another before it.</summary>
   internal SourceChange? Change() => Block.Containers.Innermost is ListItem item && BlockBefore(item) is {} previous && ItemBefore(previous) is {} itemBefore ? Change(item, itemBefore, previous) : null;

   RenderedBlock? BlockBefore(ListItem item) => _rendered.BlockBefore(FirstBlockOf(item));

   RenderedBlock FirstBlockOf(ListItem item) => _rendered.Blocks.First(it => it.Containers.OutermostFirst.Contains(item));
   RenderedBlock LastBlockOf(ListItem item) => _rendered.Blocks.Last(it => it.Containers.OutermostFirst.Contains(item));

   BlockContainers Outer => Block.Containers.WithoutTheInnermost;

   ListItem? ItemBefore(RenderedBlock previous) =>
      previous.Containers.SharedWith(Block.Containers).Count == Outer.Count ? previous.Containers.OutermostFirst.ElementAtOrDefault(Outer.Count) as ListItem : null;

   SourceChange Change(ListItem item, ListItem itemBefore, RenderedBlock previous)
   {
      var first = FirstBlockOf(item);
      var startOfFirstLine = _rendered.Source.StartOfLineAt(first.Source.Start);
      var range = new SourceRange(previous.Source.End, LastBlockOf(item).Source.End);
      var itemInside = NewItemInside(itemBefore, previous, item);
      var linesAfterTheMarker = Indented(new SourceRange(_rendered.Source.StartOfLineAfter(startOfFirstLine) ?? range.End, range.End), itemBefore);
      var firstLine = Outer.PrefixOfTheirLines + itemBefore.PrefixOfItsOtherLines + itemInside.PrefixOfItsFirstLine + _rendered.Source[first.Source.Start..Math.Min(range.End, _rendered.Source.EndOfLineAt(first.Source.Start))];

      var changes = new[] { _rendered.Source.LineBreak, new Separator(_rendered, Outer).BlankLine + _rendered.Source.LineBreak }
                   .Select(it => SourceChange.Replace(_rendered, range, it + firstLine + linesAfterTheMarker)).ToList();

      return (changes.FirstOrDefault(IsInsideTheItemBefore) ?? changes[0]).WithTheCaretIn(_rendered.NumberOf(Block), _position.Index);

      bool IsInsideTheItemBefore(SourceChange change) =>
         _rendered.NumberOf(Block) is var number && number < change.After.Blocks.Count && change.After.Blocks[number].Containers.Count == Block.Containers.Count + 1;
   }

   ///<summary>The next item of the list that is inside <paramref name="itemBefore"/> already, or the first item of a new one.</summary>
   static ListItem NewItemInside(ListItem itemBefore, RenderedBlock previous, ListItem item)
   {
      var itemInsideAlready = previous.Containers.OutermostFirst.SkipWhile(it => it != itemBefore).Skip(1).FirstOrDefault() as ListItem;
      return itemInsideAlready?.NewItemAfter() ?? item.AsTheFirstOfItsList();
   }

   ///<summary>The lines in <paramref name="range"/>, each as far in as the text of <paramref name="itemBefore"/>, with the line break before them.</summary>
   string Indented(SourceRange range, ListItem itemBefore)
   {
      var text = "";
      for(var startOfLine = range.Start; startOfLine < range.End; startOfLine = _rendered.Source.StartOfLineAfter(startOfLine) ?? range.End)
      {
         var endOfPrefix = Outer.EndOfTheirPrefixes(_rendered.Source, startOfLine);
         var line = _rendered.Source[endOfPrefix..Math.Min(range.End, _rendered.Source.EndOfLineAt(startOfLine))];
         text += _rendered.Source.LineBreak + _rendered.Source[startOfLine..endOfPrefix] + (line.Length == 0 ? "" : itemBefore.PrefixOfItsOtherLines + line);
      }

      return text;
   }
}
