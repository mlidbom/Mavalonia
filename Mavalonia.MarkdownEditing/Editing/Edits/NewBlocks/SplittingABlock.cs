using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Looks;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Editing.Edits.NewBlocks;

///<summary>Makes two blocks of one. The second is a paragraph, or, when the block is the first in a list item, the next item of that list.</summary>
class SplittingABlock
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedBlock _block;
   readonly FormattedText _text;
   readonly int _endOfFirst;
   readonly int _startOfSecond;

   internal SplittingABlock(Caret caret)
   {
      _rendered = caret.Rendered;
      _block = caret.Block;
      _text = FormattedText.Of(_rendered, _block);
      _endOfFirst = _block.Text.SkipLineBreaksBefore(caret.Index);
      _startOfSecond = _block.Text.SkipWhiteSpaceAfter(caret.Index);
   }

   ///<summary>The least change that looks right, or, when none does, the least there is.</summary>
   internal SourceChange Change()
   {
      var expectedLooks = ExpectedLooks.OfReplacing(_rendered, _block, _block, BlockLooks.Of(_block, _text[.._endOfFirst]), BlockLooks.OfAParagraph(ContainersOfTheSecond, _text[_startOfSecond..]));

      return WithTheCaretInTheSecond(Changes().FirstOrDefault(it => expectedLooks.AreThoseOf(it.After)) ?? Changes().First());
   }

   IEnumerable<SourceChange> Changes() =>
      from separator in new[] { Separator, Separator + LineBreaks.BlankLine }
      from change in new[] { Plain(separator) }.Concat(MarkupWrittenAnew(separator))
      select change;

   LineBreaksInside LineBreaks => new(_rendered, _block.Containers);

   string Separator => ItemToFollow is null ? LineBreaks.BetweenParagraphs : _rendered.Source.LineBreak + ContainersOfTheSecond.PrefixOfTheFirstLineOfTheInnermost;

   BlockContainers ContainersOfTheSecond => ItemToFollow is {} item ? _block.Containers.WithoutTheInnermost.Inside(item.NewItemAfter()) : _block.Containers;

   ListItem? ItemToFollow => _block.Containers.Innermost as ListItem is {} item && _rendered.IsTheFirstBlockInside(item, _block) ? item : null;

   ///<summary>Puts the separator between the two, and changes nothing else.</summary>
   SourceChange Plain(string separator) => SourceChange.Replace(_rendered, new SourceRange(_block.Text.EndOfSourceUpTo(_endOfFirst), _block.Text.StartOfSourceFrom(_startOfSecond)), separator);

   ///<summary>Ends the markup that is split in the first, and starts it again in the second.</summary>
   IEnumerable<SourceChange> MarkupWrittenAnew(string separator)
   {
      var start = _block.Text.StartOfMarkupThatTouches(_endOfFirst);
      var end = _block.Text.EndOfMarkupThatTouches(_startOfSecond);
      var range = new SourceRange(_block.Text.StartOfSourceFrom(start), _block.Text.EndOfSourceUpTo(end));

      return InlineMarkdownWriter.WaysToWrite(_text[start.._endOfFirst]).Zip(InlineMarkdownWriter.WaysToWrite(_text[_startOfSecond..end]))
                                 .Select(it => SourceChange.Replace(_rendered, range, it.First + separator + it.Second));
   }

   SourceChange WithTheCaretInTheSecond(SourceChange change) =>
      _rendered.NumberOf(_block) + 1 is var numberOfTheSecond && numberOfTheSecond < change.After.Blocks.Count ? change.WithTheCaretAt(PositionByNumbers.AtTheStartOf(numberOfTheSecond)) : change;
}
