using Avalonia.Controls;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.Reading_markdown;

public class A_view_that_is_out_of_sight
{
   readonly MarkdownView _view = new() { Markdown = "a" };
   readonly ScrollViewer _scrollViewer;
   readonly HeadlessWindow _window;

   public A_view_that_is_out_of_sight()
   {
      _scrollViewer = new ScrollViewer { Content = new StackPanel { Children = { new Border { Height = 3000 }, _view, new Border { Height = 3000 } } } };
      _window = new HeadlessWindow(_scrollViewer);
   }

   double Offset
   {
      get
      {
         _window.RenderFrame();
         return _scrollViewer.Offset.Y;
      }
   }

   [XFOnUIThread] public void stays_there_when_the_application_gives_it_the_focus()
   {
      _view.Focus();
      Offset.Must().Be(0.0);
   }

   [XFOnUIThread] public void is_scrolled_to_when_the_application_brings_it_into_view()
   {
      _view.BringIntoView();
      Offset.Must().BeGreaterThan(2000);
   }

   [XFOnUIThread] public void is_scrolled_to_when_the_application_gives_it_the_focus_and_then_brings_it_into_view()
   {
      _view.Focus();
      _view.BringIntoView();
      Offset.Must().BeGreaterThan(2000);
   }
}
