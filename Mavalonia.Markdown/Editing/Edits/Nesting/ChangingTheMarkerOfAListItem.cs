using Mavalonia.Markdown.Editing.Changes;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;
using Mavalonia.Markdown.Rendering.Containers;

namespace Mavalonia.Markdown.Editing.Edits.Nesting;

///<summary>Writes a list item with another marker, such as a number in place of a bullet.</summary>
///<remarks>The item's other lines are indented as far as its text is after the new marker, so that what is in the item, such as a list inside it, stays in it.</remarks>
class ChangingTheMarkerOfAListItem
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedBlock _firstBlock;
   readonly ListItem _item;
   readonly ListItem _newItem;

   ///<param name="rendered">What the item is in.</param>
   ///<param name="firstBlock">The first block of the item.</param>
   ///<param name="newItem">The item as it's written afterwards.</param>
   internal ChangingTheMarkerOfAListItem(RenderedMarkdown rendered, RenderedBlock firstBlock, ListItem newItem)
   {
      _rendered = rendered;
      _firstBlock = firstBlock;
      _item = (ListItem)firstBlock.Containers.Innermost!;
      _newItem = newItem;
   }

   string Source => _rendered.Source;
   BlockContainers ContainersOfTheList => _firstBlock.Containers.WithoutTheInnermost;
   RenderedBlock LastBlock => _rendered.Blocks.Last(it => it.IsInside(_item));
   int StartOfTheMarker => ContainersOfTheList.EndOfTheirPrefixes(Source, Source.StartOfLineAt(_firstBlock.Source.Start));

   internal SourceChange Change() =>
      SourceChange.Replace(_rendered, new SourceRange(StartOfTheMarker, LastBlock.Source.End), _newItem.PrefixOfItsFirstLine + Source[_firstBlock.Source.Start..EndOfLineAt(_firstBlock.Source.Start)] + OtherLines());

   ///<summary>The lines after the one with the marker, each with the line break before it.</summary>
   string OtherLines()
   {
      var lines = "";
      for(var startOfLine = StartOfLineAfter(_firstBlock.Source.Start); startOfLine < LastBlock.Source.End; startOfLine = StartOfLineAfter(startOfLine)) lines += Source.LineBreak + LineAt(startOfLine);

      return lines;
   }

   ///<summary>The line, indented as far as the new item needs where it's indented as far as the item does. A blank line, or one that continues a paragraph without being indented, as it is.</summary>
   string LineAt(int startOfLine)
   {
      var endOfThePrefixesOfTheList = ContainersOfTheList.EndOfTheirPrefixes(Source, startOfLine);
      return _item.EndOfItsPrefix(Source, endOfThePrefixesOfTheList) is {} endOfThePrefixOfTheItem
                ? Source[startOfLine..endOfThePrefixesOfTheList] + _newItem.PrefixOfItsOtherLines + Source[endOfThePrefixOfTheItem..EndOfLineAt(startOfLine)]
                : Source[startOfLine..EndOfLineAt(startOfLine)];
   }

   int StartOfLineAfter(int offset) => Source.StartOfLineAfter(offset) ?? LastBlock.Source.End;
   int EndOfLineAt(int offset) => Math.Min(LastBlock.Source.End, Source.EndOfLineAt(offset));
}
