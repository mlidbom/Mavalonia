using Avalonia;
using Avalonia.Media;

namespace Mavalonia.Markdown.Controls.Rendered.Layouts;

///<summary>What shows that blocks are inside a container.</summary>
abstract partial class ContainerDecoration
{
   internal abstract Rect Bounds { get; }

   internal abstract void Draw(DrawingContext context);
}
