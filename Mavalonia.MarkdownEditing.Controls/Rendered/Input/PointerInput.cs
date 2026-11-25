using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Input;

///<summary>What a person does with the pointer in a <see cref="RenderedMarkdownView"/>.</summary>
class PointerInput
{
   ///<summary>How far the pointer may move while a button is pressed without that being more than a click.</summary>
   const double FarthestInAClick = 4;

   readonly RenderedMarkdownView _view;

   ///<summary>Goes on selecting, and scrolling, while the pointer is held still outside of what is seen.</summary>
   readonly DispatcherTimer _selecting = new() { Interval = TimeSpan.FromMilliseconds(30) };

   ///<summary>Null when no button is pressed, or when it has become more than a click.</summary>
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
      _pressedAt = e.GetPosition(_view);

      var position = _view.Layout.PositionNearestTo(e.GetPosition(_view));
      switch(e.ClickCount)
      {
         case 2: SelectTheWordAt(position, selectable); break;
         case 3: Select(selectable, RenderedPosition.AtTheStartOf(position.Block), RenderedPosition.AtTheEndOf(position.Block)); break;
         default: selectable.MoveTo(position, extendSelection: e.KeyModifiers.HasFlag(KeyModifiers.Shift)); break;
      }
   }

   static void SelectTheWordAt(RenderedPosition position, SelectableMarkdown selectable)
   {
      var boundaries = position.Block.Text.Boundaries;
      var end = position.IsAtTheEndOfItsBlock ? position.Index : boundaries.EndOfWordWithoutSpacesAfter(position.Index);
      var start = boundaries.StartOfWordBefore(end);

      Select(selectable, position with { Index = start }, position with { Index = end });
   }

   static void Select(SelectableMarkdown selectable, RenderedPosition start, RenderedPosition end)
   {
      selectable.MoveTo(start);
      selectable.MoveTo(end, extendSelection: true);
   }

   internal void Move(PointerEventArgs e)
   {
      if(!ReferenceEquals(e.Pointer.Captured, _view)) _view.PointAt(e.GetPosition(_view));
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
      _pressedAt = null;
      _selecting.Stop();
   }
}
