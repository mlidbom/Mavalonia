using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Controls.Rendered.Layouts.Texts;

///<summary>The box of a task, ticked or not.</summary>
///<remarks>
/// The chars of <see cref="BoxOfATask"/> come from different fonts, one of them as a colour emoji.
/// </remarks>
class DrawnBox : DrawableTextRun
{
   readonly bool _isTicked;
   readonly double _side;

   internal DrawnBox(char box, TextRunProperties properties)
   {
      _isTicked = box == BoxOfATask.Ticked;
      _side = properties.FontRenderingEmSize * 0.9;
      Properties = properties;
      Text = box.ToString(CultureInfo.InvariantCulture).AsMemory();
   }

   public override TextRunProperties Properties { get; }
   public override ReadOnlyMemory<char> Text { get; }
   public override int Length => 1;
   public override Size Size => new(_side, _side);
   public override double Baseline => _side * 0.85;

   public override void Draw(DrawingContext drawingContext, Point origin)
   {
      var pen = new Pen(Properties.ForegroundBrush, _side / 11);
      drawingContext.DrawRectangle(pen, new Rect(origin, Size).Deflate(_side / 11), (float)(_side / 6));
      if(!_isTicked) return;

      drawingContext.DrawLine(pen, Inside(0.26, 0.52), Inside(0.44, 0.70));
      drawingContext.DrawLine(pen, Inside(0.44, 0.70), Inside(0.76, 0.30));

      Point Inside(double x, double y) => origin + new Point(_side * x, _side * y);
   }
}
