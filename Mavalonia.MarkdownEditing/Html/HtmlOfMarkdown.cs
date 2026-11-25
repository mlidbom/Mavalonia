using Markdig.Extensions.Alerts;
using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Reading;

namespace Mavalonia.MarkdownEditing.Html;

///<summary>Writes markdown as HTML that looks as the markdown does where it's shown. HTML that is shown as its source is text in what is written.</summary>
static partial class HtmlOfMarkdown
{
   ///<summary>Empty when <paramref name="markdown"/> is plain text, and when it can't be read.</summary>
   internal static string Of(string markdown, MarkdownDialect dialect)
   {
      if(dialect == MarkdownDialect.PlainText || MarkdigParser.Parse(markdown, dialect) is not {} document) return "";

      using var html = new StringWriter();
      RendererOf(dialect, html).Render(document);
      return html.ToString();
   }

   static HtmlRenderer RendererOf(MarkdownDialect dialect, StringWriter html)
   {
      var renderer = new HtmlRenderer(html);
      MarkdigParser.PipelineFor(dialect).Setup(renderer);

      renderer.ObjectRenderers.FindExact<LineBreakInlineRenderer>()!.RenderAsHardlineBreak = true;
      if(renderer.ObjectRenderers.FindExact<AlertBlockRenderer>() is {} alerts) alerts.RenderKind = WriteTitleOfAnAlert;
      renderer.ObjectRenderers.Replace<LinkInlineRenderer>(new WriterOfLinks());
      renderer.ObjectRenderers.Replace<HtmlBlockRenderer>(new WriterOfHtmlBlocks(dialect));
      renderer.ObjectRenderers.Replace<HtmlInlineRenderer>(new WriterOfInlineHtml(dialect));

      return renderer;
   }

   ///<summary>Without the icon that GitHub has, which programs show in all sizes.</summary>
   static void WriteTitleOfAnAlert(HtmlRenderer renderer, StringSlice kind) =>
      renderer.Write("<strong>").Write(Enum.TryParse<AlertKind>(kind.ToString(), ignoreCase: true, out var known) ? known.ToString() : kind.ToString()).Write("</strong>");
}
