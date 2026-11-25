using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Editing.Looks;

///<summary>How a block is to look to the person who sees it.</summary>
class BlockLooks
{
   readonly RenderedBlock _blockOfTheSameKind;
   readonly BlockContainers _containers;
   readonly FormattedText _text;

   BlockLooks(RenderedBlock blockOfTheSameKind, BlockContainers containers, FormattedText text)
   {
      _blockOfTheSameKind = blockOfTheSameKind;
      _containers = containers;
      _text = text;
   }

   internal static BlockLooks Of(RenderedMarkdown rendered, RenderedBlock block) => new(block, block.Containers, FormattedText.Of(rendered, block));

   internal static BlockLooks OfAParagraph(BlockContainers containers, FormattedText text) => new(Paragraph.EmptyAt(0, containers), containers, text);

   internal BlockLooks WithText(FormattedText text) => new(_blockOfTheSameKind, _containers, text);

   internal bool AreThoseOf(RenderedBlock block) => _blockOfTheSameKind.IsTheSameKindAs(block) && _containers.LookLike(block.Containers) && _text.LooksLike(block.Text);
}
