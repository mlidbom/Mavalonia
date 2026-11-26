using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;

namespace Mavalonia.Markdown.Controls.Measurements;

///<summary>A window that shows messages one below the other, as a chat does.</summary>
class Chat
{
   const double Height = 900;

   readonly Window _window;
   readonly ScrollViewer _scroll;
   readonly StackPanel _messages = new() { Spacing = 8 };

   internal Chat(IEnumerable<string> messages)
   {
      foreach(var message in messages) _messages.Children.Add(new MarkdownView { Markdown = message });
      _scroll = new ScrollViewer { Content = _messages };
      _window = new Window { Width = 800, Height = Height, Content = _scroll };
      _window.Show();
      DoWhatIsPending();
   }

   internal void ScrollTo(double fromTheEnd)
   {
      _scroll.Offset = new Vector(0, Math.Max(0, _scroll.Extent.Height - Height - fromTheEnd));
      Draw();
   }

   internal void Draw()
   {
      DoWhatIsPending();
      _window.CaptureRenderedFrame()?.Dispose();
   }

   internal string TextOfTheLast { set => ((MarkdownView)_messages.Children[^1]).Markdown = value; }

   internal double Width { set => _window.Width = value; }

   internal void Close() => _window.Close();

   static void DoWhatIsPending()
   {
      Dispatcher.UIThread.RunJobs();
      AvaloniaHeadlessPlatform.ForceRenderTimerTick();
      Dispatcher.UIThread.RunJobs();
   }
}
