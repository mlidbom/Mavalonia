using Markdig.Syntax;
using Mavalonia.Markdown.Rendering.Containers;
using Mavalonia.Markdown.Rendering.Texts;
using MarkdigTable = Markdig.Extensions.Tables.Table;
using MarkdigTableCell = Markdig.Extensions.Tables.TableCell;
using MarkdigTableRow = Markdig.Extensions.Tables.TableRow;
using RenderedBlock = Mavalonia.Markdown.Rendering.Blocks.RenderedBlock;
using SourceBlock = Mavalonia.Markdown.Rendering.Blocks.SourceBlock;
using Table = Mavalonia.Markdown.Rendering.Blocks.Table;
using TableCell = Mavalonia.Markdown.Rendering.Blocks.TableCell;

namespace Mavalonia.Markdown.Rendering.Reading;

///<summary>Reads a table. One whose cells can't be told apart, since the markup in one of them goes on outside it, or which has no cells, is read as source.</summary>
///<remarks>
/// Where an empty cell is, is worked out here, since Markdig doesn't say. Each row of a table is on a line of its own, with the line that separates the header from the others after the first.
/// </remarks>
class TableReader
{
   const string CharactersUnderAHeader = "|:- \t";

   readonly string _markdown;
   readonly BlockContainers _containers;
   readonly IReadOnlyList<MarkdigTableRow> _rows;
   readonly Func<TextReader> _readerOfText;

   ///<summary>Where the lines of the table start: first that of the header, then the line that separates it from the other rows, then theirs.</summary>
   readonly IReadOnlyList<int> _startsOfLines;

   internal TableReader(string markdown, BlockContainers containers, MarkdigTable table, Func<TextReader> readerOfText)
   {
      _readerOfText = readerOfText;
      _markdown = markdown;
      _containers = containers;
      _rows = [..table.Cast<MarkdigTableRow>()];
      _startsOfLines = [..markdown.StartsOfLinesFrom(markdown.StartOfLineAt(StartOfTheTextOfTheHeader ?? table.Span.Start)).Take(_rows.Count + 1)];
   }

   ///<summary>Null when the table isn't where Markdig says it starts, as when a link goes on into its header, so that its source isn't known.</summary>
   internal IReadOnlyList<RenderedBlock>? Read()
   {
      if(!IsWhereMarkdigSays) return null;

      var table = new Table(new SourceRange(StartOfTable(), EndOfTable()));
      foreach(var (number, row) in _rows.Index()) ReadRow(table, row, number);

      return table.Cells.Count > 0 && AreOneAfterTheOther(table.Cells) ? table.Cells : [new SourceBlock(_containers, _readerOfText().ReadAsSource(table.Source))];
   }

   int? StartOfTheTextOfTheHeader => CellsOf(_rows[0]).SelectMany(it => it.OfType<ParagraphBlock>()).Select(it => it.Inline?.FirstChild?.Span.Start).FirstOrDefault(it => it is not null);

   int StartOfTable()
   {
      var firstPipe = _markdown.IndexOf('|', _startsOfLines[0]);
      return StartOfTheTextOfTheHeader is {} startOfText ? Math.Min(firstPipe, startOfText) : firstPipe;
   }

   int EndOfTable() => _markdown.EndOfLineAt(_startsOfLines[^1]);

   int StartOfRow(int number) => _startsOfLines[number == 0 ? 0 : number + 1];

   bool IsWhereMarkdigSays => _startsOfLines.Count == _rows.Count + 1 && IsALineUnderAHeader(_startsOfLines[1]);

   bool IsALineUnderAHeader(int startOfLine) =>
      _markdown.AsSpan(_containers.EndOfTheirPrefixes(_markdown, startOfLine).._markdown.EndOfLineAt(startOfLine)).IndexOfAnyExcept(CharactersUnderAHeader) < 0;

   static IEnumerable<MarkdigTableCell> CellsOf(MarkdigTableRow row) => row.Cast<MarkdigTableCell>();

   void ReadRow(Table table, MarkdigTableRow row, int number)
   {
      var endOfPreviousCell = StartOfRow(number);

      foreach(var (column, cell) in CellsOf(row).Index())
      {
         var text = TextOf(cell) ?? TextOfAnEmptyCellAfter(endOfPreviousCell);
         if(text is null) return; // Markdig adds cells to a row that has too few. They aren't in the markdown.

         table.Add(new TableCell(_containers, text, table, number, column, row.IsHeader));
         endOfPreviousCell = text.Source.End;
      }
   }

   RenderedText? TextOf(MarkdigTableCell cell) =>
      cell.OfType<ParagraphBlock>().Select(it => _readerOfText().Read(it.Inline, includeTrailingSpaces: false)).FirstOrDefault();

   ///<summary>Starts after the <c>|</c> that starts the cell, and the space that usually follows it. Null when the line has no more cells.</summary>
   RenderedText? TextOfAnEmptyCellAfter(int endOfPreviousCell)
   {
      var endOfLine = _markdown.EndOfLineAt(endOfPreviousCell);
      var startOfCell = _markdown.IndexOf('|', endOfPreviousCell, endOfLine - endOfPreviousCell) + 1;
      if(startOfCell == 0 || startOfCell == endOfLine) return null;

      return RenderedText.EmptyAt(_markdown[startOfCell] == ' ' ? startOfCell + 1 : startOfCell);
   }

   bool AreOneAfterTheOther(IReadOnlyList<TableCell> cells) =>
      cells.Zip(cells.Skip(1)).All(it => it.First.Text.Source.End <= it.Second.Text.Source.Start) && cells.All(it => it.Text.Source.End <= _markdown.EndOfLineAt(it.Text.Source.Start));
}
