using Mavalonia.Markdown.Rendering;

namespace Mavalonia.Markdown.Editing.Changes;

///<summary>A change to a markdown text, and where the caret is after it.</summary>
class SourceChange
{
   SourceChange(SourceRange replaced, string replacement, RenderedMarkdown after, int caret)
   {
      Replaced = replaced;
      Replacement = replacement;
      After = after;
      Caret = caret;
   }

   internal static SourceChange Replace(RenderedMarkdown rendered, SourceRange replaced, string replacement)
   {
      var after = RenderedMarkdown.Of(rendered.Source[..replaced.Start] + replacement + rendered.Source[replaced.End..]);
      return new SourceChange(replaced, replacement, after, caret: replaced.Start + replacement.Length);
   }

   internal static SourceChange Insert(RenderedMarkdown rendered, int offset, string text) => Replace(rendered, SourceRange.EmptyAt(offset), text);

   internal static SourceChange Remove(RenderedMarkdown rendered, SourceRange removed) => Replace(rendered, removed, "");

   ///<summary>Changes nothing but where the caret is.</summary>
   internal static SourceChange OnlyMoveTheCaretTo(RenderedMarkdown rendered, RenderedPosition position) => new(SourceRange.EmptyAt(position.Offset), "", rendered, position.Offset);

   internal SourceRange Replaced { get; }
   internal string Replacement { get; }

   ///<summary>What is rendered after the change.</summary>
   internal RenderedMarkdown After { get; }

   ///<summary>The offset of the caret after the change. Unless said otherwise, it is after the text that is inserted.</summary>
   internal int Caret { get; }

   internal bool ChangesNothing => Replaced.Length == 0 && Replacement.Length == 0;

   internal SourceChange WithTheCaretAt(int offset) => new(Replaced, Replacement, After, offset);

   ///<summary>Puts the caret at <paramref name="position"/> of what is rendered after the change, or as near to there as it can be.</summary>
   internal SourceChange WithTheCaretAt(PositionByNumbers position) => WithTheCaretAt(After.PositionNearestTo(position).Offset);
}
