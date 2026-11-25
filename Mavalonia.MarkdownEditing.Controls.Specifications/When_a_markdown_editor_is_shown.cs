using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

public class When_a_markdown_editor_is_shown
{
   readonly MarkdownEditor _editor = new();
   readonly HeadlessWindow _window;
   readonly RenderedFrame _firstFrame;

   public When_a_markdown_editor_is_shown()
   {
      _window = new HeadlessWindow(_editor);
      _firstFrame = _window.RenderFrame();
   }

   [XFOnUIThread] public void its_markdown_is_empty() => _editor.Markdown.Must().Be("");

   public class and_a_person_clicks_it_and_types : When_a_markdown_editor_is_shown
   {
      readonly RenderedFrame _frameAfterTyping;

      public and_a_person_clicks_it_and_types()
      {
         _window.ClickTheCenterOf(_editor);
         _window.Type("# A heading");
         _frameAfterTyping = _window.RenderFrame();
         _frameAfterTyping.SaveForInspectionAs("A heading typed into the markdown editor");
      }

      [XFOnUIThread] public void its_markdown_is_the_text_they_typed() => _editor.Markdown.Must().Be("# A heading");
      [XFOnUIThread] public void it_looks_different() => _frameAfterTyping.Fingerprint.Must().NotBe(_firstFrame.Fingerprint);
   }

   public class and_its_markdown_is_set : When_a_markdown_editor_is_shown
   {
      readonly RenderedFrame _frameAfterSettingMarkdown;

      public and_its_markdown_is_set()
      {
         _editor.Markdown = "Some *emphasized* text";
         _frameAfterSettingMarkdown = _window.RenderFrame();
      }

      [XFOnUIThread] public void it_looks_different() => _frameAfterSettingMarkdown.Fingerprint.Must().NotBe(_firstFrame.Fingerprint);
   }
}
