using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Rendering;

///<summary>A stretch of what is rendered: from <paramref name="Start"/> up to <paramref name="End"/>, which may be in a later block.</summary>
readonly record struct RenderedRange(RenderedMarkdown Rendered, RenderedPosition Start, RenderedPosition End)
{
   internal static RenderedRange EmptyAt(RenderedMarkdown rendered, RenderedPosition position) => new(rendered, position, position);

   ///<summary>All of the blocks from <paramref name="first"/> to <paramref name="last"/>.</summary>
   internal static RenderedRange OfAll(RenderedMarkdown rendered, RenderedBlock first, RenderedBlock last) => new(rendered, RenderedPosition.AtTheStartOf(first), RenderedPosition.AtTheEndOf(last));

   internal bool IsEmpty => Start == End;
   internal bool IsInOneBlock => Start.Block == End.Block;
   internal bool IsCode => IsInOneBlock && Start.Block is CodeBlock;

   ///<summary>Whether it's all there is to see.</summary>
   internal bool IsAllThatIsRendered => Start == Rendered.Start && End == Rendered.End;

   ///<summary>The blocks that the range is in, from the first to the last.</summary>
   internal IReadOnlyList<RenderedBlock> Blocks => [..Rendered.Blocks.SkipWhile(IsNotTheFirst).TakeWhile(IsNotTheLast), End.Block];

   bool IsNotTheFirst(RenderedBlock block) => block != Start.Block;
   bool IsNotTheLast(RenderedBlock block) => block != End.Block;

   ///<summary>The part of the range that is in each of its blocks, from the first to the last.</summary>
   internal IReadOnlyList<RenderedRange> PartInEachBlock => [..Blocks.Select(PartIn)];

   RenderedRange PartIn(RenderedBlock block) =>
      new(Rendered,
          block == Start.Block ? Start : RenderedPosition.AtTheStartOf(block),
          block == End.Block ? End : RenderedPosition.AtTheEndOf(block));

   internal RangeByNumbers ByNumbers => new(Rendered.NumbersOf(Start), Rendered.NumbersOf(End));
}
