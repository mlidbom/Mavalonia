using AvaloniaEdit.Document;
using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.Markdown.Controls.Specifications;

public class A_released_markdown_editor
{
   readonly HeadlessWindow _window = new(new MarkdownEditor());
   readonly TextDocument _document = new("one");
   readonly WeakReference _released;

   public A_released_markdown_editor() => _released = ShowAndRelease();

   WeakReference ShowAndRelease()
   {
      var editor = new MarkdownEditor { Document = _document };
      _window.Content = editor;
      _window.Content = new MarkdownEditor();
      return new WeakReference(editor);
   }

   [XFOnUIThread] public void is_not_kept_alive_by_its_document()
   {
      for(var attempt = 0; attempt < 5 && _released.IsAlive; attempt++)
      {
         _window.RenderFrame();
         GC.Collect();
         GC.WaitForPendingFinalizers();
      }

      new { _released.IsAlive, _document.TextLength }.Must().Satisfy(it => !it.IsAlive);
   }
}
