using Avalonia.Input;
using AvaloniaEdit.Document;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

public class Formatting_as_the_application_says
{
   static EditorInAWindow Toggle(TextFormats format, string markdown, bool inTheSource = false)
   {
      var editor = new EditorInAWindow(markdown) { Editor = { ShowsSource = inTheSource } };
      editor.Editor.FocusTheView();
      editor.Editor.ToggleFormat(format);
      return editor;
   }

   [XFOnUIThread] public void makes_the_selected_text_bold() => Toggle(TextFormats.Bold, "⟦a⟧ b").Markdown.Must().Be("⟦**a⟧** b");
   [XFOnUIThread] public void makes_the_selected_text_no_longer_bold_when_all_of_it_is() => Toggle(TextFormats.Bold, "**⟦a⟧** b").Markdown.Must().Be("⟦a⟧ b");
   [XFOnUIThread] public void formats_what_is_typed_next_when_nothing_is_selected() => Toggle(TextFormats.Italic, "a ‸").Type("b").Markdown.Must().Be("a *b‸*");
   [XFOnUIThread] public void is_undone_at_once() => Toggle(TextFormats.Bold, "⟦a⟧ b").Press(PhysicalKey.Z, RawInputModifiers.Control).Markdown.Must().Satisfy(it => it.Replace("‸", "", StringComparison.Ordinal) == "a b");
   [XFOnUIThread] public void makes_the_selected_text_bold_in_the_source_view() => Toggle(TextFormats.Bold, "⟦a⟧ b", inTheSource: true).Markdown.Must().Be("⟦**a⟧** b");
   [XFOnUIThread] public void does_nothing_in_the_source_view_when_nothing_is_selected() => Toggle(TextFormats.Bold, "a‸ b", inTheSource: true).Markdown.Must().Be("a‸ b");

   [XFOnUIThread] public void formats_the_selected_text_of_an_editor_that_is_not_shown()
   {
      var editor = new MarkdownEditor { Document = new TextDocument("a b"), Selection = new TextSelection(0, 1) };
      editor.ToggleFormat(TextFormats.Code);
      editor.Document.Text.Must().Be("`a` b");
   }
}
