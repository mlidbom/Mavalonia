using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Reading;

namespace Mavalonia.Markdown.Html;

static partial class HtmlOfMarkdown
{
   ///<summary>Writes HTML inside text as text, but for line breaks and comments where they are in markdown that is only read.</summary>
   class WriterOfInlineHtml : HtmlObjectRenderer<HtmlInline>
   {
      readonly MarkdownDialect _dialect;

      internal WriterOfInlineHtml(MarkdownDialect dialect) => _dialect = dialect;

      protected override void Write(HtmlRenderer renderer, HtmlInline html)
      {
         if(_dialect == MarkdownDialect.ForReading && html.IsAComment) return;

         if(_dialect == MarkdownDialect.ForReading && html.IsALineBreak) renderer.Write("<br />");
         else renderer.WriteEscape(html.Tag);
      }
   }
}
