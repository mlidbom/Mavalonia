using Avalonia;
using Avalonia.Media;
using Mavalonia.Markdown.Controls.Rendered.Layouts.Texts;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Controls.Rendered.Layouts;

///<summary>Where a block is drawn, and how.</summary>
class BlockLayout
{
   const double ThicknessOfLines = 1;

   readonly LaidOutText _text;
   readonly Point _originOfText;
   readonly MarkdownAppearance _appearance;

   internal BlockLayout(RenderedBlock block, Rect bounds, LaidOutText text, Point originOfText, MarkdownAppearance appearance)
   {
      Block = block;
      Bounds = bounds;
      _text = text;
      _originOfText = originOfText;
      _appearance = appearance;
   }

   internal RenderedBlock Block { get; }
   internal Rect Bounds { get; }

   ///<summary>How far to the right what the block shows reaches: the borders of a cell of a table, or else its widest line, with as much space after it as code has before it.</summary>
   internal double RightOfWhatItShows => Block is TableCell ? Bounds.Right : _originOfText.X + _text.WidthIncludingTrailingWhitespace + (_originOfText.X - Bounds.Left);

   ///<summary>Draws what is behind the text of the block: the background of code, the borders of a cell of a table, the line of a thematic break.</summary>
   ///<remarks>It's drawn before highlights and the selection, which an opaque <see cref="MarkdownAppearance.CodeBackground"/> would otherwise hide.</remarks>
   internal void DrawBackground(DrawingContext context)
   {
      var pen = new Pen(_appearance.LineBrush, ThicknessOfLines);
      switch(Block)
      {
         case CodeBlock: context.DrawRectangle(_appearance.CodeBackground, null, Bounds, _appearance.Scaled(4), _appearance.Scaled(4)); break;
         case TableCell cell: context.DrawRectangle(cell.IsInTheHeader ? _appearance.CodeBackground : null, pen, Bounds); break;
         case ThematicBreak: context.DrawLine(pen, new Point(Bounds.Left, Bounds.Center.Y), new Point(Bounds.Right, Bounds.Center.Y)); break;
      }

      if(Block is not CodeBlock) DrawTheBackgroundOfTheCodeInTheText(context);
   }

   ///<summary>Draws the background of the code in a line of text, such as <c>`code`</c>.</summary>
   void DrawTheBackgroundOfTheCodeInTheText(DrawingContext context)
   {
      foreach(var code in Block.Text.StretchesOfTheSameFormatting.Where(it => it.Formatting.Has(TextFormats.Code)))
         foreach(var rectangle in BoundsOf(new PartOfABlock(Block, code.Start, code.Start + code.Length))) context.FillRectangle(_appearance.CodeBackground, rectangle);
   }

   ///<summary>Draws that what is between two offsets in the markdown is selected, as far as it is in the block.</summary>
   internal void DrawSelection(DrawingContext context, SourceRange selected)
   {
      if(selected.End < Block.Text.Source.Start || selected.Start > Block.Text.Source.End) return;

      var (start, end) = (Block.Text.IndexAt(selected.Start), Block.Text.IndexAt(selected.End));
      if(end > start)
         foreach(var rectangle in BoundsOf(new PartOfABlock(Block, start, end))) context.FillRectangle(_appearance.SelectionBrush, rectangle);
      else if(Block.Text.IsEmpty)
         context.FillRectangle(_appearance.SelectionBrush, CaretAt(0).WithWidth(_appearance.Scaled(5)));
   }

   ///<summary>Where a part of the block is: a rectangle for each line of it.</summary>
   internal IEnumerable<Rect> BoundsOf(PartOfABlock part) => _text.BoundsOf(part.Start, part.End).Select(it => it.Translate(_originOfText));

   internal void DrawHighlight(DrawingContext context, PartOfABlock part, IBrush background)
   {
      foreach(var rectangle in BoundsOf(part)) context.DrawRectangle(background, null, rectangle, _appearance.Scaled(2), _appearance.Scaled(2));
   }

   ///<summary>Draws what is in <paramref name="viewport"/> of the text, with <paramref name="links"/> drawn as links.</summary>
   internal void DrawText(DrawingContext context, Rect viewport, LinkedStretches links) =>
      _text.Draw(context, _originOfText, new ColoursOfAText(Block is CodeBlock code ? _appearance.ColoursOf(code) : null, links), viewport);

   ///<summary>Where a caret is drawn that is before the character at <paramref name="index"/>.</summary>
   internal Rect CaretAt(int index) => _text.BoundsAt(index).Translate(_originOfText).WithWidth(1.5);

   internal double BaselineOfTheFirstLine => _originOfText.Y + _text.Baseline;

   ///<summary>Where text as a whole starts on its first line.</summary>
   internal Point StartOfFirstLine => _originOfText;

   internal IEnumerable<LineLayout> Lines
   {
      get
      {
         var top = _originOfText.Y;
         foreach(var line in _text.Lines)
         {
            yield return new LineLayout(this, line, new Rect(_originOfText.X, top, Math.Max(line.WidthIncludingTrailingWhitespace, 1), line.Height));
            top += line.Height;
         }
      }
   }

   ///<summary>The line that a caret before the character at <paramref name="index"/> is on.</summary>
   internal LineLayout LineOf(int index) => Lines.ElementAt(_text.NumberOfTheLineAt(index));

   ///<summary>Whether <paramref name="point"/> is on <paramref name="part"/> of the block.</summary>
   internal bool Shows(PartOfABlock part, Point point) => BoundsOf(part).Any(it => it.Contains(point));

   ///<summary>Where what the markdown makes clickable at <paramref name="point"/> leads: a link, or else an image. Null when none is there.</summary>
   internal string? TargetAt(Point point) =>
      Block.Text.StretchesOfTheSameFormatting.Where(it => TargetOf(it.Formatting) is not null && Shows(new PartOfABlock(Block, it.Start, it.Start + it.Length), point))
           .Select(it => TargetOf(it.Formatting))
           .FirstOrDefault();

   static string? TargetOf(TextFormatting formatting) => formatting.Link?.Target ?? formatting.Image?.Source;

   ///<summary>The position in the block's text that is nearest to <paramref name="point"/>.</summary>
   internal RenderedPosition PositionNearestTo(Point point) => (Lines.FirstOrDefault(it => point.Y < it.Bounds.Bottom) ?? Lines.Last()).PositionNearestTo(point.X);
}
