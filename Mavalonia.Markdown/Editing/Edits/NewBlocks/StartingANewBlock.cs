using Mavalonia.Markdown.Editing.Changes;
using Mavalonia.Markdown.Editing.Edits.Nesting;
using Mavalonia.Markdown.Editing.Edits.Tables;
using Mavalonia.Markdown.Editing.Writing;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Editing.Edits.NewBlocks;

///<summary>Starts a new block where the caret is: what the Enter key usually does.</summary>
class StartingANewBlock
{
   readonly Caret _caret;

   internal StartingANewBlock(Caret caret) => _caret = caret;

   RenderedMarkdown Rendered => _caret.Rendered;
   RenderedBlock Block => _caret.Block;

   internal SourceChange Change() => Block switch
   {
      CodeBlock code => new StartingANewLineOfCode(_caret, code).Change(),
      TableCell cell => GoToTheCellBelow(cell),
      ThematicBreak or SourceBlock => new GoingToAnEmptyParagraphAfter(Rendered, Block).Change(),
      Heading { Text.IsEmpty: true } heading => SourceChange.Remove(Rendered, heading.Marks),
      Heading when _caret.IsAtTheStartOfItsBlock => InsertAnEmptyParagraphBefore(),
      { Text.IsEmpty: true, Containers.Innermost: not null } => new LeavingAContainer(_caret).Change(),
      _ => MakingATable.Of(_caret)?.Change() ?? new SplittingABlock(_caret).Change()
   };

   SourceChange GoToTheCellBelow(TableCell cell) =>
      cell.Table.CellBelow(cell) is {} below
         ? SourceChange.OnlyMoveTheCaretTo(Rendered, RenderedPosition.AtTheEndOf(below))
         : new LeavingATable(Rendered, cell.Table).Change();

   SourceChange InsertAnEmptyParagraphBefore() =>
      SourceChange.Insert(Rendered, Block.Source.Start, new LineBreaksInside(Rendered, Block.Containers).BetweenParagraphs)
                  .WithTheCaretAt(PositionByNumbers.AtTheStartOf(Rendered.NumberOf(Block) + 1));
}
