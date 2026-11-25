using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using Mavalonia.MarkdownEditing.Editing;

namespace Mavalonia.MarkdownEditing.Controls.Source;

///<summary>What a person does with the pointer in the source view of a <see cref="MarkdownEditor"/>, other than what any <see cref="TextEditor"/> does.</summary>
class PointerInput
{
   readonly MarkdownEditor _editor;
   readonly TextEditor _view;

   ///<summary>Null when what has been pressed doesn't start a click on a link.</summary>
   Point? _pressedAt;

   internal PointerInput(MarkdownEditor editor, TextEditor view)
   {
      _editor = editor;
      _view = view;
      view.AddHandler(InputElement.PointerPressedEvent, Press, RoutingStrategies.Tunnel);
      view.AddHandler(InputElement.PointerReleasedEvent, Release, RoutingStrategies.Bubble, handledEventsToo: true);
      view.AddHandler(VisualLineLinkText.OpenUriEvent, (_, open) => open.Handled = true); // Otherwise the window would have the URL opened.
   }

   void Press(object? sender, PointerPressedEventArgs e)
   {
      var pressed = e.Properties.PointerUpdateKind;
      var selectsMore = e.ClickCount > 1 || e.KeyModifiers.HasFlag(KeyModifiers.Shift);

      _pressedAt = pressed == PointerUpdateKind.LeftButtonPressed && !selectsMore ? e.GetPosition(_view) : null;
      if(pressed == PointerUpdateKind.RightButtonPressed) PutTheCaretWhereAMenuIsFor(e);
   }

   ///<summary>A menu is for what is selected where the pointer is, or else for that place.</summary>
   void PutTheCaretWhereAMenuIsFor(PointerPressedEventArgs e)
   {
      _view.TextArea.Focus(NavigationMethod.Pointer);
      if(_view.GetPositionFromPoint(e.GetPosition(_view)) is not {} position) return;

      var offset = _view.Document.GetOffset(position.Location);
      if(!_view.TextSelection.Includes(offset)) _view.TextSelection = TextSelection.CaretAt(offset);
   }

   void Release(object? sender, PointerReleasedEventArgs e)
   {
      var isAClick = _pressedAt is {} pressedAt && e.GetPosition(_view).IsWithinAClickOf(pressedAt);

      _pressedAt = null;
      if(isAClick && TargetOfTheLinkAt(e) is {} target) _editor.RaiseEvent(new LinkClickedEventArgs(target, e.KeyModifiers));
   }

   ///<summary>A link is one that the markdown has, or else a URL.</summary>
   string? TargetOfTheLinkAt(PointerEventArgs e)
   {
      var textView = _view.TextArea.TextView;
      var point = e.GetPosition(textView) + textView.ScrollOffset;
      if(textView.GetVisualLineFromVisualTop(point.Y) is not {} line) return null;

      var column = line.GetVisualColumnFloor(point);
      if(column >= line.VisualLength) return null;

      return _editor.LinkWrittenAt(line.FirstDocumentLine.Offset + line.GetRelativeOffset(column))?.Target ?? UrlAt(column, line);
   }

   static string? UrlAt(int column, VisualLine line) =>
      line.Elements.OfType<VisualLineLinkText>().FirstOrDefault(it => it.VisualColumn <= column && column < it.VisualColumn + it.VisualLength)?.NavigateUri.OriginalString;
}
