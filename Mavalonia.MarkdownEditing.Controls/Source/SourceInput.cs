using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using Mavalonia.MarkdownEditing.Editing;

namespace Mavalonia.MarkdownEditing.Controls.Source;

///<summary>What a person does in the source view of a <see cref="MarkdownEditor"/>, other than what any <see cref="TextEditor"/> does.</summary>
class SourceInput
{
   readonly MarkdownEditor _editor;
   readonly TextEditor _view;

   ///<summary>The button that was pressed latest. To the view, a link is clicked whichever it is.</summary>
   PointerUpdateKind _latestPressed;

   internal SourceInput(MarkdownEditor editor, TextEditor view)
   {
      _editor = editor;
      _view = view;
      Clipboard = new ClipboardInput(editor, view);
      view.AddHandler(InputElement.KeyDownEvent, Press, RoutingStrategies.Tunnel); // Before the view pastes text itself.
      view.AddHandler(InputElement.PointerPressedEvent, Press, RoutingStrategies.Tunnel);
      view.AddHandler(VisualLineLinkText.OpenUriEvent, TellOfTheClickedLink);
   }

   internal IClipboardInput Clipboard { get; }

   void Press(object? sender, KeyEventArgs e)
   {
      if(ActionOf(e) is not {} action) return;

      e.Handled = true;
      action().SurfaceExceptions();
   }

   Func<Task>? ActionOf(KeyEventArgs e)
   {
      if(_editor.PasteAsQuoteGesture?.Matches(e) == true) return Clipboard.PasteAsQuote;
      if(_editor.PasteTextGesture?.Matches(e) == true) return Clipboard.PasteText;
      if(_view.GetPlatformSettings()!.HotkeyConfiguration.Paste.Any(it => it.Matches(e))) return Clipboard.Paste;
      return null;
   }

   void Press(object? sender, PointerPressedEventArgs e)
   {
      _latestPressed = e.Properties.PointerUpdateKind;
      if(_latestPressed == PointerUpdateKind.RightButtonPressed) PutTheCaretWhereAMenuIsFor(e);
   }

   ///<summary>Were it not handled here, the window would have the link opened.</summary>
   void TellOfTheClickedLink(object? sender, OpenUriRoutedEventArgs click)
   {
      click.Handled = true;
      if(_latestPressed == PointerUpdateKind.LeftButtonPressed) _editor.RaiseEvent(new LinkClickedEventArgs(click.Uri.OriginalString));
   }

   ///<summary>A menu is for what is selected where the pointer is, or else for that place.</summary>
   void PutTheCaretWhereAMenuIsFor(PointerPressedEventArgs e)
   {
      _view.TextArea.Focus(NavigationMethod.Pointer);
      if(_view.GetPositionFromPoint(e.GetPosition(_view)) is not {} position) return;

      var offset = _view.Document.GetOffset(position.Location);
      if(!_view.TextSelection.Includes(offset)) _view.TextSelection = TextSelection.CaretAt(offset);
   }
}
