using System.Runtime.CompilerServices;
using Avalonia.Controls;

namespace Mavalonia.Markdown.Controls.Rendered;

///<summary>In a window, something is selected in no more than one <see cref="MarkdownView"/> at a time, as a person expects of what they read as one text.</summary>
class SelectionOfAWindow
{
   static readonly ConditionalWeakTable<TopLevel, SelectionOfAWindow> OfWindows = [];

   WeakReference<MarkdownView>? _view;

   internal static void IsNowIn(MarkdownView view)
   {
      if(TopLevel.GetTopLevel(view) is {} window) OfWindows.GetOrCreateValue(window).MoveTo(view);
   }

   void MoveTo(MarkdownView view)
   {
      if(_view?.TryGetTarget(out var previous) == true && previous != view) previous.SelectNothing();
      _view = new WeakReference<MarkdownView>(view);
   }
}
