using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.Markdown.Controls.Specifications;

public class A_released_markdown_view
{
   readonly HeadlessWindow _window = new(new MarkdownView());
   readonly WeakReference _released;

   public A_released_markdown_view() => _released = ShowAndRelease();

   WeakReference ShowAndRelease()
   {
      var view = new MarkdownView { Markdown = "one\n\n```cs\ntwo\n```" };
      _window.Content = view;
      view.SelectAll();
      _window.RenderFrame();
      _window.Content = new MarkdownView();
      return new WeakReference(view);
   }

   [XFOnUIThread] public async Task is_not_kept_alive_by_what_it_showed_or_by_what_was_selected_in_it()
   {
      for(var attempt = 0; attempt < 40 && _released.IsAlive; attempt++)
      {
         await Task.Delay(50);
         _window.RenderFrame();
         GC.Collect();
         GC.WaitForPendingFinalizers();
      }

      new { _released.IsAlive }.Must().Satisfy(it => !it.IsAlive);
   }
}
