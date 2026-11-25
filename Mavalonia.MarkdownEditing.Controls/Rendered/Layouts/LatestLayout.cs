using Avalonia;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;

///<summary>Lays out what is rendered, and keeps the layout, which is used again for as long as it is of what is rendered.</summary>
class LatestLayout
{
   TextLayouts? _texts;

   ///<summary>The layout that was made last. Null before the first is made, and after it is forgotten.</summary>
   internal DocumentLayout? Layout { get; private set; }

   internal bool IsOf(RenderedMarkdown rendered, double width) => Layout?.IsOf(rendered, width) == true;

   internal DocumentLayout LayOut(RenderedMarkdown rendered, MarkdownAppearance appearance, double width, Thickness padding)
   {
      if(Layout?.IsOf(rendered, width) == true) return Layout;

      _texts ??= new TextLayouts(appearance);
      return Layout = _texts.Use(() => DocumentLayoutBuilder.LayOut(rendered, appearance, _texts, width, padding));
   }

   ///<summary>To be done when the appearance or the padding changes.</summary>
   internal void Forget()
   {
      _texts?.Dispose();
      _texts = null;
      Layout = null;
   }
}
