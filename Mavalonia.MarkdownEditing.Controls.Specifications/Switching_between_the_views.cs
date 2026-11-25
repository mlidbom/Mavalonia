using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Document;
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

   [XFOnUIThread] public void to_the_source_shows_URLs_in_the_colour_of_the_text_also_in_the_dark_theme()
   {
      var editor = new MarkdownEditor { Document = new TextDocument("see https://a.b now"), ShowsSource = true };
      var window = new HeadlessWindow(editor, ThemeVariant.Dark);
      window.RenderFrame().SaveForInspectionAs("the_source_in_the_dark_theme");

      var source = editor.GetVisualDescendants().OfType<TextEditor>().Single();
      new { URL = $"{source.TextArea.TextView.LinkTextForegroundBrush}", Text = $"{source.Foreground}" }.Must().Satisfy(it => it.URL == it.Text);
   }

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

   public class with_the_caret_at_the_end_of_a_document_that_is_longer_than_the_editor
   {
      readonly EditorInAWindow _editor = new(string.Join("\n\n", Enumerable.Repeat("text", 100)) + "‸");

      [XFOnUIThread] public void to_the_source_shows_the_caret_there()
      {
         _editor.Editor.ShowsSource = true;
         var source = _editor.Editor.GetVisualDescendants().OfType<TextEditor>().Single();
         new { source.VerticalOffset, source.ViewportHeight, source.ExtentHeight }.Must().Satisfy(it => it.VerticalOffset + it.ViewportHeight > it.ExtentHeight - LessThanALine);
      }

      [XFOnUIThread] public void to_the_rendered_view_shows_the_caret_there()
      {
         _editor.Editor.ShowsSource = true;
         _editor.Press(PhysicalKey.Home, RawInputModifiers.Control);
         _editor.Editor.ShowsSource = false;
         _editor.Editor.ShowsSource = true;
         _editor.Press(PhysicalKey.End, RawInputModifiers.Control);
         _editor.Editor.ShowsSource = false;

         var scroll = _editor.RenderedView.FindAncestorOfType<ScrollViewer>()!;
         new { scroll.Offset.Y, scroll.Viewport.Height, HeightOfAll = scroll.Extent.Height }.Must().Satisfy(it => it.Y + it.Height > it.HeightOfAll - 2 * LessThanALine);
      }

      const double LessThanALine = 15;
   }
}
