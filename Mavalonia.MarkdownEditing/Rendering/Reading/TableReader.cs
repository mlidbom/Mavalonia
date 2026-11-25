using Markdig.Syntax;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;
using MarkdigTable = Markdig.Extensions.Tables.Table;
using MarkdigTableCell = Markdig.Extensions.Tables.TableCell;
using MarkdigTableRow = Markdig.Extensions.Tables.TableRow;
using Table = Mavalonia.MarkdownEditing.Rendering.Blocks.Table;
using TableCell = Mavalonia.MarkdownEditing.Rendering.Blocks.TableCell;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Reads a table.</summary>
class TableReader
{
   readonly string _markdown;
   readonly BlockContainers _containers;

   internal TableReader(string markdown, BlockContainers containers)
   {
      _markdown = markdown;
      _containers = containers;
   }

   int _endOfThePreviousCell;

   internal IReadOnlyList<TableCell> Read(MarkdigTable markdigTable)
   {
      var rows = markdigTable.Cast<MarkdigTableRow>().ToList();
      var table = new Table(new SourceRange(markdigTable.Span.Start, Math.Max(EndOfTheLineUnderTheHeader(), _markdown.EndOfLineAt(rows[^1].Span.Start))));

      foreach(var (number, row) in rows.Index()) ReadRow(table, row, number);

      return table.Cells;

      int EndOfTheLineUnderTheHeader() => _markdown.EndOfLineAt(_markdown.StartOfLineAfter(rows[0].Span.Start)!.Value);
   }

   void ReadRow(Table table, MarkdigTableRow row, int number)
   {
      _endOfThePreviousCell = _markdown.StartOfLineAt(row.Span.Start);

      foreach(var (column, cell) in row.Cast<MarkdigTableCell>().Index())
      {
         var text = TextOf(cell) ?? RenderedText.EmptyAt(StartOfAnEmptyCell());
         table.Add(new TableCell(_containers, text, table, number, column, row.IsHeader));
         _endOfThePreviousCell = text.Source.End;
      }
   }

   RenderedText? TextOf(MarkdigTableCell cell) =>
      cell.OfType<ParagraphBlock>().Select(it => new TextReader(_markdown, _containers).Read(it.Inline, includeTrailingSpaces: false)).FirstOrDefault();

   ///<summary>After the <c>|</c> that starts the cell, and the space that usually follows it.</summary>
   int StartOfAnEmptyCell()
   {
      var startOfCell = _markdown.IndexOf('|', _endOfThePreviousCell) + 1;

      return startOfCell < _markdown.Length && _markdown[startOfCell] == ' ' ? startOfCell + 1 : startOfCell;
   }
}
