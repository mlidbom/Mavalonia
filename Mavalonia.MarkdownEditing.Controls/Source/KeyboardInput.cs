using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaEdit;

namespace Mavalonia.MarkdownEditing.Controls.Source;

///<summary>What a person does with the keyboard in the source view of a <see cref="MarkdownEditor"/>, other than what any <see cref="TextEditor"/> does: pasting as the editor does it.</summary>
class KeyboardInput
{
   readonly MarkdownEditor _editor;
   readonly TextEditor _view;
   readonly IClipboardInput _clipboardInput;

   internal KeyboardInput(MarkdownEditor editor, TextEditor view, IClipboardInput clipboardInput)
   {
      _editor = editor;
      _view = view;
      _clipboardInput = clipboardInput;
      view.AddHandler(InputElement.KeyDownEvent, Press, RoutingStrategies.Tunnel); // Before the view pastes text itself.
   }

   void Press(object? sender, KeyEventArgs e)
   {
      if(ActionOf(e) is not {} action) return;

      e.Handled = true;
      action().SurfaceExceptions();
   }

   Func<Task>? ActionOf(KeyEventArgs e)
   {
      if(_editor.PasteAsQuoteGesture?.Matches(e) == true) return _clipboardInput.PasteAsQuote;
      if(_editor.PasteTextGesture?.Matches(e) == true) return _clipboardInput.PasteText;
      if(_view.GetPlatformSettings()!.HotkeyConfiguration.Paste.Any(it => it.Matches(e))) return _clipboardInput.Paste;
      return null;
   }
}
