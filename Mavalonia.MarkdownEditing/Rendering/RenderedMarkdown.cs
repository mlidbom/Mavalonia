using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Reading;

namespace Mavalonia.MarkdownEditing.Rendering;

///<summary>What a markdown text is shown as when it's rendered.</summary>
public class RenderedMarkdown
{
   RenderedMarkdown(string source, IReadOnlyList<RenderedBlock> blocks)
   {
      Source = source;
      Blocks = blocks;
   }

   public static RenderedMarkdown Of(string markdown) => new(markdown, MarkdownReader.Read(markdown));

   ///<summary>The markdown.</summary>
   public string Source { get; }

   ///<summary>The blocks from the first to the last. There is always at least one: an empty markdown text is rendered as an empty paragraph.</summary>
   public IReadOnlyList<RenderedBlock> Blocks { get; }

   ///<summary>The position that a caret at <paramref name="offset"/> is shown at: where nothing is shown of the markdown there, the nearest before it.</summary>
   public RenderedPosition PositionAt(int offset)
   {
      var block = Blocks.LastOrDefault(it => it.Text.Source.Start <= offset) ?? Blocks[0];
      return new RenderedPosition(block, block.Text.IndexAt(offset));
   }

   internal RenderedBlock? BlockBefore(RenderedBlock block) => NumberOf(block) is var number and > 0 ? Blocks[number - 1] : null;
   internal RenderedBlock? BlockAfter(RenderedBlock block) => NumberOf(block) + 1 is var number && number < Blocks.Count ? Blocks[number] : null;

   ///<summary>Whether <paramref name="block"/> is the first of the blocks inside <paramref name="container"/>.</summary>
   internal bool IsTheFirstIn(BlockContainer container, RenderedBlock block) => BlockBefore(block) is not {} previous || !previous.Containers.OutermostFirst.Contains(container);

   internal int NumberOf(RenderedBlock block) => Enumerable.Range(0, Blocks.Count).First(it => Blocks[it] == block);
}
