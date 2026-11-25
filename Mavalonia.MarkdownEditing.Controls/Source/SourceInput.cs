using AvaloniaEdit;
using Mavalonia.MarkdownEditing.Controls.Clipboards;

namespace Mavalonia.MarkdownEditing.Controls.Source;

///<summary>What a person does in the source view of a <see cref="MarkdownEditor"/>, other than what any <see cref="TextEditor"/> does.</summary>
class SourceInput
{
   internal SourceInput(MarkdownEditor editor, TextEditor view)
   {
      ClipboardInput = new ClipboardInput(editor, view);
      _ = new PointerInput(editor, view);
      _ = new KeyboardInput(editor, view, ClipboardInput);
   }

   internal IClipboardInput ClipboardInput { get; }
}
