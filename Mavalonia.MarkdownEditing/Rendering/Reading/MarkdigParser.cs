using Markdig;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Parsers;
using Markdig.Syntax;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Has Markdig parse markdown, with the markup that we render.</summary>
static class MarkdigParser
{
   static readonly MarkdownPipeline Pipeline = CreatePipeline();

   internal static MarkdownDocument Parse(string markdown) => Markdown.Parse(markdown, Pipeline);

   static MarkdownPipeline CreatePipeline()
   {
      var builder = new MarkdownPipelineBuilder().UsePreciseSourceLocation()
                                                 .UsePipeTables()
                                                 .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough);

      TurnOffTheMarkupThatPeopleTypeWithoutMeaningTo();
      return builder.Build();

      void TurnOffTheMarkupThatPeopleTypeWithoutMeaningTo()
      {
         builder.BlockParsers.Find<ParagraphBlockParser>()!.ParseSetextHeadings = false; // Typing a dash to start a list under a line would turn that line into a heading.
         builder.BlockParsers.TryRemove<IndentedCodeBlockParser>(); // Typing spaces at the start of a line would turn it into code.
      }
   }
}
