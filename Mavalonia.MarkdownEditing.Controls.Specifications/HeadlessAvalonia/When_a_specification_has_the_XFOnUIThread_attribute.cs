using Avalonia;
using Avalonia.Threading;
using Compze.Must;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

public class When_a_specification_has_the_XFOnUIThread_attribute
{
   readonly bool _itsConstructorRanOnTheUIThread = IsOnTheUIThread;

   static bool IsOnTheUIThread => Application.Current is HeadlessApplication && Dispatcher.UIThread.CheckAccess();

   [XFOnUIThread] public void its_constructor_runs_on_the_UI_thread() => _itsConstructorRanOnTheUIThread.Must().BeTrue();
   [XFOnUIThread] public void it_runs_on_the_UI_thread() => IsOnTheUIThread.Must().BeTrue();

   [XFOnUIThread] public async Task it_continues_on_the_UI_thread_after_awaiting()
   {
      await Task.Delay(1);
      IsOnTheUIThread.Must().BeTrue();
   }

   [XFOnUIThread] public void it_runs_in_the_class_that_declares_it_and_not_in_those_that_inherit_it() => GetType().Must().Be(typeof(When_a_specification_has_the_XFOnUIThread_attribute));

   public class and_a_class_inherits_it : When_a_specification_has_the_XFOnUIThread_attribute
   {
      [XFOnUIThread] public void the_constructor_it_inherits_runs_on_the_UI_thread() => _itsConstructorRanOnTheUIThread.Must().BeTrue();
   }
}
