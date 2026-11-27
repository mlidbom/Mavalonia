using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Editing;

///<summary>Where a person is editing: in the markdown, and in what it is rendered as.</summary>
///<remarks>
/// Several offsets in the markdown may be shown at the same position, such as inside and outside of the end of bold markup. Which of them the caret is at decides how text typed there looks.
/// </remarks>
class Caret
{
   internal Caret(RenderedMarkdown rendered, int offset)
   {
      Rendered = rendered;
      Offset = offset;
      Position = rendered.PositionAt(offset);
   }

   internal RenderedMarkdown Rendered { get; }

   ///<summary>The offset in the markdown at which text typed goes.</summary>
   internal int Offset { get; }

   ///<summary>Where the caret is shown.</summary>
   internal RenderedPosition Position { get; }

   internal RenderedBlock Block => Position.Block;
   internal int Index => Position.Index;
   internal bool IsAtTheStartOfItsBlock => Position.IsAtTheStartOfItsBlock;
   internal bool IsAtTheEndOfItsBlock => Position.IsAtTheEndOfItsBlock;

   ///<summary>The formatting that the markup around the caret gives.</summary>
   internal TextFormatting Formatting => Block.Text.FormattingAt(Offset);

   ///<summary>Whether text typed at the caret is code in a line of text, such as <c>`code`</c>.</summary>
   internal bool IsInCode => Formatting.Has(TextFormats.Code);

   ///<summary>Whether the caret is at the first of several <see cref="RenderedPosition.CaretStops"/>, as inside code where it ends, or outside it where it starts. Text typed there goes with the char before, so the caret is drawn on its side.</summary>
   internal bool IsOnTheSideOfTheCharBefore => Position.CaretStops is { Count: > 1 } stops && stops[0] == Offset;

   ///<summary>The text before the caret, on the line of its block that it is on.</summary>
   internal string TextBeforeItOnItsLine => Block.Text.Value[..Index].Split('\n')[^1];

   ///<summary>The range at the caret that text is inserted in.</summary>
   internal RenderedRange EmptyRange => RenderedRange.EmptyAt(Rendered, Position);
}
