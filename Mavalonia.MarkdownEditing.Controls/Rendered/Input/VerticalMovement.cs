using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Input;

///<summary>Moves the caret up and down, and remembers how far to the right it was before it passed shorter lines.</summary>
class VerticalMovement
{
   readonly RenderedMarkdownView _view;
   double _x;

   ///<summary>The selection after the last movement. When it's another, the caret has been moved otherwise since.</summary>
   TextSelection? _selectionAfterTheLast;

   internal VerticalMovement(RenderedMarkdownView view) => _view = view;

   internal void Up(RenderedEditor editor, bool extendSelection) =>
      Move(editor, extendSelection, _view.Layout.LineAbove(editor.CaretPosition, XOf(editor))?.PositionNearestTo(XOf(editor)) ?? RenderedPosition.AtTheStartOf(editor.Rendered.Blocks[0]));

   internal void Down(RenderedEditor editor, bool extendSelection) =>
      Move(editor, extendSelection, _view.Layout.LineBelow(editor.CaretPosition, XOf(editor))?.PositionNearestTo(XOf(editor)) ?? RenderedPosition.AtTheEndOf(editor.Rendered.Blocks[^1]));

   double XOf(RenderedEditor editor)
   {
      if(editor.Selection != _selectionAfterTheLast) _x = _view.CaretRectangle.X;
      return _x;
   }

   void Move(RenderedEditor editor, bool extendSelection, RenderedPosition position)
   {
      editor.MoveTo(position, extendSelection);
      _selectionAfterTheLast = editor.Selection;
   }
}
