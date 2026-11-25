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

   internal TableReader(string markdown, BlockContainers containers)
   {
      _markdown = markdown;
      _containers = containers;
   }

   internal IReadOnlyList<RenderedBlock> Read(MarkdigTable markdigTable)
   {
      var rows = markdigTable.Cast<MarkdigTableRow>().ToList();
      var startOfHeader = _markdown.StartOfLineAt(StartOfTextIn(rows[0]) ?? markdigTable.Span.Start);
      var table = new Table(new SourceRange(StartOfTable(), _markdown.EndOfLineAt(StartOfLine(startOfHeader, lineNumber: Math.Max(1, rows.Count)))));

      foreach(var (number, row) in rows.Index()) ReadRow(table, row, number, StartOfLine(startOfHeader, lineNumber: number == 0 ? 0 : number + 1));

      return AreOneAfterTheOther(table.Cells) ? table.Cells : [new SourceBlock(_containers, new TextReader(_markdown, _containers).ReadAsSource(table.Source))];

      int StartOfTable() => _containers.EndOfTheirPrefixes(_markdown, startOfHeader) is var end && _markdown.IndexOf('|', end) is var pipe && StartOfTextIn(rows[0]) is {} text ? Math.Min(pipe, text) : _markdown.IndexOf('|', startOfHeader);
   }

   bool AreOneAfterTheOther(IReadOnlyList<TableCell> cells) =>
      cells.Zip(cells.Skip(1)).All(it => it.First.Text.Source.End <= it.Second.Text.Source.Start) && cells.All(it => it.Text.Source.End <= _markdown.EndOfLineAt(it.Text.Source.Start));

   static int? StartOfTextIn(MarkdigTableRow row) => row.Cast<MarkdigTableCell>().SelectMany(it => it.OfType<ParagraphBlock>()).Select(it => it.Inline?.FirstChild?.Span.Start).FirstOrDefault(it => it is not null);

   int StartOfLine(int startOfFirst, int lineNumber)
   {
      var startOfLine = startOfFirst;
      for(var number = 0; number < lineNumber; number++) startOfLine = _markdown.StartOfLineAfter(startOfLine)!.Value;
      return startOfLine;
   }

   void ReadRow(Table table, MarkdigTableRow row, int number, int startOfLine)
   {
      var endOfPreviousCell = startOfLine;

      foreach(var (column, cell) in row.Cast<MarkdigTableCell>().Index())
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
}
