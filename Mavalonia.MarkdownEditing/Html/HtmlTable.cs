using System.Globalization;
using AngleSharp.Dom;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Html;

///<summary>Reads the cells of a table. A table that only lays out what is on a page isn't read as one.</summary>
static class HtmlTable
{
   const int MostColumnsThatACellSpans = 20;

   ///<summary>The cells, row by row. None when the table has nothing that only a table can show.</summary>
   internal static IReadOnlyList<IReadOnlyList<FormattedText>> RowsOf(IElement table, TextFormatting formatting)
   {
      if(table.QuerySelector("table") is not null) return [];

      IReadOnlyList<IReadOnlyList<FormattedText>> rows = [..table.QuerySelectorAll("tr").Where(it => !it.IsHidden).Select(it => CellsOf(it, formatting)).Where(it => it.Any(cell => cell.Length > 0))];
      return rows.Any(it => it.Count > 1) ? rows : [];
   }

   static IReadOnlyList<FormattedText> CellsOf(IElement row, TextFormatting formatting) =>
      [..row.Children.Where(it => it is { LocalName: "td" or "th", IsHidden: false }).SelectMany(it => Enumerable.Repeat(FormattedText.Empty, ColumnsSpannedBy(it) - 1).Prepend(TextOf(it, formatting)))];

   static FormattedText TextOf(IElement cell, TextFormatting formattingOfTheTable)
   {
      var text = HtmlOnOneLine.TextIn(cell, cell.Format(formattingOfTheTable));
      return cell.LocalName == "th" ? text.WithoutBold : text; // The cells of the header are bold as they are.
   }

   static int ColumnsSpannedBy(IElement cell) => int.TryParse(cell.GetAttribute("colspan"), CultureInfo.InvariantCulture, out var count) ? Math.Clamp(count, 1, MostColumnsThatACellSpans) : 1;
}
