namespace Mavalonia.MarkdownEditing.Rendering.Texts;

///<summary>The ways in which text can be formatted inside a block.</summary>
[Flags]
public enum TextFormats
{
   None = 0,
   Bold = 1,
   Italic = 2,
   Strikethrough = 4,
   Code = 8
}
