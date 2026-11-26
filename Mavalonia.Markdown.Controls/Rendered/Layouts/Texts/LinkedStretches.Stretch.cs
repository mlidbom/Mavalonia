namespace Mavalonia.Markdown.Controls.Rendered.Layouts.Texts;

partial class LinkedStretches
{
   ///<summary>Chars of the text of a block that are shown as a link.</summary>
   ///<param name="Start">The index of the first of them.</param>
   ///<param name="End">The index of the char after the last of them.</param>
   internal readonly record struct Stretch(int Start, int End);
}
