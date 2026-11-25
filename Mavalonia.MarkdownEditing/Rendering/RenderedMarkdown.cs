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

   ///<summary>All the text that is shown, from the first block to the last.</summary>
   internal string Text => TextOf(Blocks);

   internal static string TextOf(IEnumerable<RenderedBlock> blocks) => string.Concat(blocks.Select(it => it.Text.Value));

   public RenderedPosition Start => RenderedPosition.AtTheStartOf(Blocks[0]);
   public RenderedPosition End => RenderedPosition.AtTheEndOf(Blocks[^1]);

   ///<summary>The position that a caret at <paramref name="offset"/> is shown at: where nothing is shown of the markdown there, the nearest before it.</summary>
   internal RenderedPosition PositionAt(int offset)
   {
      var block = Blocks.LastOrDefault(it => it.Text.Source.Start <= offset) ?? Blocks[0];
      return new RenderedPosition(block, block.Text.IndexAt(offset));
   }

   ///<summary>The offset nearest to <paramref name="offset"/> at which text can be typed: not inside what is shown as one char, nor outside the text of all blocks.</summary>
   internal int OffsetWhereTextCanBeTyped(int offset)
   {
      var position = PositionAt(offset);
      var text = position.Block.Text;
      if(!text.Source.Touches(offset)) return position.Offset;

      return position.IsAtTheEndOfItsBlock ? offset : Math.Min(offset, text.Chars[position.Index].Source.Start);
   }

   internal PositionByNumbers NumbersOf(RenderedPosition position) => new(NumberOf(position.Block), position.Index);

   ///<summary>The position that <paramref name="numbers"/> say, or the nearest there is.</summary>
   internal RenderedPosition PositionNearestTo(PositionByNumbers numbers)
   {
      if(numbers.BlockNumber >= Blocks.Count) return End;

      var block = Blocks[numbers.BlockNumber];
      return new RenderedPosition(block, Math.Min(numbers.Index, block.Text.Length));
   }

   internal RenderedBlock? BlockBefore(RenderedBlock block) => NumberOf(block) is var number and > 0 ? Blocks[number - 1] : null;
   internal RenderedBlock? BlockAfter(RenderedBlock block) => NumberOf(block) + 1 is var number && number < Blocks.Count ? Blocks[number] : null;

   ///<summary>Whether <paramref name="block"/> is the first of the blocks inside <paramref name="container"/>.</summary>
   internal bool IsTheFirstBlockInside(BlockContainer container, RenderedBlock block) => BlockBefore(block)?.IsInside(container) != true;

   ///<summary>Whether there is no line between <paramref name="block"/> and the block after it.</summary>
   internal bool IsFollowedByABlockOnTheNextLine(RenderedBlock block) => BlockAfter(block) is {} next && Source.StartOfLineAfter(block.Source.End) == Source.StartOfLineAt(next.Source.Start);

   internal int NumberOf(RenderedBlock block) => Enumerable.Range(0, Blocks.Count).First(it => Blocks[it] == block);
}
