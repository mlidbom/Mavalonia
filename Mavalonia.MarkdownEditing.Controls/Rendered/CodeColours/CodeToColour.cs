namespace Mavalonia.MarkdownEditing.Controls.Rendered.CodeColours;

///<summary>Some code, and what decides its colours.</summary>
///<param name="Text">The code.</param>
///<param name="Language">What its block says that it's written in, such as <c>cs</c>.</param>
///<param name="IsDark">Whether it's drawn on a dark background.</param>
readonly record struct CodeToColour(string Text, string Language, bool IsDark);
