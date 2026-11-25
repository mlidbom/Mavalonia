using Avalonia;
using Avalonia.Media.TextFormatting;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;

///<summary>Lays out a table: each column as wide as its widest cell, or, when the table would be too wide, in proportion narrower.</summary>
class TableLayout
{
   readonly Table _table;
   readonly MarkdownAppearance _appearance;
   readonly TextLayouts _texts;
   readonly List<BlockLayout> _cells = [];

   internal TableLayout(Table table, MarkdownAppearance appearance, TextLayouts texts, Point topLeft, double widthAvailable)
   {
      _table = table;
      _appearance = appearance;
      _texts = texts;
      Bottom = topLeft.Y;

      var widths = WidthsOfColumns(widthAvailable);
      foreach(var row in table.Cells.GroupBy(it => it.Row)) LayOut([..row], topLeft.X, widths);
   }

   internal IReadOnlyList<BlockLayout> Cells => _cells;

   internal double Bottom { get; private set; }

   double HorizontalPadding => _appearance.Scaled(9);
   double VerticalPadding => _appearance.Scaled(4);

   IReadOnlyList<double> WidthsOfColumns(double widthAvailable)
   {
      var wanted = Enumerable.Range(0, _table.ColumnCount).Select(WidthWantedBy).ToList();
      var shrinkage = Math.Min(1, widthAvailable / wanted.Sum());

      return [..wanted.Select(it => it * shrinkage)];

      double WidthWantedBy(int column) => _table.Cells.Where(it => it.Column == column).Max(it => _texts.Of(it, double.PositiveInfinity).WidthIncludingTrailingWhitespace) + 2 * HorizontalPadding + 1;
   }

   void LayOut(IReadOnlyList<TableCell> row, double left, IReadOnlyList<double> widths)
   {
      var texts = row.Select(it => _texts.Of(it, Math.Max(1, widths[it.Column] - 2 * HorizontalPadding))).ToList();
      var height = texts.Max(it => it.Height) + 2 * VerticalPadding;

      foreach(var (cell, text) in row.Zip(texts)) _cells.Add(LayOut(cell, text, new Rect(left + widths.Take(cell.Column).Sum(), Bottom, widths[cell.Column], height)));

      Bottom += height;
   }

   BlockLayout LayOut(TableCell cell, TextLayout text, Rect bounds) => new(cell, bounds, text, bounds.TopLeft + new Point(HorizontalPadding, VerticalPadding), _appearance);
}
