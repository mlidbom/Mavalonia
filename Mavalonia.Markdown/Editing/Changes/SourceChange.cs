using Mavalonia.Markdown.Rendering;

namespace Mavalonia.Markdown.Editing.Changes;

///<summary>A change to a markdown text, and where the caret is after it.</summary>
class SourceChange
{
   SourceChange(SourceRange replaced, string replacement, SourceRange written, RenderedMarkdown after, int caret)
   {
      Replaced = replaced;
      Replacement = replacement;
      Written = written;
      After = after;
      Caret = caret;
   }

   internal static SourceChange Replace(RenderedMarkdown rendered, SourceRange replaced, string replacement)
   {
      var after = RenderedMarkdown.Of(rendered.Source[..replaced.Start] + replacement + rendered.Source[replaced.End..]);
      return new SourceChange(replaced, replacement, WrittenOver(rendered.Source[replaced.Start..replaced.End], replaced.Start, replacement), after, caret: replaced.Start + replacement.Length);
   }

   internal static SourceChange Insert(RenderedMarkdown rendered, int offset, string text) => Replace(rendered, SourceRange.EmptyAt(offset), text);

   internal static SourceChange Remove(RenderedMarkdown rendered, SourceRange removed) => Replace(rendered, removed, "");

   ///<summary>Changes nothing but where the caret is.</summary>
   internal static SourceChange OnlyMoveTheCaretTo(RenderedMarkdown rendered, RenderedPosition position) => new(SourceRange.EmptyAt(position.Offset), "", SourceRange.EmptyAt(position.Offset), rendered, position.Offset);

   internal SourceRange Replaced { get; }
   internal string Replacement { get; }

   ///<summary>
   /// Where the text after the change differs from the text before it: <see cref="Replacement"/>, without what it starts and ends with that <see cref="Replaced"/> did too.
   /// Such as the markup around a paste that is written anew.
   ///</summary>
   internal SourceRange Written { get; }

   ///<summary>What is rendered after the change.</summary>
   internal RenderedMarkdown After { get; }

   ///<summary>The offset of the caret after the change. Unless said otherwise, it is after the text that is inserted.</summary>
   internal int Caret { get; }

   internal bool ChangesNothing => Replaced.Length == 0 && Replacement.Length == 0;

   internal SourceChange WithTheCaretAt(int offset) => new(Replaced, Replacement, Written, After, offset);

   ///<summary>Puts the caret at <paramref name="position"/> of what is rendered after the change, or as near to there as it can be.</summary>
   internal SourceChange WithTheCaretAt(PositionByNumbers position) => WithTheCaretAt(After.PositionNearestTo(position).Offset);

   ///<summary>The stretch of <paramref name="replacement"/>, put in at <paramref name="start"/>, that differs from <paramref name="replaced"/>, which it replaces.</summary>
   static SourceRange WrittenOver(string replaced, int start, string replacement)
   {
      var sameAtTheStart = replaced.AsSpan().CommonPrefixLength(replacement);
      var sameAtTheEnd = CommonSuffixLength(replaced.AsSpan(sameAtTheStart), replacement.AsSpan(sameAtTheStart));
      return new SourceRange(start + sameAtTheStart, start + replacement.Length - sameAtTheEnd);

      static int CommonSuffixLength(ReadOnlySpan<char> first, ReadOnlySpan<char> second)
      {
         var length = 0;
         while(length < first.Length && length < second.Length && first[^(length + 1)] == second[^(length + 1)]) length++;
         return length;
      }
   }
}
