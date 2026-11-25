using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Writing;

///<summary>Writes a table as markdown. Its first row becomes the header.</summary>
///<remarks>
/// What looks right in a cell by itself may not in its row: a backtick in one cell and another in the next make code of what is between them. So it's the whole table that is verified.
/// </remarks>
class MarkdownOfATable
{
   readonly IReadOnlyList<IReadOnlyList<FormattedText>> _rows;
   readonly int _columnCount;

   internal MarkdownOfATable(IReadOnlyList<IReadOnlyList<FormattedText>> rows)
   {
      _columnCount = rows.Max(it => it.Count);
      _rows = [..rows.Select(row => (IReadOnlyList<FormattedText>)[..row, ..Enumerable.Repeat(FormattedText.Empty, _columnCount - row.Count)])];
   }

   internal IReadOnlyList<string> Lines => WaysToWriteACell.Select(LinesWithCells).FirstOrDefault(LooksRight) ?? LinesWithCells(WithoutFormatting);

   static IEnumerable<Func<FormattedText, string>> WaysToWriteACell => [MarkdownOfText.InACell, it => InlineMarkdownWriter.PreferredWayToWrite(it.WithPunctuationEscaped)];

   static string WithoutFormatting(FormattedText cell) => cell.WithoutFormatting.WithPunctuationEscaped.MarkdownWithoutFormatting;

   IReadOnlyList<string> LinesWithCells(Func<FormattedText, string> markdownOf)
   {
      return [LineOf(_rows[0]), "|" + string.Concat(Enumerable.Repeat("---|", _columnCount)), .._rows.Skip(1).Select(LineOf)];

      string LineOf(IReadOnlyList<FormattedText> cells) => "|" + string.Concat(cells.Select(it => $" {markdownOf(it)} |"));
   }

   bool LooksRight(IReadOnlyList<string> lines)
   {
      IReadOnlyList<FormattedText> cells = [.._rows.SelectMany(it => it)];
      var blocks = RenderedMarkdown.Of(string.Join('\n', lines)).Blocks;
      return blocks.Count == cells.Count && blocks.Zip(cells).All(it => it.First is TableCell && it.Second.LooksLike(it.First.Text));
   }
}
