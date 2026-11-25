using Markdig;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Has Markdig parse markdown, with the markup that we render.</summary>
static class MarkdigParser
{
   static readonly MarkdownPipeline PipelineForEditing = PipelineBuilderFor(MarkdownDialect.ForEditing).Build();
   static readonly MarkdownPipeline PipelineForReading = PipelineBuilderFor(MarkdownDialect.ForReading).Build();

   internal static MarkdownDocument Parse(string markdown, MarkdownDialect dialect) => Markdown.Parse(markdown, dialect == MarkdownDialect.ForReading ? PipelineForReading : PipelineForEditing);

   internal static MarkdownPipelineBuilder PipelineBuilderFor(MarkdownDialect dialect) =>
      dialect == MarkdownDialect.ForReading ? PipelineOfAllDialects().UseAutoLinks().UseTaskLists().UseAlertBlocks() : PipelineWithoutTheMarkupThatPeopleTypeWithoutMeaningTo();

   static MarkdownPipelineBuilder PipelineOfAllDialects() => new MarkdownPipelineBuilder().UsePreciseSourceLocation()
                                                                                         .UsePipeTables()
                                                                                         .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough);

   static MarkdownPipelineBuilder PipelineWithoutTheMarkupThatPeopleTypeWithoutMeaningTo()
   {
      var builder = PipelineOfAllDialects();
      builder.BlockParsers.Find<ParagraphBlockParser>()!.ParseSetextHeadings = false; // Typing a dash to start a list under a line would turn that line into a heading.
      builder.BlockParsers.TryRemove<IndentedCodeBlockParser>(); // Typing spaces at the start of a line would turn it into code.

      return builder;
   }
}
