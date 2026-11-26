using Mavalonia.Markdown.Editing;
using Mavalonia.Markdown.Rendering;

namespace Mavalonia.Markdown.Controls.Rendered.Input;

///<summary>Moves the caret up and down, and remembers how far to the right it was before it passed shorter lines.</summary>
class VerticalMovement
{
   readonly RenderedMarkdownView _view;
   double _x;

   ///<summary>The selection after the last movement. When it's another, the caret has been moved otherwise since.</summary>
   TextSelection? _selectionAfterTheLast;

   internal VerticalMovement(RenderedMarkdownView view) => _view = view;

   internal void Up(SelectableMarkdown selectable, bool extendSelection) =>
      Move(selectable, extendSelection, _view.Layout.LineAbove(selectable.CaretPosition, XOf(selectable))?.PositionNearestTo(XOf(selectable)) ?? selectable.Rendered.Start);

   internal void Down(SelectableMarkdown selectable, bool extendSelection) =>
      Move(selectable, extendSelection, _view.Layout.LineBelow(selectable.CaretPosition, XOf(selectable))?.PositionNearestTo(XOf(selectable)) ?? selectable.Rendered.End);

   double XOf(SelectableMarkdown selectable)
   {
      if(selectable.Selection != _selectionAfterTheLast) _x = _view.CaretRectangle.X;
      return _x;
   }

   void Move(SelectableMarkdown selectable, bool extendSelection, RenderedPosition position)
   {
      selectable.MoveTo(position, extendSelection);
      _selectionAfterTheLast = selectable.Selection;
   }
}
