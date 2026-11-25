using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Rendering.Blocks;

///<summary>A line across the page, written as <c>---</c>.</summary>
public class ThematicBreak : RenderedBlock
{
   internal ThematicBreak(BlockContainers containers, SourceRange source) : base(containers, RenderedText.EmptyAt(source.End), source) {}

   internal override bool IsTheSameKindAs(RenderedBlock other) => other is ThematicBreak;
}
