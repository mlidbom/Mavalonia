using Avalonia.Controls;
using Avalonia.Input.Platform;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Input;

///<summary>What a person does with the clipboard in a <see cref="RenderedEditorView"/>.</summary>
class ClipboardInput
{
   readonly RenderedEditorView _view;

   internal ClipboardInput(RenderedEditorView view) => _view = view;

   IClipboard Clipboard => TopLevel.GetTopLevel(_view)!.Clipboard!;

   ///<summary>Awaits <paramref name="use"/> of the clipboard, so that an exception in it surfaces where all of the application's do.</summary>
   // ReSharper disable once AsyncVoidMethod
   internal static async void Use(Task use) => await use;

   internal async Task Copy(RenderedEditor editor)
   {
      if(!editor.Selection.IsEmpty) await ClipboardContent.Write(editor, Clipboard);
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
      var content = await ClipboardContent.ReadFrom(Clipboard);
      Insert(editor.CaretPosition.Block is CodeBlock ? content.Text : content.Markdown, editor.Paste);
   }

   internal async Task PasteText(RenderedEditor editor) => Insert((await ClipboardContent.ReadFrom(Clipboard)).Text, editor.Paste);

   internal async Task PasteAsQuote(RenderedEditor editor) => Insert((await ClipboardContent.ReadFrom(Clipboard)).Markdown, editor.Quote);

   void Insert(string markdown, Action<string> insert)
   {
      if(markdown.Length == 0) return;

      insert(markdown);
      _view.BringTheCaretIntoView();
   }
}
