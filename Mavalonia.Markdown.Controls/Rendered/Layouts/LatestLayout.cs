using Avalonia;
using Mavalonia.Markdown.Controls.Rendered.Layouts.Texts;
using Mavalonia.Markdown.Rendering;

namespace Mavalonia.Markdown.Controls.Rendered.Layouts;

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

   ///<summary>Whether what is laid out, if anything, is laid out to look otherwise than as <paramref name="appearance"/> says.</summary>
   internal bool LooksOtherwiseThan(MarkdownAppearance appearance) => _appearance is not null && _appearance != appearance;

   ///<summary>To be done when some texts look otherwise than when they were laid out. The layouts of the others are used again.</summary>
   internal void ForgetAllButTheTexts() => _layout = null;

   ///<summary>To be done when the appearance or the padding changes.</summary>
   internal void ForgetAll()
   {
      _texts?.Dispose();
      _texts = null;
      _appearance = null;
      _layout = null;
   }
}
