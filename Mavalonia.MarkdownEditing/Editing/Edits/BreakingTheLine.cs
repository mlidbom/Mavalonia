using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

///<summary>Breaks the line where the caret is, inside the block: what Shift and the Enter key usually do.</summary>
class BreakingTheLine
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedPosition _position;
   readonly int _offset;

   internal BreakingTheLine(RenderedMarkdown rendered, int offset)
   {
      _rendered = rendered;
      _offset = offset;
      _position = rendered.PositionAt(offset);
   }

   internal SourceChange Change() => _position.Block switch
   {
      TableCell => SourceChange.OnlyMoveTheCaretTo(_rendered, _position), // A cell has a single line.
      CodeBlock or Paragraph { Text.IsEmpty: false } => Break(),
      _ => new StartingANewBlock(_rendered, _position).Change() // Where a line break would not be shown.
   };

   SourceChange Break()
   {
      var newLine = new Separator(_rendered, _position.Block.Containers).NewLine;
      var lineBreak = new FormattedText([new FormattedCharacter('\n', _position.Block.Text.FormattingAt(_offset), newLine)]);

      return new TextReplacement(_rendered, _position, _position, lineBreak).ThatLooksRightInsertedAt(_offset) ?? SourceChange.Insert(_rendered, _offset, newLine);
   }
}
