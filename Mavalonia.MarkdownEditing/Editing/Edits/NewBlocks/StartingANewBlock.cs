using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Edits.Nesting;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits.NewBlocks;

///<summary>Starts a new block where the caret is: what the Enter key usually does.</summary>
class StartingANewBlock
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedPosition _position;

   internal StartingANewBlock(RenderedMarkdown rendered, RenderedPosition position)
   {
      _rendered = rendered;
      _position = position;
   }

   RenderedBlock Block => _position.Block;

   internal SourceChange Change() => Block switch
   {
      CodeBlock code => new StartingANewLineOfCode(_rendered, _position, code).Change(),
      TableCell cell => GoToTheCellBelow(cell),
      ThematicBreak or SourceBlock => new GoingToAnEmptyParagraphAfter(_rendered, Block).Change(),
      Heading { Text.IsEmpty: true } heading => SourceChange.Remove(_rendered, heading.Marks),
      Heading when _position.IsAtTheStartOfItsBlock => InsertAnEmptyParagraphBefore(),
      { Text.IsEmpty: true, Containers.Innermost: not null } => new LeavingAContainer(_rendered, _position).Change(),
      _ => new SplittingABlock(_rendered, _position).Change()
   };

   SourceChange GoToTheCellBelow(TableCell cell) =>
      cell.Table.CellBelow(cell) is {} below
         ? SourceChange.OnlyMoveTheCaretTo(_rendered, RenderedPosition.AtTheEndOf(below))
         : new GoingToAnEmptyParagraphAfter(_rendered, cell.Table.LastCell).Change();

   SourceChange InsertAnEmptyParagraphBefore() =>
      SourceChange.Insert(_rendered, Block.Source.Start, new LineBreaksInside(_rendered, Block.Containers).BetweenParagraphs)
                  .WithTheCaretAt(PositionByNumbers.AtTheStartOf(_rendered.NumberOf(Block) + 1));
}
