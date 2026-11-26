using AvaloniaEdit.Document;
using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.Markdown.Editing;

namespace Mavalonia.Markdown.Controls.Specifications;

public class When_a_markdown_editor_is_shown
{
   readonly MarkdownEditor _editor = new() { Watermark = "Write something" };
   readonly HeadlessWindow _window;
   readonly RenderedFrame _firstFrame;

   public When_a_markdown_editor_is_shown()
   {
      _window = new HeadlessWindow(_editor);
      _firstFrame = _window.RenderFrame();
   }

   [XFOnUIThread] public void its_document_is_empty() => _editor.Document.Text.Must().Be("");
   [XFOnUIThread] public void it_shows_what_is_rendered() => _editor.ShowsSource.Must().BeFalse();

   public class without_a_watermark : When_a_markdown_editor_is_shown
   {
      readonly RenderedFrame _frameWithoutWatermark;

      public without_a_watermark()
      {
         _editor.Watermark = null;
         _frameWithoutWatermark = _window.RenderFrame();
      }

      [XFOnUIThread] public void it_looks_different() => _frameWithoutWatermark.Fingerprint.Must().NotBe(_firstFrame.Fingerprint);
   }

   public class and_a_person_clicks_it_and_types : When_a_markdown_editor_is_shown
   {
      public and_a_person_clicks_it_and_types()
      {
         _window.ClickTheCenterOf(_editor);
         _window.Type("# A **heading**");
      }

      [XFOnUIThread] public void its_document_is_what_they_typed() => _editor.Document.Text.Must().Be("# A **heading**");
      [XFOnUIThread] public void it_looks_different() => _window.RenderFrame().Fingerprint.Must().NotBe(_firstFrame.Fingerprint);
   }

   public class and_it_is_given_another_document : When_a_markdown_editor_is_shown
   {
      readonly TextDocument _document = new("Some *emphasized* text");
      readonly RenderedFrame _frameOfTheDocument;

      public and_it_is_given_another_document()
      {
         _editor.Document = _document;
         _frameOfTheDocument = _window.RenderFrame();
      }

      [XFOnUIThread] public void it_looks_different() => _frameOfTheDocument.Fingerprint.Must().NotBe(_firstFrame.Fingerprint);
      [XFOnUIThread] public void its_caret_is_at_the_end_of_the_document() => _editor.Selection.Caret.Must().Be(_document.TextLength);

      [XFOnUIThread] public void it_shows_what_others_change_in_the_document()
      {
         _document.Insert(0, "More. ");
         _window.RenderFrame().Fingerprint.Must().NotBe(_frameOfTheDocument.Fingerprint);
      }
   }

   public class and_taken_out_of_its_window_and_put_back : When_a_markdown_editor_is_shown
   {
      public and_taken_out_of_its_window_and_put_back()
      {
         _editor.Document = new TextDocument("one two");
         _editor.Selection = TextSelection.CaretAt(3);
         _window.Content = new MarkdownEditor();
         _window.Content = _editor;
         _editor.FocusTheView();
      }

      [XFOnUIThread] public void its_caret_is_where_it_was() => _editor.Selection.Caret.Must().Be(3);

      [XFOnUIThread] public void it_can_be_written_in()
      {
         _window.Type("X");
         _editor.Document.Text.Must().Be("oneX two");
      }
   }
}
