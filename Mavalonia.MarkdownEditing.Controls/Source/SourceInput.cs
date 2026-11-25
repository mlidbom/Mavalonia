using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaEdit;

namespace Mavalonia.MarkdownEditing.Controls.Source;

///<summary>What a person does in the source view of a <see cref="MarkdownEditor"/>, other than what any <see cref="TextEditor"/> does.</summary>
class SourceInput
{
   readonly MarkdownEditor _editor;
   readonly TextEditor _view;

   internal SourceInput(MarkdownEditor editor, TextEditor view)
   {
      _editor = editor;
      _view = view;
      _ = new PointerInput(editor, view);
      ClipboardInput = new ClipboardInput(editor, view);
      view.AddHandler(InputElement.KeyDownEvent, Press, RoutingStrategies.Tunnel); // Before the view pastes text itself.
   }

   internal IClipboardInput ClipboardInput { get; }

   void Press(object? sender, KeyEventArgs e)
   {
      if(ActionOf(e) is not {} action) return;

      e.Handled = true;
      action().SurfaceExceptions();
   }

   Func<Task>? ActionOf(KeyEventArgs e)
   {
      if(_editor.PasteAsQuoteGesture?.Matches(e) == true) return ClipboardInput.PasteAsQuote;
      if(_editor.PasteTextGesture?.Matches(e) == true) return ClipboardInput.PasteText;
      if(_view.GetPlatformSettings()!.HotkeyConfiguration.Paste.Any(it => it.Matches(e))) return ClipboardInput.Paste;
      return null;
   }
}
