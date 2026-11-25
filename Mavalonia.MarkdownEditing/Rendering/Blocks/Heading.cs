using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Rendering.Blocks;

public class Heading : RenderedBlock
{
   internal Heading(BlockContainers containers, RenderedText text, SourceRange source, int level) : base(containers, text, source) => Level = level;

   ///<summary>1 for the most prominent heading, 6 for the least.</summary>
   public int Level { get; }

   ///<summary>The number signs that make it a heading, with the spaces between them and its text.</summary>
   internal SourceRange Marks => new(Source.Start, Text.Source.Start);

   internal override bool IsTheSameKindAs(RenderedBlock other) => other is Heading heading && heading.Level == Level;
}
