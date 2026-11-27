using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using Mavalonia.Markdown.Controls.Clipboards;
using Mavalonia.Markdown.Controls.Formatting;

namespace Mavalonia.Markdown.Controls.Source;

///<summary>What a person does with the keyboard in the source view of a <see cref="MarkdownEditor"/>, other than what any <see cref="TextEditor"/> does: formatting, and pasting as the editor does it.</summary>
class KeyboardInput
{
   readonly MarkdownEditor _editor;
   readonly TextEditor _view;
   readonly IClipboardInput _clipboardInput;
   readonly FormattingKeys _formattingKeys;

   internal KeyboardInput(MarkdownEditor editor, TextEditor view, IClipboardInput clipboardInput)
   {
      _editor = editor;
      _view = view;
      _clipboardInput = clipboardInput;
      _formattingKeys = new FormattingKeys(editor);
      view.AddHandler(InputElement.KeyDownEvent, Press, RoutingStrategies.Tunnel); // Before the view pastes text itself.
   }

   void Press(object? sender, KeyEventArgs e)
   {
      if(ActionOf(e) is not {} action) return;

      e.Handled = true;
      action();
   }

   Action? ActionOf(KeyEventArgs e) => FormattingOf(e) ?? ClipboardActionOf(e);

   ///<summary>
   /// Null where the key would change nothing, so that it goes on to the application: with nothing selected, there is no text to format,<br/>
   /// and where the caret is in code, there is no paragraph or heading to make an item of a list.
   ///</summary>
   Action? FormattingOf(KeyEventArgs e)
   {
      if(_formattingKeys.FormatToggledBy(e) is {} format) return _editor.CanToggleAFormat ? () => _editor.ToggleFormat(format) : null;
      if(_formattingKeys.ListToggledBy(e) is {} kind) return _editor.CanToggleAList ? () => _editor.ToggleList(kind) : null;
      return null;
   }

   Action? ClipboardActionOf(KeyEventArgs e)
   {
      if(_editor.PasteAsQuoteGesture?.Matches(e) == true) return () => _clipboardInput.PasteAsQuote().SurfaceExceptions();
      if(_editor.PasteTextGesture?.Matches(e) == true) return () => _clipboardInput.PasteText().SurfaceExceptions();
      if(_view.Hotkeys.Paste.Any(it => it.Matches(e))) return () => _clipboardInput.Paste().SurfaceExceptions();
      return null;
   }
}
