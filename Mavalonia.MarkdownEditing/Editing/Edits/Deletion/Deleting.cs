using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Edits.Nesting;
using Mavalonia.MarkdownEditing.Editing.Looks;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing.Edits.Deletion;

///<summary>Deletes what is before or after the caret.</summary>
class Deleting
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedPosition _position;

   internal Deleting(RenderedMarkdown rendered, RenderedPosition position)
   {
      _rendered = rendered;
      _position = position;
   }

   RenderedBlock Block => _position.Block;
   TextBoundaries Boundaries => Block.Text.Boundaries;

   internal SourceChange TheCharacterBefore() => _position.IsAtTheStartOfItsBlock ? WhatIsBeforeTheBlock() : TheTextFrom(Boundaries.StartOfCharacterBefore(_position.Index));
   internal SourceChange TheWordBefore() => _position.IsAtTheStartOfItsBlock ? WhatIsBeforeTheBlock() : TheTextFrom(Boundaries.StartOfWordBefore(_position.Index));
   internal SourceChange TheCharacterAfter() => _position.IsAtTheEndOfItsBlock ? WhatIsAfterTheBlock() : TheTextUpTo(Boundaries.EndOfCharacterAfter(_position.Index));
   internal SourceChange TheWordAfter() => _position.IsAtTheEndOfItsBlock ? WhatIsAfterTheBlock() : TheTextUpTo(Boundaries.EndOfWordAfter(_position.Index));

   SourceChange TheTextFrom(int start) => TextBetween(_position with { Index = start }, _position);
   SourceChange TheTextUpTo(int end) => TextBetween(_position, _position with { Index = end });
   SourceChange TextBetween(RenderedPosition start, RenderedPosition end) => new DeletingText(new RenderedRange(_rendered, start, end)).Change();

   SourceChange WhatIsBeforeTheBlock() => Block switch
   {
      Heading heading => SourceChange.Remove(_rendered, heading.Marks),
      CodeBlock { Text.IsEmpty: true } code => SourceChange.Remove(_rendered, code.Source),
      TableCell cell => GoToTheCellBefore(cell),
      CodeBlock or SourceBlock => GoToTheEndOfTheBlockBefore(),
      _ when Block.Containers.Innermost is {} container && _rendered.IsTheFirstBlockInside(container, Block) => new LeavingAContainer(_rendered, _position).Change(),
      _ => _rendered.BlockBefore(Block) is {} previous ? Join(previous, Block) : Nothing()
   };

   SourceChange WhatIsAfterTheBlock() => _rendered.BlockAfter(Block) is {} next ? Join(Block, next) : Nothing();

   SourceChange Nothing() => SourceChange.OnlyMoveTheCaretTo(_rendered, _position);

   SourceChange GoToTheCellBefore(TableCell cell) => cell == cell.Table.FirstCell ? Nothing() : GoToTheEndOfTheBlockBefore();

   SourceChange GoToTheEndOfTheBlockBefore() => _rendered.BlockBefore(Block) is {} previous ? SourceChange.OnlyMoveTheCaretTo(_rendered, RenderedPosition.AtTheEndOf(previous)) : Nothing();

   SourceChange Join(RenderedBlock first, RenderedBlock second)
   {
      if(second is Paragraph { Text.IsEmpty: true }) return RemoveTheEmptyParagraphAfter(first, second);
      if(first is ThematicBreak) return Remove(first);
      if(first is Paragraph { Text.IsEmpty: true } && RemoveTheEmptyParagraphBefore(second, first) is {} removal) return removal;
      if(!CanHaveTextJoined(first) || !CanHaveTextJoined(second)) return GoAcrossTheBorderBetween(first, second);

      return JoinTheTextsOf(first, second);
   }

   SourceChange Remove(RenderedBlock block) => new RemovingBlocks(RenderedRange.OfAll(_rendered, block, block)).Change().WithTheCaretAt(NumbersOfTheStartOf(block));

   SourceChange RemoveTheEmptyParagraphAfter(RenderedBlock block, RenderedBlock paragraph) =>
      SourceChange.Remove(_rendered, new SourceRange(block.Source.End, paragraph.Source.End)).WithTheCaretAt(RenderedPosition.AtTheEndOf(block).Offset);

   ///<summary>Null when the paragraph can't be removed without changing how what is left looks.</summary>
   SourceChange? RemoveTheEmptyParagraphBefore(RenderedBlock block, RenderedBlock paragraph)
   {
      var removal = SourceChange.Remove(_rendered, new SourceRange(_rendered.Source.StartOfLineAt(paragraph.Source.Start), _rendered.Source.StartOfLineAt(block.Source.Start)));

      return ExpectedLooks.OfReplacing(_rendered, paragraph, paragraph).AreThoseOf(removal.After) ? removal.WithTheCaretAt(NumbersOfTheStartOf(paragraph)) : null;
   }

   PositionByNumbers NumbersOfTheStartOf(RenderedBlock block) => _rendered.NumbersOf(RenderedPosition.AtTheStartOf(block));

   static bool CanHaveTextJoined(RenderedBlock block) => block is Paragraph or Heading;

   SourceChange JoinTheTextsOf(RenderedBlock first, RenderedBlock second)
   {
      var endOfFirst = RenderedPosition.AtTheEndOf(first);
      return TextBetween(endOfFirst, RenderedPosition.AtTheStartOf(second)).WithTheCaretAt(_rendered.NumbersOf(endOfFirst));
   }

   SourceChange GoAcrossTheBorderBetween(RenderedBlock first, RenderedBlock second) =>
      SourceChange.OnlyMoveTheCaretTo(_rendered, Block == first ? RenderedPosition.AtTheStartOf(second) : RenderedPosition.AtTheEndOf(first));
}
