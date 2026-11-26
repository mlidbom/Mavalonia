namespace Mavalonia.Markdown.Rendering.Texts;

///<summary>Characters of a <see cref="RenderedText"/> that follow one another and are formatted the same.</summary>
///<param name="Start">The index of the first of them.</param>
///<param name="Length">How many they are.</param>
///<param name="Formatting">How each of them is formatted.</param>
public readonly record struct StretchOfText(int Start, int Length, TextFormatting Formatting);
