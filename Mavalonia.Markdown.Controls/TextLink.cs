using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Controls;

///<summary>Some text that the application shows as a link where the markdown has none, such as the name of something in its code.</summary>
///<param name="Range">The text, in the <see cref="MarkdownView.PlainText"/> of the view.</param>
///<param name="Target">Where it leads: the <see cref="LinkClickedEventArgs.Target"/> of a click on it.</param>
public readonly record struct TextLink(PlainTextRange Range, string Target);
