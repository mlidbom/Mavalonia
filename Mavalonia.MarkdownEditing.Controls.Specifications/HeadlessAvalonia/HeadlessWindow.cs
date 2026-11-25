using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

///<summary>A window, shown only in memory, in which a specification does what a person would do with mouse and keyboard, and sees what they would see.</summary>
class HeadlessWindow
{
   readonly Window _window;

   public HeadlessWindow(Control content)
   {
      _window = new Window { Width = 800, Height = 600, Content = content };
      _window.Show();
   }

   public void ClickTheCenterOf(Control control)
   {
      var center = control.TranslatePoint(new Rect(control.Bounds.Size).Center, _window)!.Value;
      _window.MouseDown(center, MouseButton.Left);
      _window.MouseUp(center, MouseButton.Left);
   }

   public void Type(string text) => _window.KeyTextInput(text);

   public RenderedFrame RenderFrame()
   {
      using var frame = _window.CaptureRenderedFrame()!;
      return new RenderedFrame(frame);
   }
}
