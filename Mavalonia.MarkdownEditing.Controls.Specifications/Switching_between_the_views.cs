using Avalonia.Input;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.MarkdownEditing.Editing;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

public class Switching_between_the_views
{
   readonly EditorInAWindow _editor = new("# one\n\nsome **bo‸ld** text");
   readonly RenderedFrame _rendered;

   public Switching_between_the_views()
   {
      _rendered = _editor.Window.RenderFrame();
      _editor.Type("X");
      _editor.Editor.ShowsSource = true;
   }

   [XFOnUIThread] public void to_the_source_looks_different() => _editor.Window.RenderFrame().Fingerprint.Must().NotBe(_rendered.Fingerprint);
   [XFOnUIThread] public void to_the_source_leaves_the_caret_where_it_is() => _editor.Markdown.Must().Be("# one\n\nsome **boX‸ld** text");
   [XFOnUIThread] public void to_the_source_leaves_the_keyboard_focus_in_the_editor() => _editor.Type("Y").Markdown.Must().Be("# one\n\nsome **boXY‸ld** text");
   [XFOnUIThread] public void to_the_source_leaves_what_is_selected_selected() => SelectionAfterSwitchingThereAndBack("some ⟦**bold** te⟧xt").Must().Be("some ⟦**bold** te⟧xt");

   [XFOnUIThread] public void to_the_source_leaves_the_caret_at_the_start_of_what_is_selected_backwards()
   {
      var editor = new EditorInAWindow("‸abcdef") { Editor = { Selection = new TextSelection(Anchor: 5, Caret: 2), ShowsSource = true } };
      editor.Editor.Selection.Must().Be(new TextSelection(Anchor: 5, Caret: 2));
   }

   public class and_back : Switching_between_the_views
   {
      public and_back()
      {
         _editor.Type("Y");
         _editor.Editor.ShowsSource = false;
      }

      [XFOnUIThread] public void leaves_the_caret_where_it_is() => _editor.Markdown.Must().Be("# one\n\nsome **boXY‸ld** text");
      [XFOnUIThread] public void leaves_the_keyboard_focus_in_the_editor() => _editor.Type("Z").Markdown.Must().Be("# one\n\nsome **boXYZ‸ld** text");
      [XFOnUIThread] public void undoes_what_was_typed_in_either_view() => _editor.Press(PhysicalKey.Z, RawInputModifiers.Control, times: 2).Markdown.Must().Be("# one\n\nsome **bo‸ld** text");
   }

   static string SelectionAfterSwitchingThereAndBack(string markdown)
   {
      var editor = new EditorInAWindow(markdown) { Editor = { ShowsSource = true } };
      editor.Editor.ShowsSource = false;
      return editor.Markdown;
   }
}
