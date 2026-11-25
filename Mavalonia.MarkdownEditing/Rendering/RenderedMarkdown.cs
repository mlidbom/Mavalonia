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

   ///<summary>The offset nearest to <paramref name="offset"/> at which text can be typed: not inside what is shown as one character, nor outside the text of all blocks.</summary>
   internal int OffsetWhereTextCanBeTyped(int offset)
   {
      var position = PositionAt(offset);
      var text = position.Block.Text;
      if(!text.Source.Touches(offset)) return position.Offset;

      return position.IsAtTheEndOfItsBlock ? offset : Math.Min(offset, text.Characters[position.Index].Source.Start);
   }

   ///<summary>The position in block number <paramref name="blockNumber"/> before its character number <paramref name="index"/>, or the nearest there is.</summary>
   internal RenderedPosition PositionNearestTo(int blockNumber, int index)
   {
      if(blockNumber < 0) return RenderedPosition.AtTheStartOf(Blocks[0]);
      if(blockNumber >= Blocks.Count) return RenderedPosition.AtTheEndOf(Blocks[^1]);

      return new RenderedPosition(Blocks[blockNumber], Math.Min(index, Blocks[blockNumber].Text.Length));
   }

   internal RenderedBlock? BlockBefore(RenderedBlock block) => NumberOf(block) is var number and > 0 ? Blocks[number - 1] : null;
   internal RenderedBlock? BlockAfter(RenderedBlock block) => NumberOf(block) + 1 is var number && number < Blocks.Count ? Blocks[number] : null;

   ///<summary>Whether <paramref name="block"/> is the first of the blocks inside <paramref name="container"/>.</summary>
   internal bool IsTheFirstIn(BlockContainer container, RenderedBlock block) => BlockBefore(block) is not {} previous || !previous.Containers.OutermostFirst.Contains(container);

   internal int NumberOf(RenderedBlock block) => Enumerable.Range(0, Blocks.Count).First(it => Blocks[it] == block);
}
