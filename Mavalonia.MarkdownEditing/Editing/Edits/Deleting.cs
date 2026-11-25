using Mavalonia.MarkdownEditing.Editing.Looks;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

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
   RenderedText Text => Block.Text;

   internal SourceChange TheCharacterBefore() => _position.IsAtTheStartOfItsBlock ? WhatIsBeforeTheBlock() : TextBetween(Text.Boundaries.StartOfCharacterBefore(_position.Index), _position.Index);
   internal SourceChange TheWordBefore() => _position.IsAtTheStartOfItsBlock ? WhatIsBeforeTheBlock() : TextBetween(Text.Boundaries.StartOfWordBefore(_position.Index), _position.Index);
   internal SourceChange TheCharacterAfter() => _position.IsAtTheEndOfItsBlock ? WhatIsAfterTheBlock() : TextBetween(_position.Index, Text.Boundaries.EndOfCharacterAfter(_position.Index));
   internal SourceChange TheWordAfter() => _position.IsAtTheEndOfItsBlock ? WhatIsAfterTheBlock() : TextBetween(_position.Index, Text.Boundaries.EndOfWordAfter(_position.Index));

   SourceChange TextBetween(int start, int end)
   {
      if(Block is CodeBlock code) return new DeletingCode(_rendered, code).Between(start, end);

      var replacement = new TextReplacement(_rendered, new RenderedPosition(Block, start), new RenderedPosition(Block, end), FormattedText.Empty);
      return replacement.ThatLooksRight() ?? replacement.Plain();
   }

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
      if(first is ThematicBreak) return new RemovingBlocks(_rendered).Remove(first, first).WithTheCaretIn(_rendered.NumberOf(first), 0);
      if(first is Paragraph { Text.IsEmpty: true } && RemoveTheEmptyParagraphBefore(second, first) is {} removal) return removal;
      if(!CanHaveTextJoined(first) || !CanHaveTextJoined(second)) return GoAcrossTheBorderBetween(first, second);

      var replacement = new TextReplacement(_rendered, RenderedPosition.AtTheEndOf(first), RenderedPosition.AtTheStartOf(second), FormattedText.Empty);
      return (replacement.ThatLooksRight() ?? replacement.Plain()).WithTheCaretIn(_rendered.NumberOf(first), first.Text.Length);
   }

   static bool CanHaveTextJoined(RenderedBlock block) => block is Paragraph or Heading;

   SourceChange GoAcrossTheBorderBetween(RenderedBlock first, RenderedBlock second) =>
      SourceChange.OnlyMoveTheCaretTo(_rendered, Block == first ? RenderedPosition.AtTheStartOf(second) : RenderedPosition.AtTheEndOf(first));

   SourceChange RemoveTheEmptyParagraphAfter(RenderedBlock block, RenderedBlock paragraph) =>
      SourceChange.Remove(_rendered, new SourceRange(block.Source.End, paragraph.Source.End)).WithTheCaretAt(RenderedPosition.AtTheEndOf(block).Offset);

   ///<summary>Null when the paragraph can't be removed without changing how what is left looks.</summary>
   SourceChange? RemoveTheEmptyParagraphBefore(RenderedBlock block, RenderedBlock paragraph)
   {
      var removal = SourceChange.Remove(_rendered, new SourceRange(_rendered.Source.StartOfLineAt(paragraph.Source.Start), _rendered.Source.StartOfLineAt(block.Source.Start)));

      return ExpectedLooks.OfReplacing(_rendered, paragraph, paragraph).AreThoseAfter(removal) ? removal.WithTheCaretIn(_rendered.NumberOf(paragraph), 0) : null;
   }
}
