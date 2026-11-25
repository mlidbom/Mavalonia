using Avalonia;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.Reading_markdown;

public class A_view_that_is_higher_than_the_window
{
   [XFOnUIThread] public void does_not_scroll_when_it_is_clicked()
   {
      var view = new ViewInAWindow(string.Join("\n\n", Enumerable.Range(0, 100).Select(it => $"line{it}"))) { ScrollViewer = { Offset = new Vector(0, 500) } };
      view.Window.RenderFrame();
      view.Window.Click(new Point(100, 300));
      view.ScrollViewer.Offset.Must().Be(new Vector(0, 500));
   }
}
