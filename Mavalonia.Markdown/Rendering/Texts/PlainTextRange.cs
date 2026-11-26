using JetBrains.Annotations;

namespace Mavalonia.Markdown.Rendering.Texts;

///<summary>A part of a <see cref="PlainText"/>.</summary>
public readonly record struct PlainTextRange(int Start, int Length)
{
   [PublicAPI]
   public int End => Start + Length;
}
