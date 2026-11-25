using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;

///<summary>What shows that blocks are inside a container.</summary>
abstract class ContainerDecoration
{
   internal abstract Rect Bounds { get; }

   internal abstract void Draw(DrawingContext context);

   ///<summary>The bar along a quote.</summary>
   internal class QuoteBar : ContainerDecoration
   {
      readonly IBrush _brush;

      internal QuoteBar(Rect bounds, IBrush brush)
      {
         Bounds = bounds;
         _brush = brush;
      }

      internal override Rect Bounds { get; }

      internal override void Draw(DrawingContext context) => context.FillRectangle(_brush, Bounds);
   }

   ///<summary>The bullet or number of a list item.</summary>
   internal class ListMarker : ContainerDecoration
   {
      readonly TextLayout _text;

      internal ListMarker(TextLayout text, Point topRight)
      {
         _text = text;
         Bounds = new Rect(topRight.X - _text.Width, topRight.Y, _text.Width, _text.Height);
      }

      internal override Rect Bounds { get; }

      internal override void Draw(DrawingContext context) => _text.Draw(context, Bounds.TopLeft);
   }
}
