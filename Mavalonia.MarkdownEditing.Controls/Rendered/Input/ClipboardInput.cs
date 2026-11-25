using Avalonia.Controls;
using Avalonia.Input.Platform;
using Mavalonia.MarkdownEditing.Editing;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Input;

///<summary>What a person does with the clipboard in a <see cref="RenderedMarkdownView"/>.</summary>
class ClipboardInput
{
   readonly RenderedMarkdownView _view;

   internal ClipboardInput(RenderedMarkdownView view) => _view = view;

   IClipboard Clipboard => TopLevel.GetTopLevel(_view)!.Clipboard!;

   internal async Task Copy(RenderedEditor editor)
   {
      if(editor.SelectedMarkdown is { Length: > 0 } markdown) await Clipboard.SetTextAsync(markdown);
   }

   internal async Task Cut(RenderedEditor editor)
   {
      if(editor.Selection.IsEmpty) return;

      var copying = Copy(editor);
      editor.DeleteCharacterBefore();
      await copying;
   }

   internal async Task Paste(RenderedEditor editor)
   {
      if(await Clipboard.TryGetTextAsync() is not { Length: > 0 } text) return;

      editor.Paste(text);
      _view.BringTheCaretIntoView();
   }
}
