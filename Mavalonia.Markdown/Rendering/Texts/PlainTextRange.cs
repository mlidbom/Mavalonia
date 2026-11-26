namespace Mavalonia.Markdown.Rendering.Texts;

///<summary>A part of a <see cref="PlainText"/>.</summary>
public readonly record struct PlainTextRange(int Start, int Length)
{
   public int End => Start + Length;
}
