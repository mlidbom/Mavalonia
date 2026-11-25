using Markdig;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Has Markdig parse markdown, with the markup that we render.</summary>
static class MarkdigParser
{
   ///<summary>Fewer than Markdig itself can write as HTML.</summary>
   const int DeepestInlines = 100;

   static readonly MarkdownPipeline PipelineForEditing = PipelineBuilderFor(MarkdownDialect.ForEditing).Build();
   static readonly MarkdownPipeline PipelineForReading = PipelineBuilderFor(MarkdownDialect.ForReading).Build();

   ///<summary>Null when blocks or inlines are inside one another deeper than they can be read, which nobody writes: hundreds deep.</summary>
   internal static MarkdownDocument? Parse(string markdown, MarkdownDialect dialect)
   {
      if(ParseUnlessBlocksAreTooDeep(markdown, dialect == MarkdownDialect.ForReading ? PipelineForReading : PipelineForEditing) is not {} document) return null;

      var inlinesOfBlocks = document.Descendants<LeafBlock>().Select(it => it.Inline).OfType<ContainerInline>().ToList();
      foreach(var inlines in inlinesOfBlocks) inlines.TakeWhatFollowsDelimitersOutOfThem();

      return inlinesOfBlocks.Any(it => it.AreInsideOneAnotherDeeperThan(DeepestInlines)) ? null : document;
   }

   static MarkdownDocument? ParseUnlessBlocksAreTooDeep(string markdown, MarkdownPipeline pipeline)
   {
      try
      {
         return Markdown.Parse(markdown, pipeline);
      }
      catch(ArgumentException limitExceeded) when(limitExceeded.Message.Contains("depth limit", StringComparison.Ordinal)) // Markdig has no other way to tell.
      {
         return null;
      }
   }

   internal static MarkdownPipelineBuilder PipelineBuilderFor(MarkdownDialect dialect, Action<HtmlRenderer, StringSlice>? writeTitleOfAnAlert = null) =>
      dialect == MarkdownDialect.ForReading ? PipelineOfAllDialects().UseAutoLinks().UseTaskLists().UseAlertBlocks(writeTitleOfAnAlert) : PipelineWithoutTheMarkupThatPeopleTypeWithoutMeaningTo();

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
