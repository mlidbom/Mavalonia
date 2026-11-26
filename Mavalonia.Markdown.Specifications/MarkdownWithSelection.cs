using Mavalonia.Markdown.Editing;

namespace Mavalonia.Markdown.Specifications;

///<summary>Markdown with what is selected of it. Written with <c>‸</c> for the caret, or <c>⟦</c> and <c>⟧</c> for the start and end of what is selected, so that specifications can say it in a string.</summary>
record MarkdownWithSelection(string Markdown, TextSelection Selection)
{
   const string Caret = "‸";
   const string StartOfSelection = "⟦";
   const string EndOfSelection = "⟧";

   internal static MarkdownWithSelection Parse(string written) => new(written.Replace(Caret, "").Replace(StartOfSelection, "").Replace(EndOfSelection, ""), SelectionIn(written));

   static TextSelection SelectionIn(string written) =>
      written.Contains(StartOfSelection, StringComparison.Ordinal)
         ? new TextSelection(Anchor: written.IndexOf(StartOfSelection, StringComparison.Ordinal), Caret: written.IndexOf(EndOfSelection, StringComparison.Ordinal) - 1)
         : TextSelection.CaretAt(written.IndexOf(Caret, StringComparison.Ordinal));

   public override string ToString() =>
      Selection.IsEmpty
         ? Markdown.Insert(Selection.Caret, Caret)
         : Markdown.Insert(Selection.End, EndOfSelection).Insert(Selection.Start, StartOfSelection);
}
