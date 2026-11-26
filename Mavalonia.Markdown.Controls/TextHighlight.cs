using Avalonia.Media;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Controls;

///<summary>Some text that is marked out, such as what a search has found.</summary>
///<param name="Range">The text, in the <see cref="MarkdownView.PlainText"/> of the view.</param>
///<param name="Background">What is drawn behind it.</param>
public readonly record struct TextHighlight(PlainTextRange Range, IBrush Background);
