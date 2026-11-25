using Avalonia;
using Avalonia.Media;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;

abstract partial class ContainerDecoration
{
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
}
