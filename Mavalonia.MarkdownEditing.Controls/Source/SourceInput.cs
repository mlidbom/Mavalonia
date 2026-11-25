using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Document;

namespace Mavalonia.MarkdownEditing.Controls.Source;

///<summary>What a person pastes in the source view of a <see cref="MarkdownEditor"/>, which has no notion itself of rich text, nor of markdown.</summary>
class SourceInput
{
   readonly MarkdownEditor _editor;
   readonly TextEditor _view;

   internal SourceInput(MarkdownEditor editor, TextEditor view)
   {
      _editor = editor;
      _view = view;
      view.AddHandler(InputElement.KeyDownEvent, Press, RoutingStrategies.Tunnel); // Before the view pastes text itself.
   }

   // ReSharper disable once AsyncVoidEventHandlerMethod
   async void Press(object? sender, KeyEventArgs e)
   {
      if(ActionOf(e) is not {} insert) return;

      e.Handled = true;
      insert(await ClipboardContent.ReadFrom(TopLevel.GetTopLevel(_view)!.Clipboard!));
   }

   Action<ClipboardContent>? ActionOf(KeyEventArgs e)
   {
      if(_editor.PasteAsQuoteGesture?.Matches(e) == true) return Quote;
      if(_editor.PasteTextGesture?.Matches(e) == true) return it => Insert(it.Text);
      if(_view.GetPlatformSettings()!.HotkeyConfiguration.Paste.Any(it => it.Matches(e))) return it => Insert(it.Markdown);
      return null;
   }

   void Quote(ClipboardContent content)
   {
      if(content.Markdown.Length > 0) _editor.Quote(content.Markdown);
   }

   void Insert(string markdown)
   {
      if(markdown.Length == 0) return;

      _view.TextArea.Selection.ReplaceSelectionWithText(TextUtilities.NormalizeNewLines(markdown, TextUtilities.GetNewLineFromDocument(_view.Document, _view.TextArea.Caret.Line)));
      _view.TextArea.Caret.BringCaretToView();
   }
}
