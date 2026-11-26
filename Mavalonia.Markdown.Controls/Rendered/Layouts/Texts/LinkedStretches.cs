using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Controls.Rendered.Layouts.Texts;

///<summary>The stretches of the text of a block that the application shows as links (<see cref="MarkdownView.Links"/>), in their order, none overlapping another.</summary>
///<remarks>
/// Two are equal when their stretches are, so that lines drawn with some links are drawn again only when the links in them are others.
///</remarks>
sealed partial class LinkedStretches : IEquatable<LinkedStretches>
{
   readonly IReadOnlyList<Stretch> _stretches;

   LinkedStretches(IReadOnlyList<Stretch> stretches) => _stretches = stretches;

   ///<summary>The stretches of <paramref name="parts"/>, of one block: those that overlap or touch one another are one.</summary>
   internal static LinkedStretches Of(IEnumerable<PartOfABlock> parts)
   {
      var stretches = new List<Stretch>();
      foreach(var part in parts.OrderBy(it => it.Start))
      {
         if(stretches.Count > 0 && part.Start <= stretches[^1].End) stretches[^1] = stretches[^1] with { End = Math.Max(stretches[^1].End, part.End) };
         else stretches.Add(new Stretch(part.Start, part.End));
      }

      return new LinkedStretches(stretches);
   }

   internal bool AreAny => _stretches.Count > 0;

   ///<summary>Those that are between the char at <paramref name="start"/> and that before <paramref name="end"/>, as far as they are.</summary>
   internal IEnumerable<Stretch> Between(int start, int end) =>
      _stretches.Where(it => it.End > start && it.Start < end).Select(it => new Stretch(Math.Max(it.Start, start), Math.Min(it.End, end)));

   public bool Equals(LinkedStretches? other) => other is not null && _stretches.SequenceEqual(other._stretches);
   public override bool Equals(object? other) => Equals(other as LinkedStretches);
   public override int GetHashCode() => _stretches.Aggregate(_stretches.Count, (hash, it) => HashCode.Combine(hash, it));
}
