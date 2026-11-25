namespace Mavalonia.MarkdownEditing.Rendering;

///<summary>A stretch of a markdown text: from <paramref name="Start"/> up to, and not including, <paramref name="End"/>.</summary>
public readonly record struct SourceRange(int Start, int End)
{
   public static SourceRange EmptyAt(int offset) => new(offset, offset);

   public int Length => End - Start;

   ///<summary>Whether <paramref name="offset"/> is in the range or at either end of it.</summary>
   public bool Touches(int offset) => Start <= offset && offset <= End;
}
