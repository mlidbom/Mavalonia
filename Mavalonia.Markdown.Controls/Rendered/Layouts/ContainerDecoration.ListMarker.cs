using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace Mavalonia.Markdown.Controls.Rendered.Layouts;

abstract partial class ContainerDecoration
{
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
