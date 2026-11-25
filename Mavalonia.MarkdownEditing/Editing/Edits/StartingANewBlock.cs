using Mavalonia.MarkdownEditing.Editing.Looks;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

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
      ThematicBreak or SourceBlock => new EmptyParagraphAfter(_rendered, Block).Change(),
      Heading { Text.IsEmpty: true } heading => RemoveMarksOf(heading),
      Heading when _position.IsAtTheStartOfItsBlock => InsertAnEmptyParagraphBefore(),
      { Text.IsEmpty: true, Containers.Innermost: not null } => new LeavingAContainer(_rendered, _position).Change(),
      _ => Split()
   };

   SourceChange GoToTheCellBelow(TableCell cell) =>
      cell.Table.CellBelow(cell) is {} below ? SourceChange.OnlyMoveTheCaretTo(_rendered, RenderedPosition.AtTheEndOf(below)) : new EmptyParagraphAfter(_rendered, cell.Table.Cells[^1]).Change();

   SourceChange RemoveMarksOf(Heading heading) => SourceChange.Remove(_rendered, new SourceRange(heading.Source.Start, heading.Text.Source.Start));

   SourceChange InsertAnEmptyParagraphBefore() =>
      SourceChange.Insert(_rendered, Block.Source.Start, new Separator(_rendered, Block.Containers).OfParagraphs).WithTheCaretIn(_rendered.NumberOf(Block) + 1, 0);

   SourceChange Split()
   {
      var all = FormattedText.Of(_rendered, Block);
      var endOfFirst = Block.Text.SkipLineBreaksBefore(_position.Index);
      var startOfSecond = Block.Text.SkipWhiteSpaceAfter(_position.Index);
      var (first, second) = (all[..endOfFirst], all[startOfSecond..]);

      var expectedLooks = ExpectedLooks.OfReplacing(_rendered, Block, Block, BlockLooks.Of(_rendered, Block).WithText(first), BlockLooks.OfAParagraph(ContainersOfTheSecond(), second));
      var changes = Changes().ToList();

      return changes.FirstOrDefault(expectedLooks.AreThoseAfter) ?? changes[0];

      IEnumerable<SourceChange> Changes() =>
         from separator in new[] { Separator(), Separator() + new Separator(_rendered, Block.Containers).BlankLine }
         from change in new[] { Plain(separator) }.Concat(Rewritten(separator))
         select WithTheCaretInTheSecond(change);

      SourceChange Plain(string separator) => SourceChange.Replace(_rendered, new SourceRange(Block.Text.EndOfSourceUpTo(endOfFirst), Block.Text.StartOfSourceFrom(startOfSecond)), separator);

      IEnumerable<SourceChange> Rewritten(string separator)
      {
         var start = Block.Text.StartOfMarkupThatTouches(endOfFirst);
         var end = Block.Text.EndOfMarkupThatTouches(startOfSecond);
         var range = new SourceRange(Block.Text.StartOfSourceFrom(start), Block.Text.EndOfSourceUpTo(end));

         return InlineMarkdownWriter.WaysToWrite(all[start..endOfFirst]).Zip(InlineMarkdownWriter.WaysToWrite(all[startOfSecond..end]))
                                    .Select(it => SourceChange.Replace(_rendered, range, it.First + separator + it.Second));
      }
   }

   SourceChange WithTheCaretInTheSecond(SourceChange change) =>
      _rendered.NumberOf(Block) + 1 is var number && number < change.After.Blocks.Count ? change.WithTheCaretIn(number, 0) : change;

   ListItem? ItemToFollow => Block.Containers.Innermost as ListItem is {} item && _rendered.IsTheFirstIn(item, Block) ? item : null;

   BlockContainers ContainersOfTheSecond() => ItemToFollow is {} item ? Block.Containers.WithoutTheInnermost.Inside(item.NewItemAfter()) : Block.Containers;

   string Separator() => ItemToFollow is null ? new Separator(_rendered, Block.Containers).OfParagraphs : _rendered.Source.LineBreak + ContainersOfTheSecond().PrefixOfTheFirstLineOfTheInnermost;
}
