namespace Mavalonia.Markdown.Rendering;

///<summary>A <see cref="RenderedRange"/> said by numbers alone, so that it can be found again in what is rendered after a change.</summary>
readonly record struct RangeByNumbers(PositionByNumbers Start, PositionByNumbers End)
{
   ///<summary>The range in <paramref name="rendered"/>, or the nearest to it that there is.</summary>
   internal RenderedRange In(RenderedMarkdown rendered) => new(rendered, rendered.PositionNearestTo(Start), rendered.PositionNearestTo(End));
}
