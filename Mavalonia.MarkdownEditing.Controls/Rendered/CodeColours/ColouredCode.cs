namespace Mavalonia.MarkdownEditing.Controls.Rendered.CodeColours;

///<summary>The colours that some code is drawn in.</summary>
class ColouredCode
{
   readonly CodeToColour _code;

   internal ColouredCode(CodeToColour code, IReadOnlyList<ColouredStretch> stretches)
   {
      _code = code;
      Stretches = stretches;
   }

   internal IReadOnlyList<ColouredStretch> Stretches { get; }

   ///<summary>How much of <paramref name="other"/> starts as this code does. Nothing, when it's in another language, or for another theme.</summary>
   internal int LengthInCommonWith(CodeToColour other) => _code.Language == other.Language && _code.IsDark == other.IsDark ? _code.Text.AsSpan().CommonPrefixLength(other.Text) : 0;

   ///<summary>The colours of <paramref name="other"/>, as far as it starts as this code does.</summary>
   internal ColouredCode AsFarAsInCommonWith(CodeToColour other)
   {
      var end = LengthInCommonWith(other);
      return new ColouredCode(other, [..Stretches.Where(it => it.End <= end)]);
   }
}
