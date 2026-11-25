using Avalonia.Input;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Input;

///<summary>What a person does with the keyboard in a <see cref="MarkdownView"/>. Keys that only move a caret are left to others, such as to scroll, since there is none to see.</summary>
class ReadingKeys
{
   readonly MarkdownView _view;
   readonly MovementKeys _movement;

   internal ReadingKeys(MarkdownView view)
   {
      _view = view;
      _movement = new MovementKeys(view);
   }

   internal void Press(KeyEventArgs e)
   {
      if(ActionOf(e) is not {} action) return;

      e.Handled = true;
      action();
   }

   Action? ActionOf(KeyEventArgs e)
   {
      if(_view.Hotkeys.SelectAll.Any(it => it.Matches(e))) return _view.SelectAll;
      if(_view.IsAnythingSelected && _view.Hotkeys.Copy.Any(it => it.Matches(e))) return () => _view.CopySelection().SurfaceExceptions();
      if(_view.IsAnythingSelected && e is { Key: Key.Escape, KeyModifiers: KeyModifiers.None }) return _view.SelectNothing;
      return e.KeyModifiers.HasFlag(KeyModifiers.Shift) ? ExtensionOfTheSelection(e) : null;
   }

   Action? ExtensionOfTheSelection(KeyEventArgs e) =>
      _movement.ActionOf(e, _view.Selectable!) is {} move
         ? () =>
         {
            _view.SelectingStarts();
            move();
            _view.BringTheCaretIntoView();
         }
         : null;
}
