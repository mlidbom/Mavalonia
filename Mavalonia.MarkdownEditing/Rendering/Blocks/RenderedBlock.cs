using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Rendering.Blocks;

///<summary>One of the blocks that a markdown text is rendered as, such as a paragraph. Each has a text of its own, in which a caret can be.</summary>
public abstract class RenderedBlock
{
   internal RenderedBlock(BlockContainers containers, RenderedText text, SourceRange source)
   {
      Containers = containers;
      Text = text;
      Source = source;
   }

   ///<summary>The quotes and list items that the block is inside.</summary>
   public BlockContainers Containers { get; }

   public RenderedText Text { get; }

   ///<summary>The whole of the block in the markdown: its own markup included, but not what its containers put before it on its first line.</summary>
   internal SourceRange Source { get; }

   public bool IsInside(BlockContainer container) => Containers.OutermostFirst.Contains(container);

   ///<summary>Whether a paragraph typed on the line right after the block would be a block of its own.</summary>
   internal virtual bool EndsWithItsLastLine => true;

   internal bool LooksLike(RenderedBlock other) => IsTheSameKindAs(other) && Containers.LookLike(other.Containers) && Text.LooksLike(other.Text);

   ///<summary>Whether <paramref name="other"/> is the same kind of block, so that the same text in either looks the same.</summary>
   internal abstract bool IsTheSameKindAs(RenderedBlock other);
}
