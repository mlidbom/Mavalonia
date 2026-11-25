using Avalonia.Input;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Input;

///<summary>What a person does with the pointer in a <see cref="RenderedMarkdownView"/>.</summary>
class PointerInput
{
   readonly RenderedMarkdownView _view;

   internal PointerInput(RenderedMarkdownView view) => _view = view;

   internal void Press(PointerPressedEventArgs e)
   {
      if(_view.Editor is not {} editor || !e.GetCurrentPoint(_view).Properties.IsLeftButtonPressed) return;

      e.Handled = true;
      _view.Focus(NavigationMethod.Pointer);
      e.Pointer.Capture(_view);

      var position = _view.Layout.PositionNearestTo(e.GetPosition(_view));
      switch(e.ClickCount)
      {
         case 2: SelectTheWordAt(position, editor); break;
         case 3: Select(editor, RenderedPosition.AtTheStartOf(position.Block), RenderedPosition.AtTheEndOf(position.Block)); break;
         default: editor.MoveTo(position, extendSelection: e.KeyModifiers.HasFlag(KeyModifiers.Shift)); break;
      }
   }

   static void SelectTheWordAt(RenderedPosition position, RenderedEditor editor)
   {
      var text = position.Block.Text;
      var end = position.IsAtTheEndOfItsBlock ? position.Index : text.EndOfWordWithoutSpacesAfter(position.Index);
      var start = text.StartOfWordBefore(end);

      Select(editor, position with { Index = start }, position with { Index = end });
   }

   static void Select(RenderedEditor editor, RenderedPosition start, RenderedPosition end)
   {
      editor.MoveTo(start);
      editor.MoveTo(end, extendSelection: true);
   }

   internal void Move(PointerEventArgs e)
   {
      if(_view.Editor is not {} editor || !ReferenceEquals(e.Pointer.Captured, _view)) return;

      editor.MoveTo(_view.Layout.PositionNearestTo(e.GetPosition(_view)), extendSelection: true);
      _view.BringTheCaretIntoView();
   }

   internal static void Release(PointerReleasedEventArgs e) => e.Pointer.Capture(null);
}
