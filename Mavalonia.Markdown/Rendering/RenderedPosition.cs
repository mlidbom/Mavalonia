using JetBrains.Annotations;
using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Rendering;

///<summary>A place where a caret can be in what is rendered: in the text of <paramref name="Block"/>, before its char number <paramref name="Index"/>.</summary>
public readonly record struct RenderedPosition(RenderedBlock Block, int Index)
{
   public static RenderedPosition AtTheStartOf(RenderedBlock block) => new(block, 0);
   public static RenderedPosition AtTheEndOf(RenderedBlock block) => new(block, block.Text.Length);

   internal bool IsAtTheStartOfItsBlock => Index == 0;
   [PublicAPI]
   public bool IsAtTheEndOfItsBlock => Index == Block.Text.Length;

   ///<summary>The offset in the markdown at which text typed at the position goes: it continues what is before the position.</summary>
   public int Offset => Block.Text.OffsetAt(Index);

   ///<summary>The offsets in the markdown that a caret at the position can be at, the first being <see cref="Offset"/>: see <see cref="Texts.RenderedText.CaretStopsAt"/>.</summary>
   internal IReadOnlyList<int> CaretStops => Block.Text.CaretStopsAt(Index);
}
