namespace Mavalonia.Markdown.Controls.Rendered.CodeColours;

///<summary>Some code, and what decides its colours.</summary>
///<param name="Text">The code.</param>
///<param name="Language">What its block says that it's written in, such as <c>cs</c>.</param>
///<param name="IsDark">Whether it's drawn on a dark background.</param>
readonly record struct CodeToColour(string Text, string Language, bool IsDark)
{
   ///<summary>Whether <paramref name="start"/> is how this starts, in the same language and for the same background.</summary>
   internal bool GoesOnFrom(CodeToColour start) => IsLike(start) && Text.StartsWith(start.Text, StringComparison.Ordinal);

   ///<summary>How much of <paramref name="other"/> starts as this does. Nothing, when it's in another language, or for another background.</summary>
   internal int LengthInCommonWith(CodeToColour other) => IsLike(other) ? Text.AsSpan().CommonPrefixLength(other.Text) : 0;

   bool IsLike(CodeToColour other) => Language == other.Language && IsDark == other.IsDark;
}
