using Avalonia;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.VisualTree;
using AvaloniaEdit.Document;
using Mavalonia.Markdown.Controls.Rendered;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.Markdown.Specifications;

namespace Mavalonia.Markdown.Controls.Specifications;

///<summary>A <see cref="MarkdownEditor"/> that a person is writing in. What it edits is said as <see cref="MarkdownWithSelection"/>.</summary>
class EditorInAWindow
{
   internal EditorInAWindow(string markdown)
   {
      var parsed = MarkdownWithSelection.Parse(markdown);
      Editor = new MarkdownEditor { Document = new TextDocument(parsed.Markdown), Padding = new Thickness(10) };
      Window = new HeadlessWindow(Editor);
      Editor.Selection = parsed.Selection;
      Editor.FocusTheView();
   }

   internal MarkdownEditor Editor { get; }
   internal HeadlessWindow Window { get; }

   internal RenderedEditorView RenderedView => Editor.GetVisualDescendants().OfType<RenderedEditorView>().Single();

   ///<summary>What the view tells an input method of the text and the caret, as Avalonia asks it on a desktop. The headless platform has no input method.</summary>
   internal TextInputMethodClient InputMethodClient
   {
      get
      {
         var request = new TextInputMethodClientRequestedEventArgs { RoutedEvent = InputElement.TextInputMethodClientRequestedEvent };
         RenderedView.RaiseEvent(request);
         return request.Client!;
      }
   }

   internal string Markdown => new MarkdownWithSelection(Editor.Document.Text, Editor.Selection).ToString();

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
