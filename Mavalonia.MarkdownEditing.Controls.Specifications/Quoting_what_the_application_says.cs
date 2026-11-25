using Avalonia.Input;
using AvaloniaEdit.Document;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.MarkdownEditing.Editing;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

public class Quoting_what_the_application_says
{
   static EditorInAWindow Quote(string into, bool inTheSource = false)
   {
      var editor = new EditorInAWindow(into) { Editor = { ShowsSource = inTheSource } };
      editor.Editor.Quote("**x**");
      return editor;
   }

   [XFOnUIThread] public void adds_the_quote_and_puts_the_caret_after_it() => Quote("a‸").Markdown.Must().Be("a\n\n> **x**\n\n‸");
   [XFOnUIThread] public void and_then_typing_writes_after_the_quote() => Quote("‸").Type("y").Markdown.Must().Be("> **x**\n\ny‸");
   [XFOnUIThread] public void is_undone_at_once() => Quote("a‸").Press(PhysicalKey.Z, RawInputModifiers.Control).Markdown.Must().Be("a‸");
   [XFOnUIThread] public void in_the_source_view_puts_the_caret_after_it_there() => Quote("a‸", inTheSource: true).Markdown.Must().Be("a\n\n> **x**\n\n‸");

   [XFOnUIThread] public void into_an_editor_that_is_not_shown_adds_the_quote_and_puts_the_caret_after_it()
   {
      var editor = new MarkdownEditor { Document = new TextDocument("a") };
      editor.Quote("x");
      new { editor.Document.Text, editor.Selection }.Must().Satisfy(it => it.Text == "a\n\n> x\n\n" && it.Selection == TextSelection.CaretAt(it.Text.Length));
   }
}
