using AvaloniaEdit;
using AvaloniaEdit.Editing;
using Mavalonia.MarkdownEditing.Editing;

namespace Mavalonia.MarkdownEditing.Controls.Source;

static class TextEditorEX
{
   extension(TextEditor @this)
   {
      ///<summary>What is selected, and the caret's end of it.</summary>
      internal TextSelection TextSelection
      {
         get
         {
            if(@this.SelectionLength == 0) return TextSelection.CaretAt(@this.CaretOffset);

            var (start, end) = (@this.SelectionStart, @this.SelectionStart + @this.SelectionLength);
            return new TextSelection(Anchor: @this.CaretOffset == start ? end : start, @this.CaretOffset);
         }
         set
         {
            @this.TextArea.Selection = Selection.Create(@this.TextArea, value.Anchor, value.Caret);
            @this.TextArea.Caret.Offset = value.Caret;
         }
      }
   }
}
