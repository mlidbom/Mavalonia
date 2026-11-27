using Avalonia;
using Avalonia.Media;
using Mavalonia.Markdown.Controls.Rendered.Layouts.Texts;
using Mavalonia.Markdown.Editing;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Controls.Rendered.Layouts;

///<summary>Where everything that is rendered is drawn.</summary>
class DocumentLayout
{
   readonly RenderedMarkdown _rendered;
   readonly IReadOnlyList<BlockLayout> _blocks;
   readonly IReadOnlyList<ContainerDecoration> _decorations;

   internal DocumentLayout(RenderedMarkdown rendered, IReadOnlyList<BlockLayout> blocks, IReadOnlyList<ContainerDecoration> decorations, Size size, double widthOfWhatItShows)
   {
      _rendered = rendered;
      _blocks = blocks;
      _decorations = decorations;
      Size = size;
      WidthOfWhatItShows = Math.Min(size.Width, widthOfWhatItShows);
   }

   internal Size Size { get; }

   ///<summary>How wide it needs to be to show what it shows as it does: as wide as it's laid out, or less when none of its lines is that long.</summary>
   internal double WidthOfWhatItShows { get; }

   internal bool IsOf(RenderedMarkdown rendered, double width) => _rendered == rendered && Size.Width.Equals(width);

   ///<summary>Draws what is in <paramref name="viewport"/>.</summary>
   internal void Draw(DrawingContext context, SourceRange? selected, IReadOnlyList<TextHighlight> highlights, IReadOnlyList<TextLink> links, Rect viewport)
   {
      var visible = _blocks.Where(it => it.Bounds.Intersects(viewport)).ToList();

      foreach(var decoration in _decorations.Where(it => it.Bounds.Intersects(viewport))) decoration.Draw(context);
      foreach(var block in visible) block.DrawBackground(context);
      foreach(var highlight in highlights)
         foreach(var part in _rendered.PlainText.PartsOf(highlight.Range)) visible.FirstOrDefault(it => it.Block == part.Block)?.DrawHighlight(context, part, highlight.Background);
      if(selected is {} range)
         foreach(var block in visible) block.DrawSelection(context, range);

      var linked = links.SelectMany(it => _rendered.PlainText.PartsOf(it.Range)).ToLookup(it => it.Block);
      foreach(var block in visible) block.DrawText(context, viewport, LinkedStretches.Of(linked[block.Block]));
   }

   BlockLayout LayoutOf(RenderedBlock block) => _blocks.First(it => it.Block == block);

   ///<inheritdoc cref="BlockLayout.CaretAt(Caret)"/>
   internal Rect CaretAt(Caret caret) => LayoutOf(caret.Block).CaretAt(caret);

   internal Point StartOfTheFirstLine => _blocks[0].StartOfFirstLine;

   internal RenderedPosition PositionNearestTo(Point point) => _blocks.MinBy(it => DistanceBetween(it.Bounds, point))!.PositionNearestTo(point);

   ///<summary>Where a part of a block is: a rectangle for each line of it.</summary>
   internal IEnumerable<Rect> BoundsOf(PartOfABlock part) => LayoutOf(part.Block).BoundsOf(part);

   ///<summary>The code at <paramref name="point"/>, and where its block is. Null when no code is there.</summary>
   internal (CodeBlock Block, Rect Bounds)? CodeAt(Point point) => _blocks.FirstOrDefault(it => it.Bounds.Contains(point)) is { Block: CodeBlock code } layout ? (code, layout.Bounds) : null;

   ///<summary>
   /// Where what can be clicked at <paramref name="point"/> leads: a link of the markdown, or else an image, or else the last of <paramref name="links"/> there.
   /// Null when none is there.
   ///</summary>
   internal string? TargetAt(Point point, IReadOnlyList<TextLink> links) =>
      _blocks.FirstOrDefault(it => it.Bounds.Contains(point)) is {} block ? block.TargetAt(point) ?? TargetOfTheLinkAt(point, block, links) : null;

   string? TargetOfTheLinkAt(Point point, BlockLayout block, IReadOnlyList<TextLink> links) =>
      links.Where(link => _rendered.PlainText.PartsOf(link.Range).Any(part => part.Block == block.Block && block.Shows(part, point))).Select(it => it.Target).LastOrDefault();

   ///<summary>What is to the left or right of a point is nearer to it than what is above or below.</summary>
   static double DistanceBetween(Rect bounds, Point point)
   {
      var horizontal = Math.Max(0, Math.Max(bounds.Left - point.X, point.X - bounds.Right));
      var vertical = Math.Max(0, Math.Max(bounds.Top - point.Y, point.Y - bounds.Bottom));
      return horizontal + vertical * 1000;
   }

   internal LineLayout LineOf(RenderedPosition position) => LayoutOf(position.Block).LineOf(position.Index);

   ///<summary>The line above that of <paramref name="position"/> that is nearest to <paramref name="x"/>. Null when there is none above.</summary>
   internal LineLayout? LineAbove(RenderedPosition position, double x)
   {
      var above = LinesWhere(it => it.Center.Y < LineOf(position).Bounds.Top);
      return above.Count == 0 ? null : NearestTo(x, above.Where(it => it.Bounds.Bottom > above.Max(line => line.Bounds.Bottom) - it.Bounds.Height / 2));
   }

   ///<summary>The line below that of <paramref name="position"/> that is nearest to <paramref name="x"/>. Null when there is none below.</summary>
   internal LineLayout? LineBelow(RenderedPosition position, double x)
   {
      var below = LinesWhere(it => it.Center.Y > LineOf(position).Bounds.Bottom);
      return below.Count == 0 ? null : NearestTo(x, below.Where(it => it.Bounds.Top < below.Min(line => line.Bounds.Top) + it.Bounds.Height / 2));
   }

   List<LineLayout> LinesWhere(Func<Rect, bool> condition) => [.._blocks.SelectMany(it => it.Lines).Where(it => condition(it.Bounds))];

   static LineLayout NearestTo(double x, IEnumerable<LineLayout> lines) => lines.MinBy(it => Math.Max(0, Math.Max(it.Bounds.Left - x, x - it.Bounds.Right)))!;
}
