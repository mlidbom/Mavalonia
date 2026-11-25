using Avalonia;
using Avalonia.Media.TextFormatting;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;

///<summary>One of the lines that the text of a block is laid out in.</summary>
class LineLayout
{
   readonly BlockLayout _block;
   readonly TextLine _line;

   internal LineLayout(BlockLayout block, TextLine line, Rect bounds)
   {
      _block = block;
      _line = line;
      Bounds = bounds;
   }

   internal Rect Bounds { get; }

   internal RenderedPosition Start => PositionBefore(_line.FirstTextSourceIndex);

   ///<summary>After the last character on the line that isn't white space where the line is wrapped, or a line break.</summary>
   internal RenderedPosition End => PositionBefore(_line.FirstTextSourceIndex + _line.Length - WhiteSpaceAtTheEnd);

   int WhiteSpaceAtTheEnd => _line.NewLineLength > 0 ? _line.NewLineLength : IsTheLast ? 0 : _line.TrailingWhitespaceLength;

   bool IsTheLast => _line.FirstTextSourceIndex + _line.Length >= _block.Block.Text.Length;

   RenderedPosition PositionBefore(int index) => new(_block.Block, Math.Clamp(index, 0, _block.Block.Text.Length));

   ///<summary>The position on the line that is nearest to <paramref name="x"/>.</summary>
   internal RenderedPosition PositionNearestTo(double x)
   {
      var hit = _line.GetCharacterHitFromDistance(x - Bounds.Left);
      return PositionBefore(Math.Min(hit.FirstCharacterIndex + hit.TrailingLength, End.Index));
   }
}
