using Mavalonia.Markdown.Editing.Changes;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Editing.Edits.Deletion;

///<summary>Deletes what is selected.</summary>
class DeletingASelection
{
   readonly RenderedRange _selected;

   internal DeletingASelection(RenderedRange selected) => _selected = selected;

   ///<summary>The changes to make, one after the other: each is made to what is rendered after the one before it.</summary>
   internal IEnumerable<Func<RenderedMarkdown, SourceChange>> Changes()
   {
      if(_selected.IsInOneBlock || CanBecomeOneBlock) return [DeletionOfText(_selected)];
      if(IsAllOfOneTable) return [RemovalOf(_selected)];

      return DeletionsFromTheLastBlockToTheFirst();
   }

   bool CanBecomeOneBlock => _selected is { Start.Block: Paragraph or Heading, End.Block: Paragraph or Heading };

   bool IsAllOfOneTable => _selected.Start.Block is TableCell { Table: var table } && _selected == AllOf(table);

   IEnumerable<Func<RenderedMarkdown, SourceChange>> DeletionsFromTheLastBlockToTheFirst() =>
      _selected.PartInEachBlock.Reverse()
               .GroupBy(it => AllOfTheBlockOrTableOf(it.Start.Block))
               .SelectMany(it => GoesAwayAsAWhole(it.Key) ? [RemovalOf(it.Key)] : it.Select(DeletionOfText));

   RenderedRange AllOfTheBlockOrTableOf(RenderedBlock block) => block is TableCell cell ? AllOf(cell.Table) : RenderedRange.OfAll(_selected.Rendered, block, block);

   RenderedRange AllOf(Table table) => RenderedRange.OfAll(_selected.Rendered, table.FirstCell, table.LastCell);

   ///<summary>Whether there is selected text on both sides of a block or table, so that it goes away as a whole.</summary>
   bool GoesAwayAsAWhole(RenderedRange blockOrTable)
   {
      var (first, last) = (NumberOf(blockOrTable.Start), NumberOf(blockOrTable.End));

      var startsInTheSelection = first > NumberOf(_selected.Start);
      var endsInTheSelection = last < NumberOf(_selected.End) || (blockOrTable.End == _selected.End && blockOrTable.End.Block is TableCell or CodeBlock);
      return startsInTheSelection && endsInTheSelection;

      int NumberOf(RenderedPosition position) => _selected.Rendered.NumberOf(position.Block);
   }

   static Func<RenderedMarkdown, SourceChange> RemovalOf(RenderedRange blocks) => After(blocks, it => new RemovingBlocks(it).Change());

   static Func<RenderedMarkdown, SourceChange> DeletionOfText(RenderedRange text) => After(text, it => new DeletingText(it).Change());

   static Func<RenderedMarkdown, SourceChange> After(RenderedRange range, Func<RenderedRange, SourceChange> change)
   {
      var byNumbers = range.ByNumbers;
      return rendered => change(byNumbers.In(rendered));
   }
}
