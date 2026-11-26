using Avalonia.Input.Platform;
using Mavalonia.Markdown.Controls.Clipboards;
using Mavalonia.Markdown.Editing;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Controls.Rendered.Input;

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

   ///<summary>Inserts <paramref name="markdown"/> with <paramref name="insert"/>, which returns the stretch of the markdown afterwards that it wrote, and tells of it.</summary>
   void Insert(string markdown, Func<string, SourceRange> insert)
   {
      if(markdown.Length == 0) return;

      var written = insert(markdown);
      _view.BringTheCaretIntoView();
      _view.RaiseEvent(new PastedEventArgs(written));
   }
}
