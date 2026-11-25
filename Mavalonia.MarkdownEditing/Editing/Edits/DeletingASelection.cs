using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

///<summary>Deletes what is between two positions.</summary>
class DeletingASelection
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedPosition _from;
   readonly RenderedPosition _to;

   internal DeletingASelection(RenderedMarkdown rendered, RenderedPosition from, RenderedPosition to)
   {
      _rendered = rendered;
      _from = from;
      _to = to;
   }

   ///<summary>The changes to make, one after the other: each is made to what is rendered after the one before it.</summary>
   internal IEnumerable<Func<RenderedMarkdown, SourceChange>> Changes()
   {
      if(_from.Block == _to.Block || CanBecomeOneBlock) return [Text(_from, _to)];
      if(AreAllOfOneTable) return [_ => new RemovingBlocks(_rendered).Remove(_from.Block, _to.Block).WithTheCaretAt(StartOfWhatIsRemoved)];

      return FromTheLastBlockToTheFirst();
   }

   bool CanBecomeOneBlock => _from.Block is Paragraph or Heading && _to.Block is Paragraph or Heading;

   bool AreAllOfOneTable => _from is { Block: TableCell first, IsAtTheStartOfItsBlock: true } && _to is { Block: TableCell last, IsAtTheEndOfItsBlock: true } && first.Table == last.Table && first == first.Table.Cells[0] && last == last.Table.Cells[^1];

   int StartOfWhatIsRemoved => _rendered.BlockBefore(_from.Block) is {} previous ? RenderedPosition.AtTheEndOf(previous).Offset : 0;

   IEnumerable<Func<RenderedMarkdown, SourceChange>> FromTheLastBlockToTheFirst()
   {
      var (first, last) = (_rendered.NumberOf(_from.Block), _rendered.NumberOf(_to.Block));

      for(var number = last; number >= first; number--)
      {
         var block = _rendered.Blocks[number];
         if(IsAllSelected(block))
         {
            var firstToRemove = block is TableCell cell ? cell.Table.Cells[0] : block;
            yield return Removal(number: _rendered.NumberOf(firstToRemove), numberOfTheLast: number);
            number = _rendered.NumberOf(firstToRemove);
         }
         else
            yield return Text(number, number == first ? _from.Index : 0, number == last ? _to.Index : block.Text.Length);
      }
   }

   ///<summary>Whether there is selected text on both sides of the block, or of the table that it is a cell of, so that it goes away as a whole.</summary>
   bool IsAllSelected(RenderedBlock block)
   {
      var (firstOfIt, lastOfIt) = block is TableCell cell ? (cell.Table.Cells[0], cell.Table.Cells[^1]) : (block, block);
      var (numberOfFirst, numberOfLast) = (_rendered.NumberOf(firstOfIt), _rendered.NumberOf(lastOfIt));
      var (from, to) = (_rendered.NumberOf(_from.Block), _rendered.NumberOf(_to.Block));

      var startsInTheSelection = numberOfFirst > from;
      var endsInTheSelection = numberOfLast < to || (numberOfLast == to && _to.IsAtTheEndOfItsBlock && block is TableCell or CodeBlock);
      return startsInTheSelection && endsInTheSelection;
   }

   static Func<RenderedMarkdown, SourceChange> Removal(int number, int numberOfTheLast) => rendered =>
      new RemovingBlocks(rendered).Remove(rendered.PositionNearestTo(number, 0).Block, rendered.PositionNearestTo(numberOfTheLast, 0).Block).WithTheCaretAtTheEndOf(number - 1);

   static Func<RenderedMarkdown, SourceChange> Text(int number, int from, int to) => rendered => TextIn(rendered, rendered.PositionNearestTo(number, from), rendered.PositionNearestTo(number, to));

   static Func<RenderedMarkdown, SourceChange> Text(RenderedPosition from, RenderedPosition to) => rendered => TextIn(rendered, from, to);

   static SourceChange TextIn(RenderedMarkdown rendered, RenderedPosition from, RenderedPosition to)
   {
      if(from.Block is CodeBlock code && from.Block == to.Block) return new DeletingCode(rendered, code).Between(from.Index, to.Index);

      var replacement = new TextReplacement(rendered, from, to, FormattedText.Empty);
      return replacement.ThatLooksRight() ?? replacement.Plain();
   }
}
