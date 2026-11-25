namespace Mavalonia.MarkdownEditing.Rendering.Texts;

///<summary>One <see cref="char"/> that is shown, and the markdown it stands for.</summary>
///<param name="Value">What is shown.</param>
///<param name="Source">The markdown that the char stands for. Usually that one char, but an escaped char is preceded by its backslash, an entity such as <c>&amp;amp;</c> is several,
/// and a line break includes what precedes the text on the next line, such as a quote mark.</param>
///<param name="Formatting">How it is formatted.</param>
public readonly record struct RenderedChar(char Value, SourceRange Source, TextFormatting Formatting)
{
   internal bool IsLineBreak => Value == '\n';
   public bool IsWhiteSpace => char.IsWhiteSpace(Value);
}
