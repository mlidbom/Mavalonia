using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

///<summary>A window, shown only in memory, in which a specification does what a person would do with mouse and keyboard, and sees what they would see.</summary>
class HeadlessWindow
{
   readonly Window _window;

   public HeadlessWindow(Control content, ThemeVariant? theme = null)
   {
      _window = new Window { Width = 800, Height = 600, Content = content, RequestedThemeVariant = theme ?? ThemeVariant.Light };
      _window.Show();
      DoWhatIsPending();
   }

   public IClipboard Clipboard => _window.Clipboard!;

   public Control Content
   {
      set
      {
         _window.Content = value;
         DoWhatIsPending();
      }
   }

   public void ClickTheCenterOf(Control control) => Click(control.TranslatePoint(new Rect(control.Bounds.Size).Center, _window)!.Value);

   public void Click(Point point, RawInputModifiers modifiers = RawInputModifiers.None)
   {
      _window.MouseDown(point, MouseButton.Left, modifiers);
      _window.MouseUp(point, MouseButton.Left, modifiers);
      DoWhatIsPending();
   }

   ///<summary>Presses the button, and doesn't release it.</summary>
   public void PressTheButton(Point point)
   {
      _window.MouseDown(point, MouseButton.Left);
      DoWhatIsPending();
   }

   public void ReleaseTheButton(Point point)
   {
      _window.MouseUp(point, MouseButton.Left);
      DoWhatIsPending();
   }

   public void Drag(Point from, Point to, RawInputModifiers modifiers = RawInputModifiers.None)
   {
      DragAndHold(from, to, modifiers);
      _window.MouseUp(to, MouseButton.Left, modifiers);
      DoWhatIsPending();
   }

   ///<summary>Drags, and doesn't release the button.</summary>
   public void DragAndHold(Point from, Point to, RawInputModifiers modifiers = RawInputModifiers.None)
   {
      _window.MouseDown(from, MouseButton.Left, modifiers);
      _window.MouseMove(to, RawInputModifiers.LeftMouseButton | modifiers);
      DoWhatIsPending();
   }

   public void RightClick(Point point, RawInputModifiers modifiers = RawInputModifiers.None)
   {
      _window.MouseDown(point, MouseButton.Right, modifiers);
      _window.MouseUp(point, MouseButton.Right, modifiers);
      DoWhatIsPending();
   }

   public void DragWithTheRightButton(Point from, Point to)
   {
      _window.MouseDown(from, MouseButton.Right);
      _window.MouseMove(to, RawInputModifiers.RightMouseButton);
      _window.MouseUp(to, MouseButton.Right);
      DoWhatIsPending();
   }

   public void MoveThePointerTo(Point point, RawInputModifiers modifiers = RawInputModifiers.None)
   {
      _window.MouseMove(point, modifiers);
      DoWhatIsPending();
   }

   ///<summary>Types <paramref name="text"/> as a person does: one character at a time.</summary>
   public void Type(string text)
   {
      foreach(var character in text) _window.KeyTextInput(character.ToString());
      DoWhatIsPending();
   }

   public void Press(PhysicalKey key, RawInputModifiers modifiers = RawInputModifiers.None, int times = 1)
   {
      for(var time = 0; time < times; time++)
      {
         _window.KeyPressQwerty(key, modifiers);
         _window.KeyReleaseQwerty(key, modifiers);
         DoWhatIsPending();
      }
   }

   ///<summary>Where <paramref name="point"/> of <paramref name="visual"/> is in the window.</summary>
   public Point PointOf(Visual visual, Point point) => visual.TranslatePoint(point, _window)!.Value;

   public RenderedFrame RenderFrame()
   {
      DoWhatIsPending();
      using var frame = _window.CaptureRenderedFrame()!;
      return new RenderedFrame(frame);
   }

   static void DoWhatIsPending()
   {
      Dispatcher.UIThread.RunJobs();
      AvaloniaHeadlessPlatform.ForceRenderTimerTick();
      Dispatcher.UIThread.RunJobs();
   }
}
