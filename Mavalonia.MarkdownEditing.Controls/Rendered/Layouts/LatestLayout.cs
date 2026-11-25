using Avalonia;
using Mavalonia.MarkdownEditing.Controls.Rendered.Layouts.Texts;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;

///<summary>Lays out what is rendered, and keeps the layout, which is used again for as long as it is of what is rendered.</summary>
class LatestLayout
{
   TextLayouts? _texts;
   MarkdownAppearance? _appearance;

   ///<summary>Null before the first layout is made, and after it is forgotten.</summary>
   DocumentLayout? _layout;

   internal bool IsOf(RenderedMarkdown rendered, double width) => _layout?.IsOf(rendered, width) == true;

   internal DocumentLayout LayOut(RenderedMarkdown rendered, Func<MarkdownAppearance> appearance, double width, Thickness padding)
   {
      if(_layout?.IsOf(rendered, width) == true) return _layout;

      _appearance ??= appearance();
      _texts ??= new TextLayouts(_appearance);
      return _layout = _texts.Use(() => DocumentLayoutBuilder.LayOut(rendered, _appearance, _texts, width, padding));
   }

   ///<summary>To be done when some texts look otherwise than when they were laid out. The layouts of the others are used again.</summary>
   internal void LayOutAgain() => _layout = null;

   ///<summary>To be done when the appearance or the padding changes.</summary>
   internal void Forget()
   {
      _texts?.Dispose();
      _texts = null;
      _appearance = null;
      _layout = null;
   }
}
