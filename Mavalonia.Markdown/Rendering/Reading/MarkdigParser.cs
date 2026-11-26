using Markdig;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Extensions.Tables;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Mavalonia.Markdown.Rendering.Reading;

///<summary>Has Markdig parse markdown, with the markup that we render.</summary>
static class MarkdigParser
{
   ///<summary>How deep inlines may be, the blocks that they are inside counted too. Markdig itself can't write what is much deeper as HTML.</summary>
   const int Deepest = 100;

   static readonly MarkdownPipeline PipelineForEditing = PipelineWithoutTheMarkupThatPeopleTypeWithoutMeaningTo().Build();
   static readonly MarkdownPipeline PipelineForReading = PipelineOfAllDialects().UseAutoLinks().UseTaskLists().UseAlertBlocks().Build();

   internal static MarkdownPipeline PipelineFor(MarkdownDialect dialect) => dialect == MarkdownDialect.ForReading ? PipelineForReading : PipelineForEditing;

   ///<summary>Null when blocks or inlines are inside one another deeper than they can be read, which nobody writes: hundreds deep.</summary>
   internal static MarkdownDocument? Parse(string markdown, MarkdownDialect dialect)
   {
      if(ParseUnlessBlocksAreTooDeep(markdown, PipelineFor(dialect)) is not {} document) return null;

      var leaves = document.Descendants<LeafBlock>().ToList();
      foreach(var leaf in leaves) leaf.Inline?.TakeWhatFollowsDelimitersOutOfThem();
      foreach(var cell in document.Descendants<TableCell>()) cell.TakeTheBackslashOutOfEscapedPipesInCode();

      return leaves.Any(IsTooDeep) ? null : document;
   }

   static bool IsTooDeep(LeafBlock leaf) => leaf.Inline?.AreInsideOneAnotherDeeperThan(Deepest - DepthOf(leaf)) == true;

   static int DepthOf(Block block) => block.Parent is {} parent ? DepthOf(parent) + 1 : 0;

   static MarkdownDocument? ParseUnlessBlocksAreTooDeep(string markdown, MarkdownPipeline pipeline)
   {
      try
      {
         return Markdig.Markdown.Parse(markdown, pipeline);
      }
      catch(ArgumentException limitExceeded) when(limitExceeded.Message.Contains("depth limit", StringComparison.Ordinal)) // Markdig has no other way to tell.
      {
         return null;
      }
   }

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
