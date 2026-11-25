using AvaloniaEdit.Document;
using Mavalonia.MarkdownEditing.Editing;

namespace Mavalonia.MarkdownEditing.Specifications.Editing;

///<summary>Lets specifications say what is edited, and what comes of it, as markdown in which <c>‸</c> is the caret, or <c>⟦</c> and <c>⟧</c> the start and end of the selection.</summary>
static class Editing
{
   const string Caret = "‸";
   const string StartOfSelection = "⟦";
   const string EndOfSelection = "⟧";

   ///<summary>The markdown with its caret after doing <paramref name="edits"/> to <paramref name="markdown"/>.</summary>
   internal static string Of(string markdown, params Action<RenderedEditor>[] edits)
   {
      using var editor = EditorOf(markdown);
      foreach(var edit in edits) edit(editor);
      return MarkdownIn(editor);
   }

   internal static RenderedEditor EditorOf(string markdown)
   {
      var withoutCaret = markdown.Replace(Caret, "").Replace(StartOfSelection, "").Replace(EndOfSelection, "");
      var document = new TextDocument(withoutCaret);
      document.UndoStack.ClearAll();

      return new RenderedEditor(document, SelectionIn(markdown));
   }

   static TextSelection SelectionIn(string markdown) =>
      markdown.Contains(StartOfSelection, StringComparison.Ordinal)
         ? new TextSelection(Anchor: markdown.IndexOf(StartOfSelection, StringComparison.Ordinal), Caret: markdown.IndexOf(EndOfSelection, StringComparison.Ordinal) - 1)
         : TextSelection.CaretAt(markdown.IndexOf(Caret, StringComparison.Ordinal));

   static string MarkdownIn(RenderedEditor editor)
   {
      var selection = editor.Selection;
      return selection.IsEmpty
                ? editor.Document.Text.Insert(selection.Caret, Caret)
                : editor.Document.Text.Insert(selection.End, EndOfSelection).Insert(selection.Start, StartOfSelection);
   }

   ///<summary>Types <paramref name="text"/> as a person does: one character at a time, the line breaks with the Enter key.</summary>
   internal static Action<RenderedEditor> Type(string text) => editor =>
   {
      foreach(var character in text)
         if(character == '\n') editor.StartNewBlock();
         else editor.Type(character.ToString());
   };
}
