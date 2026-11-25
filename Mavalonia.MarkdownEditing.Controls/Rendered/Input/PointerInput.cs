using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Input;

///<summary>What a person does with the pointer in a <see cref="RenderedMarkdownView"/>.</summary>
class PointerInput
{
   ///<summary>How far the pointer may move while a button is pressed without that being more than a click.</summary>
   const double FarthestInAClick = 4;

   readonly RenderedMarkdownView _view;

   ///<summary>Goes on selecting, and scrolling, while the pointer is held still outside of what is seen.</summary>
   readonly DispatcherTimer _selecting = new() { Interval = TimeSpan.FromMilliseconds(30) };

   ///<summary>Whether the button that selects is pressed.</summary>
   bool _isPressed;

   ///<summary>Null when the button isn't pressed, or when it has become more than a click.</summary>
   Point? _pressedAt;

   Point _latestInTheWindow;

   internal PointerInput(RenderedMarkdownView view)
   {
      _view = view;
      _selecting.Tick += (_, _) => SelectUpTo(_latestInTheWindow);
   }

   internal void Press(PointerPressedEventArgs e)
   {
      if(_view.Selectable is not {} selectable || !e.GetCurrentPoint(_view).Properties.IsLeftButtonPressed) return;

      e.Handled = true;
      _view.Focus(NavigationMethod.Pointer);
      e.Pointer.Capture(_view);
      _view.SelectingStarts();
      _isPressed = true;
      _pressedAt = e.GetPosition(_view);

      var position = _view.Layout.PositionNearestTo(e.GetPosition(_view));
      switch(e.ClickCount)
      {
         case 2: selectable.SelectTheWordAt(position); break;
         case 3: selectable.SelectTheBlockAt(position); break;
         default: selectable.MoveTo(position, extendSelection: e.KeyModifiers.HasFlag(KeyModifiers.Shift)); break;
      }
   }

   internal void Move(PointerEventArgs e)
   {
      if(!_isPressed) _view.PointAt(e.GetPosition(_view));
      else if(!IsNoMoreThanAClick(e.GetPosition(_view))) SelectUpTo(e);
   }

   bool IsNoMoreThanAClick(Point point) => _pressedAt is {} pressed && Point.Distance(pressed, point) < FarthestInAClick;

   void SelectUpTo(PointerEventArgs e)
   {
      _pressedAt = null;
      _latestInTheWindow = e.GetPosition(TopLevel.GetTopLevel(_view));
      _selecting.Start();
      SelectUpTo(_latestInTheWindow);
   }

   void SelectUpTo(Point inTheWindow)
   {
      if(_view.Selectable is not {} selectable || TopLevel.GetTopLevel(_view)?.TranslatePoint(inTheWindow, _view) is not {} point) return;

      var position = _view.Layout.PositionNearestTo(point);
      if(position.Offset == selectable.Selection.Caret) return;

      selectable.MoveTo(position, extendSelection: true);
      _view.BringTheCaretIntoView();
   }

   internal void Release(PointerReleasedEventArgs e)
   {
      var isAClick = e.InitialPressMouseButton == MouseButton.Left && _pressedAt is not null && _view.Selectable?.Selection.IsEmpty == true;
      var point = e.GetPosition(_view);

      e.Pointer.Capture(null);
      EndSelecting();
      if(isAClick) _view.ClickAt(point);
   }

   internal void EndSelecting()
   {
      _isPressed = false;
      _pressedAt = null;
      _selecting.Stop();
   }
}
