using Avalonia;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using Mavalonia.MarkdownEditing.Editing;

namespace Mavalonia.MarkdownEditing.Controls.Source;

static class TextEditorEX
{
   extension(TextEditor @this)
   {
      ///<summary>Just below the caret, from the corner of the <see cref="TextArea.TextView"/>.</summary>
      internal Point PlaceBelowTheCaret => @this.TextArea.Caret.CalculateCaretRectangle().BottomLeft - @this.TextArea.TextView.ScrollOffset;

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
