using AvaloniaEdit.Document;
using Mavalonia.Markdown.Editing;
using Mavalonia.Markdown.Rendering;

namespace Mavalonia.Markdown.Specifications.Editing;

///<summary>Lets specifications say what is edited, and what comes of it, as <see cref="MarkdownWithSelection"/>.</summary>
static class Editing
{
   ///<summary>The markdown with its caret after doing <paramref name="edits"/> to <paramref name="markdown"/>.</summary>
   internal static string Of(string markdown, params Action<RenderedEditor>[] edits)
   {
      using var editor = EditorOf(markdown);
      foreach(var edit in edits) edit(editor);
      return new MarkdownWithSelection(editor.Document.Text, editor.Selection).ToString();
   }

   ///<summary>The markdown after doing <paramref name="edit"/> to <paramref name="markdown"/>, with the stretch that the edit says it wrote written as selected.</summary>
   internal static string WrittenBy(Func<RenderedEditor, SourceRange> edit, string markdown)
   {
      using var editor = EditorOf(markdown);
      var written = edit(editor);
      return new MarkdownWithSelection(editor.Document.Text, new TextSelection(written.Start, written.End)).ToString();
   }

   internal static RenderedEditor EditorOf(string markdown)
   {
      var parsed = MarkdownWithSelection.Parse(markdown);
      var document = new TextDocument(parsed.Markdown);
      document.UndoStack.ClearAll();

      return new RenderedEditor(document, parsed.Selection);
   }

   ///<summary>Types <paramref name="text"/> as a person does: one character at a time, the line breaks with the Enter key.</summary>
   internal static Action<RenderedEditor> Type(string text) => editor =>
   {
      foreach(var character in text)
         if(character == '\n') editor.StartNewBlock();
         else editor.Type(character.ToString());
   };
}
