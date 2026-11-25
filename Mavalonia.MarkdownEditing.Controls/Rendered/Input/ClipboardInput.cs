using Avalonia.Input.Platform;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Input;

///<summary>What a person does with the clipboard in a <see cref="RenderedEditorView"/>.</summary>
class ClipboardInput : IClipboardInput
{
   readonly RenderedEditorView _view;

   internal ClipboardInput(RenderedEditorView view) => _view = view;

   IClipboard Clipboard => _view.Clipboard;
   RenderedEditor Editor => _view.Editor!;

   public Task Copy() => Editor.Selection.IsEmpty ? Task.CompletedTask : new CopiedSelection(Editor).PutOn(Clipboard);

   public async Task Cut()
   {
      if(Editor.Selection.IsEmpty) return;

      var copying = Copy();
      Editor.DeleteCharacterBefore();
      _view.BringTheCaretIntoView();
      await copying;
   }

   public async Task Paste()
   {
      var editor = Editor;
      var content = await ClipboardContent.ReadFrom(Clipboard);
      Insert(editor.CaretPosition.Block is CodeBlock ? content.Text : content.Markdown, editor.Paste);
   }

   public async Task PasteText()
   {
      var editor = Editor;
      Insert((await ClipboardContent.ReadFrom(Clipboard)).Text, editor.Paste);
   }

   public async Task PasteAsQuote()
   {
      var editor = Editor;
      Insert((await ClipboardContent.ReadFrom(Clipboard)).Markdown, editor.Quote);
   }

   void Insert(string markdown, Action<string> insert)
   {
      if(markdown.Length == 0) return;

      insert(markdown);
      _view.BringTheCaretIntoView();
   }
}
