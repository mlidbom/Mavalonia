using Avalonia;
using Avalonia.Input.TextInput;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Input;

///<summary>Tells an input method, such as one for Japanese, what it needs to know of a <see cref="RenderedEditorView"/>: where to show what is being composed.</summary>
class InputMethodClient : TextInputMethodClient
{
   readonly RenderedEditorView _view;

   internal InputMethodClient(RenderedEditorView view) => _view = view;

   public override Visual TextViewVisual => _view;
   public override Rect CursorRectangle => _view.CaretRectangle;

   ///<summary>The input method shows what is being composed itself.</summary>
   public override bool SupportsPreedit => false;

   public override bool SupportsSurroundingText => false;
   public override string SurroundingText => "";
   public override TextSelection Selection { get; set; }

   internal void CaretHasMoved() => RaiseCursorRectangleChanged();
}
