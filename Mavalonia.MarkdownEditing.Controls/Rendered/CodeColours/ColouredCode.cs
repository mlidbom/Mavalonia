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

   ///<inheritdoc cref="CodeToColour.LengthInCommonWith"/>
   internal int LengthInCommonWith(CodeToColour other) => _code.LengthInCommonWith(other);

   ///<summary>The colours of <paramref name="other"/>, as far as it starts as this code does.</summary>
   internal ColouredCode AsFarAsInCommonWith(CodeToColour other)
   {
      var end = LengthInCommonWith(other);
      return new ColouredCode(other, [..Stretches.Where(it => it.End <= end)]);
   }
}
