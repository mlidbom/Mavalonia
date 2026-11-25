using Avalonia.Styling;
using AvaloniaEdit.Document;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.MarkdownEditing.Editing;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

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

   public class and_released : When_a_markdown_editor_is_shown
   {
      [XFOnUIThread] public void its_document_does_not_keep_it_alive()
      {
         var document = new TextDocument("one");
         var released = ShowAndRelease(document);

         for(var attempt = 0; attempt < 5 && released.IsAlive; attempt++)
         {
            _window.RenderFrame();
            GC.Collect();
            GC.WaitForPendingFinalizers();
         }

         new { released.IsAlive, document.TextLength }.Must().Satisfy(it => !it.IsAlive);
      }

      WeakReference ShowAndRelease(TextDocument document)
      {
         var editor = new MarkdownEditor { Document = document };
         _window.Content = editor;
         _window.Content = new MarkdownEditor();
         return new WeakReference(editor);
      }
   }

   public class in_the_dark_theme : When_a_markdown_editor_is_shown
   {
      [XFOnUIThread] public void it_looks_different()
      {
         var document = "Some text";
         var light = new HeadlessWindow(new MarkdownEditor { Document = new TextDocument(document) }).RenderFrame();
         var dark = new HeadlessWindow(new MarkdownEditor { Document = new TextDocument(document) }, ThemeVariant.Dark).RenderFrame();
         dark.Fingerprint.Must().NotBe(light.Fingerprint);
      }
   }
}
