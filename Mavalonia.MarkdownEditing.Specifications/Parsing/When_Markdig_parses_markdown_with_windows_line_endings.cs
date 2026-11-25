using Compze.Must;
using Compze.xUnitBDD;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Mavalonia.MarkdownEditing.Specifications.Parsing;

public class When_Markdig_parses_markdown_with_windows_line_endings
{
   const string MarkdownText = "# A heading\r\n\r\nSome *emphasized* text\r\n";

   readonly MarkdownDocument _document = Markdown.Parse(MarkdownText, new MarkdownPipelineBuilder().UsePreciseSourceLocation().Build());

   [XF] public void the_source_span_of_a_heading_covers_exactly_its_text() => TextOf(_document.Descendants<HeadingBlock>().Single()).Must().Be("# A heading");
   [XF] public void the_source_span_of_an_emphasis_covers_exactly_its_text() => TextOf(_document.Descendants<EmphasisInline>().Single()).Must().Be("*emphasized*");

   static string TextOf(MarkdownObject markdownObject) => MarkdownText.Substring(markdownObject.Span.Start, markdownObject.Span.Length);
}
