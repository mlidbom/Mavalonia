using Mavalonia.Markdown.Editing.Changes;
using Mavalonia.Markdown.Editing.Edits.NewBlocks;
using Mavalonia.Markdown.Editing.Writing;
using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Editing.Edits;

///<summary>Breaks the line where the caret is, inside the block: what Shift and the Enter key usually do.</summary>
class BreakingTheLine
{
   readonly Caret _caret;

   internal BreakingTheLine(Caret caret) => _caret = caret;

   internal SourceChange Change() => _caret.Block switch
   {
      TableCell => SourceChange.OnlyMoveTheCaretTo(_caret.Rendered, _caret.Position), // A cell has a single line.
      CodeBlock or Paragraph { Text.IsEmpty: false } => Break(),
      _ => new StartingANewBlock(_caret).Change() // Where a line break would not be shown.
   };

   SourceChange Break()
   {
      var newLine = new LineBreaksInside(_caret.Rendered, _caret.Block.Containers).NewLine;
      var lineBreak = new FormattedText([new FormattedChar('\n', _caret.Formatting, newLine)]);
      var inserted = SourceChange.Insert(_caret.Rendered, _caret.Offset, newLine);

      return new TextReplacement(_caret.EmptyRange, lineBreak).ThatLooksRight(tryFirst: inserted) ?? inserted;
   }
}
