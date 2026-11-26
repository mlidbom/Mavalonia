using Mavalonia.Markdown.Editing.Changes;
using Mavalonia.Markdown.Editing.Edits.Nesting;
using Mavalonia.Markdown.Editing.Looks;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Editing.Edits.Deletion;

///<summary>Deletes what is before or after the caret.</summary>
class Deleting
{
   readonly Caret _caret;

   internal Deleting(Caret caret) => _caret = caret;

   RenderedMarkdown Rendered => _caret.Rendered;
   RenderedPosition Position => _caret.Position;
   RenderedBlock Block => _caret.Block;
   TextBoundaries Boundaries => Block.Text.Boundaries;

   internal SourceChange TheCharacterBefore() => Position.IsAtTheStartOfItsBlock ? WhatIsBeforeTheBlock() : TheTextFrom(Boundaries.StartOfCharacterBefore(Position.Index));
   internal SourceChange TheWordBefore() => Position.IsAtTheStartOfItsBlock ? WhatIsBeforeTheBlock() : TheTextFrom(Boundaries.StartOfWordBefore(Position.Index));
   internal SourceChange TheCharacterAfter() => Position.IsAtTheEndOfItsBlock ? WhatIsAfterTheBlock() : TheTextUpTo(Boundaries.EndOfCharacterAfter(Position.Index));
   internal SourceChange TheWordAfter() => Position.IsAtTheEndOfItsBlock ? WhatIsAfterTheBlock() : TheTextUpTo(Boundaries.EndOfWordAfter(Position.Index));

   SourceChange TheTextFrom(int start) => TextBetween(Position with { Index = start }, Position);
   SourceChange TheTextUpTo(int end) => TextBetween(Position, Position with { Index = end });
   SourceChange TextBetween(RenderedPosition start, RenderedPosition end) => new DeletingText(new RenderedRange(Rendered, start, end)).Change();

   SourceChange WhatIsBeforeTheBlock() => Block switch
   {
      Heading heading => SourceChange.Remove(Rendered, heading.Marks),
      CodeBlock { Text.IsEmpty: true } code => SourceChange.Remove(Rendered, code.Source),
      TableCell cell => GoToTheCellBefore(cell),
      CodeBlock or SourceBlock => GoToTheEndOfTheBlockBefore(),
      _ when Block.Containers.Innermost is {} container && Rendered.IsTheFirstBlockInside(container, Block) => new LeavingAContainer(_caret).Change(),
      _ => Rendered.BlockBefore(Block) is {} previous ? Join(previous, Block) : Nothing()
   };

   SourceChange WhatIsAfterTheBlock() => Rendered.BlockAfter(Block) is {} next ? Join(Block, next) : Nothing();

   SourceChange Nothing() => SourceChange.OnlyMoveTheCaretTo(Rendered, Position);

   SourceChange GoToTheCellBefore(TableCell cell) => cell == cell.Table.FirstCell ? Nothing() : GoToTheEndOfTheBlockBefore();

   SourceChange GoToTheEndOfTheBlockBefore() => Rendered.BlockBefore(Block) is {} previous ? SourceChange.OnlyMoveTheCaretTo(Rendered, RenderedPosition.AtTheEndOf(previous)) : Nothing();

   SourceChange Join(RenderedBlock first, RenderedBlock second)
   {
      if(second is Paragraph { Text.IsEmpty: true }) return RemoveTheEmptyParagraphAfter(first, second);
      if(first is ThematicBreak) return Remove(first);
      if(first is Paragraph { Text.IsEmpty: true } && RemoveTheEmptyParagraphBefore(second, first) is {} removal) return removal;
      if(!CanHaveTextJoined(first) || !CanHaveTextJoined(second)) return GoAcrossTheBorderBetween(first, second);

      return JoinTheTextsOf(first, second);
   }

   SourceChange Remove(RenderedBlock block) => new RemovingBlocks(RenderedRange.OfAll(Rendered, block, block)).Change().WithTheCaretAt(NumbersOfTheStartOf(block));

   SourceChange RemoveTheEmptyParagraphAfter(RenderedBlock block, RenderedBlock paragraph) =>
      SourceChange.Remove(Rendered, new SourceRange(block.Source.End, paragraph.Source.End)).WithTheCaretAt(RenderedPosition.AtTheEndOf(block).Offset);

   ///<summary>Null when the paragraph can't be removed without changing how what is left looks.</summary>
   SourceChange? RemoveTheEmptyParagraphBefore(RenderedBlock block, RenderedBlock paragraph)
   {
      var removal = SourceChange.Remove(Rendered, new SourceRange(Rendered.Source.StartOfLineAt(paragraph.Source.Start), Rendered.Source.StartOfLineAt(block.Source.Start)));

      return ExpectedLooks.OfReplacing(Rendered, paragraph, paragraph).AreThoseOf(removal.After) ? removal.WithTheCaretAt(NumbersOfTheStartOf(paragraph)) : null;
   }

   PositionByNumbers NumbersOfTheStartOf(RenderedBlock block) => Rendered.NumbersOf(RenderedPosition.AtTheStartOf(block));

   static bool CanHaveTextJoined(RenderedBlock block) => block is Paragraph or Heading;

   SourceChange JoinTheTextsOf(RenderedBlock first, RenderedBlock second)
   {
      var endOfFirst = RenderedPosition.AtTheEndOf(first);
      return TextBetween(endOfFirst, RenderedPosition.AtTheStartOf(second)).WithTheCaretAt(Rendered.NumbersOf(endOfFirst));
   }

   SourceChange GoAcrossTheBorderBetween(RenderedBlock first, RenderedBlock second) =>
      SourceChange.OnlyMoveTheCaretTo(Rendered, Block == first ? RenderedPosition.AtTheStartOf(second) : RenderedPosition.AtTheEndOf(first));
}
