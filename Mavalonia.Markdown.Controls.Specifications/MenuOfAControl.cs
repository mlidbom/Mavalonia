using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Headless;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.Markdown.Controls.Specifications;

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

   ///<summary>A picture of the menu as it is shown.</summary>
   internal RenderedFrame RenderFrame()
   {
      HeadlessWindow.DoWhatIsPending();
      using var frame = TopLevel.GetTopLevel(_menu.Popup.Child)!.CaptureRenderedFrame()!;
      return new RenderedFrame(frame);
   }

   ///<summary>Those that are seen.</summary>
   IEnumerable<MenuItem> Items => _menu.Popup.Child!.GetVisualDescendants().OfType<MenuItem>().Where(it => it is { IsEffectivelyVisible: true, Bounds: { Width: > 50, Height: > 10 } });

   ///<summary>What a person sees in the menu: each item, with the keys that do the same.</summary>
   ///<remarks>The keys are told as the menu tells them: as the platform does, such as <c>8</c> for <see cref="Avalonia.Input.Key.D8"/>.</remarks>
   internal IReadOnlyList<string> Shown => [..Items.Select(it => it.InputGesture is {} keys ? $"{it.Header} ({PlatformKeyGestureConverter.ToPlatformString(keys)})" : $"{it.Header}")];

   ///<summary>Those that are shown with a check mark: that are on.</summary>
   internal IReadOnlyList<string> Checked => [..Items.Where(it => it.IsChecked).Select(it => $"{it.Header}")];

   ///<summary>What a person can choose now.</summary>
   internal IReadOnlyList<string> Choices => [..Items.Where(it => it.IsEnabled).Select(it => $"{it.Header}")];

   ///<summary>Does what a click on the item does: what the item is for, and closes the menu.</summary>
   internal async Task Choose(string header)
   {
      Items.Single(it => it.IsEnabled && (string?)it.Header == header).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
      Close();
      await Task.Yield();
      Dispatcher.UIThread.RunJobs();
   }
}
