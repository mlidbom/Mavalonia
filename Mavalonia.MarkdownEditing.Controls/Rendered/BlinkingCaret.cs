using Avalonia.Threading;

namespace Mavalonia.MarkdownEditing.Controls.Rendered;

///<summary>Decides when a caret is seen: it blinks, but not while it's being moved.</summary>
class BlinkingCaret
{
   readonly DispatcherTimer _timer;

   internal BlinkingCaret(Action changed)
   {
      _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(530) };
      _timer.Tick += (_, _) =>
      {
         IsSeen = !IsSeen;
         changed();
      };
   }

   internal bool IsSeen { get; private set; }

   ///<summary>Shows the caret, and has it blink from now on.</summary>
   internal void Show()
   {
      _timer.Stop();
      _timer.Start();
      IsSeen = true;
   }

   internal void Hide()
   {
      _timer.Stop();
      IsSeen = false;
   }
}
