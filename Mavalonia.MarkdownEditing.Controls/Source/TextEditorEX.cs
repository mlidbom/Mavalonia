using AvaloniaEdit;
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
            //bug: TextEditor.Select puts the caret at the end of what it selects, so a selection whose caret is at its start comes out turned around.
            @this.Select(value.Start, value.End - value.Start);
            @this.CaretOffset = value.Caret;
            if(!value.IsEmpty) @this.Select(value.Start, value.End - value.Start);
         }
      }
   }
}
