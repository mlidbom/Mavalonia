using Markdig;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Reading;

namespace Mavalonia.MarkdownEditing.Html;

///<summary>Writes markdown as HTML. HTML in the markdown is text in what is written, as it is where the markdown is shown.</summary>
static class HtmlOfMarkdown
{
   static readonly MarkdownPipeline PipelineForEditing = MarkdigParser.PipelineBuilderFor(MarkdownDialect.ForEditing).DisableHtml().UseSoftlineBreakAsHardlineBreak().Build();
   static readonly MarkdownPipeline PipelineForReading = MarkdigParser.PipelineBuilderFor(MarkdownDialect.ForReading).DisableHtml().UseSoftlineBreakAsHardlineBreak().Build();

   ///<summary>Empty when <paramref name="dialect"/> is plain text.</summary>
   internal static string Of(string markdown, MarkdownDialect dialect) =>
      dialect == MarkdownDialect.PlainText ? "" : Markdown.ToHtml(markdown, dialect == MarkdownDialect.ForReading ? PipelineForReading : PipelineForEditing);
}
