using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Rendering.Blocks;

///<summary>A block that isn't rendered but shown as it is written, such as HTML.</summary>
public class SourceBlock : RenderedBlock
{
   internal SourceBlock(BlockContainers containers, RenderedText text) : base(containers, text, text.Source) {}

   internal override bool EndsWithItsLastLine => false;

   internal override bool IsTheSameKindAs(RenderedBlock other) => other is SourceBlock;
}
