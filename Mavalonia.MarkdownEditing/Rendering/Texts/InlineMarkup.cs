namespace Mavalonia.MarkdownEditing.Rendering.Texts;

///<summary>Markup that formats a stretch of a block's text, such as <c>**bold**</c>.</summary>
///<param name="Source">The whole of it, delimiters included.</param>
///<param name="Content">What is between its delimiters.</param>
///<param name="FirstIndex">The index of the first character that it formats.</param>
///<param name="EndIndex">The index after the last character that it formats.</param>
///<param name="Formatting">The formatting it gives, alone.</param>
record InlineMarkup(SourceRange Source, SourceRange Content, int FirstIndex, int EndIndex, TextFormatting Formatting);
