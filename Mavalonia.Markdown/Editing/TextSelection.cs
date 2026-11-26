using JetBrains.Annotations;
using Mavalonia.Markdown.Rendering;

namespace Mavalonia.Markdown.Editing;

///<summary>What is selected of a markdown text.</summary>
///<param name="Anchor">The offset at which selecting started.</param>
///<param name="Caret">The offset that the caret is at.</param>
public readonly record struct TextSelection(int Anchor, int Caret)
{
   public static TextSelection CaretAt(int offset) => new(offset, offset);

   public bool IsEmpty => Anchor == Caret;
   public int Start => Math.Min(Anchor, Caret);
   public int End => Math.Max(Anchor, Caret);

   ///<summary>All of what is selected.</summary>
   [PublicAPI]
   public SourceRange Range => new(Start, End);

   ///<summary>Whether anything is selected, and <paramref name="offset"/> is in it or at either end of it.</summary>
   internal bool Includes(int offset) => !IsEmpty && Range.Touches(offset);
}
