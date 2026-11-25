using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

///<summary>The menu that one of our controls has when the application gives it no other.</summary>
class MenuOfAControl
{
   readonly Control _control;
   readonly MenuFlyout _menu;

   internal MenuOfAControl(Control control)
   {
      _control = control;
      _menu = (MenuFlyout)control.ContextFlyout!;
   }

   internal bool IsOpen => _menu.IsOpen;

   ///<summary>The corner of the menu, from that of the control.</summary>
   internal Point Place => _control.PointToClient(_menu.Popup.Child!.PointToScreen(default));

   internal void Close() => _menu.Hide();

   ///<summary>Those that are seen.</summary>
   IEnumerable<MenuItem> Items => _menu.Popup.Child!.GetVisualDescendants().OfType<MenuItem>().Where(it => it is { IsEffectivelyVisible: true, Bounds: { Width: > 50, Height: > 10 } });

   ///<summary>What a person sees in the menu: each item, with the keys that do the same.</summary>
   internal IReadOnlyList<string> Shown => [..Items.Select(it => it.InputGesture is {} keys ? $"{it.Header} ({keys})" : $"{it.Header}")];

   ///<summary>What a person can choose now.</summary>
   internal IReadOnlyList<string> Choices => [..Items.Where(it => it.IsEnabled).Select(it => $"{it.Header}")];

   internal async Task Choose(string header)
   {
      Items.Single(it => it.IsEnabled && (string?)it.Header == header).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
      await Task.Yield();
      Dispatcher.UIThread.RunJobs();
   }
}
