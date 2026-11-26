using Mavalonia.Markdown.Rendering.Containers;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Rendering.Blocks;

///<summary>A block that isn't rendered but shown as it is written, such as HTML.</summary>
public class SourceBlock : RenderedBlock
{
   internal SourceBlock(BlockContainers containers, RenderedText text) : base(containers, text, text.Source) {}

   internal override bool EndsWithItsLastLine => false;

   internal override bool IsTheSameKindAs(RenderedBlock other) => other is SourceBlock;
}
