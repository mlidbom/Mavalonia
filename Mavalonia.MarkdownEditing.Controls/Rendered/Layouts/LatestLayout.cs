using Avalonia;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;

///<summary>Lays out what is rendered, and keeps the layout, which is used again for as long as it is of what is rendered.</summary>
class LatestLayout
{
   TextLayouts? _texts;

   ///<summary>Null before the first layout is made, and after it is forgotten.</summary>
   DocumentLayout? _layout;

   internal bool IsOf(RenderedMarkdown rendered, double width) => _layout?.IsOf(rendered, width) == true;

   internal DocumentLayout LayOut(RenderedMarkdown rendered, MarkdownAppearance appearance, double width, Thickness padding)
   {
      if(_layout?.IsOf(rendered, width) == true) return _layout;

      _texts ??= new TextLayouts(appearance);
      return _layout = _texts.Use(() => DocumentLayoutBuilder.LayOut(rendered, appearance, _texts, width, padding));
   }

   ///<summary>To be done when some texts look otherwise than when they were laid out. The layouts of the others are used again.</summary>
   internal void LayOutAgain() => _layout = null;

   ///<summary>To be done when the appearance or the padding changes.</summary>
   internal void Forget()
   {
      _texts?.Dispose();
      _texts = null;
      _layout = null;
   }
}
