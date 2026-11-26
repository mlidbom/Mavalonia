using Avalonia.Input.Platform;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using Mavalonia.Markdown.Controls.Clipboards;

namespace Mavalonia.Markdown.Controls.Source;

///<summary>What a person does with the clipboard in the source view of a <see cref="MarkdownEditor"/>.</summary>
class ClipboardInput : IClipboardInput
{
   readonly MarkdownEditor _editor;
   readonly TextEditor _view;

   internal ClipboardInput(MarkdownEditor editor, TextEditor view)
   {
      _editor = editor;
      _view = view;
   }

   IClipboard Clipboard => _view.Clipboard;

   public Task Copy() => _view.TextSelection.IsEmpty ? Task.CompletedTask : Clipboard.SetTextAsync(_view.SelectedText);

   public async Task Cut()
   {
      if(_view.TextSelection.IsEmpty) return;

      var copying = Copy();
      ReplaceTheSelectionWith("");
      await copying;
   }

   public async Task Paste() => Insert((await ClipboardContent.ReadFrom(Clipboard)).Markdown);

   public async Task PasteText() => Insert((await ClipboardContent.ReadFrom(Clipboard)).Text);

   public async Task PasteAsQuote()
   {
      var content = await ClipboardContent.ReadFrom(Clipboard);
      if(content.Markdown.Length > 0) _editor.Quote(content.Markdown);
   }

   void Insert(string markdown)
   {
      if(markdown.Length > 0) ReplaceTheSelectionWith(TextUtilities.NormalizeNewLines(markdown, TextUtilities.GetNewLineFromDocument(_view.Document, _view.TextArea.Caret.Line)));
   }

   void ReplaceTheSelectionWith(string markdown)
   {
      _view.TextArea.Selection.ReplaceSelectionWithText(markdown);
      _view.TextArea.Caret.BringCaretToView();
   }
}
