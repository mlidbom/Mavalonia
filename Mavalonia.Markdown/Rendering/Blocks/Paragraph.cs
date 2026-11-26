using Mavalonia.Markdown.Rendering.Containers;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Rendering.Blocks;

///<summary>A paragraph. It may be empty: markdown has no empty paragraphs, but a person who is about to write one has to see where.</summary>
public class Paragraph : RenderedBlock
{
   internal Paragraph(BlockContainers containers, RenderedText text) : base(containers, text, text.Source) {}

   internal static Paragraph EmptyAt(int offset, BlockContainers containers) => new(containers, RenderedText.EmptyAt(offset));

   internal override bool EndsWithItsLastLine => false;

   internal override bool IsTheSameKindAs(RenderedBlock other) => other is Paragraph;
}
