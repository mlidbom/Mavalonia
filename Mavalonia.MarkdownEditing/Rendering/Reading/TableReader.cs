using Markdig.Syntax;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;
using MarkdigTable = Markdig.Extensions.Tables.Table;
using MarkdigTableCell = Markdig.Extensions.Tables.TableCell;
using MarkdigTableRow = Markdig.Extensions.Tables.TableRow;
using RenderedBlock = Mavalonia.MarkdownEditing.Rendering.Blocks.RenderedBlock;
using SourceBlock = Mavalonia.MarkdownEditing.Rendering.Blocks.SourceBlock;
using Table = Mavalonia.MarkdownEditing.Rendering.Blocks.Table;
using TableCell = Mavalonia.MarkdownEditing.Rendering.Blocks.TableCell;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Reads a table. One whose cells can't be told apart, since the markup in one of them goes on outside it, is read as source.</summary>
///<remarks>
/// Where an empty cell is, is worked out here, since Markdig doesn't say. Each row of a table is on a line of its own, with the line that separates the header from the others after the first.
/// </remarks>
class TableReader
{
   readonly string _markdown;
   readonly BlockContainers _containers;
   readonly IReadOnlyList<MarkdigTableRow> _rows;
   readonly int _startOfHeader;

   internal TableReader(string markdown, BlockContainers containers, MarkdigTable table)
   {
      _markdown = markdown;
      _containers = containers;
      _rows = [..table.Cast<MarkdigTableRow>()];
      _startOfHeader = markdown.StartOfLineAt(StartOfTheTextOfTheHeader ?? table.Span.Start);
   }

   internal IReadOnlyList<RenderedBlock> Read()
   {
      var table = new Table(new SourceRange(StartOfTable(), EndOfTable()));
      foreach(var (number, row) in _rows.Index()) ReadRow(table, row, number);

      return AreOneAfterTheOther(table.Cells) ? table.Cells : [new SourceBlock(_containers, new TextReader(_markdown, _containers).ReadAsSource(table.Source))];
   }

   int? StartOfTheTextOfTheHeader => CellsOf(_rows[0]).SelectMany(it => it.OfType<ParagraphBlock>()).Select(it => it.Inline?.FirstChild?.Span.Start).FirstOrDefault(it => it is not null);

   int StartOfTable()
   {
      var firstPipe = _markdown.IndexOf('|', _startOfHeader);
      return StartOfTheTextOfTheHeader is {} startOfText ? Math.Min(firstPipe, startOfText) : firstPipe;
   }

   int EndOfTable() => _markdown.EndOfLineAt(StartOfLine(_rows.Count));

   int StartOfRow(int number) => StartOfLine(number == 0 ? 0 : number + 1);

   ///<summary>Where line number <paramref name="lineNumber"/> of the table starts. The header is on line 0, and the line that separates it from the other rows is line 1.</summary>
   int StartOfLine(int lineNumber)
   {
      var startOfLine = _startOfHeader;
      //bug: throws when there are too few lines, as in the markdown "*[|", "]()-|-", on a line each, of which Markdig says that the header is on the last line.
      for(var number = 0; number < lineNumber; number++) startOfLine = _markdown.StartOfLineAfter(startOfLine)!.Value;
      return startOfLine;
   }

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
      cell.OfType<ParagraphBlock>().Select(it => new TextReader(_markdown, _containers).Read(it.Inline, includeTrailingSpaces: false)).FirstOrDefault();

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
