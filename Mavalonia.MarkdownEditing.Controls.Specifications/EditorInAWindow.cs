using Avalonia;
using Avalonia.Input;
using Avalonia.VisualTree;
using AvaloniaEdit.Document;
using Mavalonia.MarkdownEditing.Controls.Rendered;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.MarkdownEditing.Editing;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

///<summary>A <see cref="MarkdownEditor"/> that a person is writing in. Its markdown is said with <c>‸</c> for the caret, or <c>⟦</c> and <c>⟧</c> for the start and end of what is selected.</summary>
class EditorInAWindow
{
   const string Caret = "‸";
   const string StartOfSelection = "⟦";
   const string EndOfSelection = "⟧";

   internal EditorInAWindow(string markdown)
   {
      var withoutSelection = markdown.Replace(Caret, "").Replace(StartOfSelection, "").Replace(EndOfSelection, "");
      Editor = new MarkdownEditor { Document = new TextDocument(withoutSelection), Padding = new Thickness(10) };
      Window = new HeadlessWindow(Editor);
      Editor.Selection = markdown.Contains(StartOfSelection, StringComparison.Ordinal)
                            ? new TextSelection(markdown.IndexOf(StartOfSelection, StringComparison.Ordinal), markdown.IndexOf(EndOfSelection, StringComparison.Ordinal) - 1)
                            : TextSelection.CaretAt(markdown.IndexOf(Caret, StringComparison.Ordinal));
      Editor.FocusTheView();
   }

   internal MarkdownEditor Editor { get; }
   internal HeadlessWindow Window { get; }

   RenderedMarkdownView RenderedView => Editor.GetVisualDescendants().OfType<RenderedMarkdownView>().Single();

   internal string Markdown
   {
      get
      {
         var selection = Editor.Selection;
         return selection.IsEmpty ? Editor.Document.Text.Insert(selection.Caret, Caret) : Editor.Document.Text.Insert(selection.End, EndOfSelection).Insert(selection.Start, StartOfSelection);
      }
   }

   internal EditorInAWindow Type(string text)
   {
      Window.Type(text);
      return this;
   }

   internal EditorInAWindow Press(PhysicalKey key, RawInputModifiers modifiers = RawInputModifiers.None, int times = 1)
   {
      Window.Press(key, modifiers, times);
      return this;
   }

   ///<summary>A point in the window, in the middle of the line of text number <paramref name="line"/> of a document of paragraphs of one line, <paramref name="x"/> from the start of the text.</summary>
   internal Point PointInTheText(double x, int line = 0) => Window.PointOf(RenderedView, new Point(10 + x, 10 + 8 + line * 25));
}
