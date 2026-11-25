using Markdig.Syntax;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Reads the marker of a list item, and from it where the blocks in the item are. Those are read as blocks anywhere are.</summary>
class ListItemReader
{
   readonly string _markdown;
   readonly ListItemBlock _item;
   readonly int? _number;
   readonly PlaceOfBlocks _place;
   readonly string _marker;

   internal ListItemReader(string markdown, ListItemBlock item, int? number, PlaceOfBlocks place)
   {
      _markdown = markdown;
      _item = item;
      _number = number;
      _place = place;
      _marker = ReadMarker();
   }

   string ReadMarker()
   {
      var end = _item.Span.Start;
      while(char.IsAsciiDigit(_markdown[end])) end++;
      return _markdown[_item.Span.Start..(end + 1)];
   }

   int EndOfMarker => _item.Span.Start + _marker.Length;
   int EndOfLine => _markdown.EndOfLineAt(EndOfMarker);

   ///<summary>Where the blocks in the item are.</summary>
   internal PlaceOfBlocks PlaceOfItsBlocks => InTheItem(ColumnOfItsFirstBlock());

   ///<summary>Reads an item that has no blocks in it.</summary>
   internal Paragraph ReadEmpty() =>
      EndOfLine == EndOfMarker
         ? new Paragraph(_place.Containers, new TextReader(_markdown, _place.Containers).ReadAsText(new SourceRange(_item.Span.Start, EndOfMarker))) // So far, it may be the start of anything, such as of -5.
         : Paragraph.EmptyAt(EndOfLine, InTheItem(ColumnAfterTheSpaceAfterTheMarker).Containers);

   PlaceOfBlocks InTheItem(int column) => _place.In(new ListItem(_marker, _number, width: column - ColumnThatItIsIndentedFrom()), column);

   int ColumnOfItsFirstBlock() => _item[0].Span.Start <= EndOfLine ? _markdown.ColumnAt(_item[0].Span.Start) : ColumnAfterTheSpaceAfterTheMarker;

   int ColumnAfterTheSpaceAfterTheMarker => _markdown.ColumnAt(EndOfMarker) + 1;

   ///<summary>On a line that isn't the first one inside a container, text may start at another column than on the first.</summary>
   int ColumnThatItIsIndentedFrom()
   {
      var startOfLine = _markdown.StartOfLineAt(_item.Span.Start);
      var isOnALineWithAllPrefixes = _place.Containers.OfTheLineAt(_markdown, startOfLine).Count == _place.Containers.Count;
      return isOnALineWithAllPrefixes ? _markdown.ColumnAt(_place.Containers.EndOfTheirPrefixes(_markdown, startOfLine)) : _place.Column;
   }
}
