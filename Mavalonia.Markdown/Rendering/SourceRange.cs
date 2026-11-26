namespace Mavalonia.Markdown.Rendering;

///<summary>A stretch of a markdown text: from <paramref name="Start"/> up to, and not including, <paramref name="End"/>.</summary>
public readonly record struct SourceRange(int Start, int End)
{
   internal static SourceRange EmptyAt(int offset) => new(offset, offset);

   public int Length => End - Start;

   ///<summary>What is between the delimiters that the range starts and ends with, such as the <c>**</c> of <c>**bold**</c>, each <paramref name="ofLength"/> long.</summary>
   internal SourceRange BetweenDelimiters(int ofLength) => new(Start + ofLength, End - ofLength);

   ///<summary>Whether <paramref name="offset"/> is in the range: at its start, or after that and before its end.</summary>
   internal bool Contains(int offset) => Start <= offset && offset < End;

   ///<summary>Whether <paramref name="offset"/> is in the range or at either end of it.</summary>
   internal bool Touches(int offset) => Start <= offset && offset <= End;
}
