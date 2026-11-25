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
   readonly TextSplit _split;

   internal SplittingABlock(Caret caret)
   {
      _rendered = caret.Rendered;
      _block = caret.Block;
      _split = new TextSplit(caret);
   }

   ///<summary>The least change that looks right, or, when none does, the least there is.</summary>
   internal SourceChange Change()
   {
      var expectedLooks = ExpectedLooks.OfReplacing(_rendered, _block, _block, BlockLooks.Of(_block, _split.First), BlockLooks.OfAParagraph(ContainersOfTheSecond, _split.Second));

      return WithTheCaretInTheSecond(Changes().FirstOrDefault(it => expectedLooks.AreThoseOf(it.After)) ?? Changes().First());
   }

   IEnumerable<SourceChange> Changes() => new[] { Separator, Separator + LineBreaks.BlankLine }.SelectMany(it => _split.With(it, caret: it.Length));

   LineBreaksInside LineBreaks => new(_rendered, _block.Containers);

   string Separator => ItemToFollow is null ? LineBreaks.BetweenParagraphs : _rendered.Source.LineBreak + ContainersOfTheSecond.PrefixOfTheFirstLineOfTheInnermost;

   BlockContainers ContainersOfTheSecond => ItemToFollow is {} item ? _block.Containers.WithoutTheInnermost.Inside(item.NewItemAfter()) : _block.Containers;

   ListItem? ItemToFollow => _block.Containers.Innermost as ListItem is {} item && _rendered.IsTheFirstBlockInside(item, _block) ? item : null;

   SourceChange WithTheCaretInTheSecond(SourceChange change) =>
      _rendered.NumberOf(_block) + 1 is var numberOfTheSecond && numberOfTheSecond < change.After.Blocks.Count ? change.WithTheCaretAt(PositionByNumbers.AtTheStartOf(numberOfTheSecond)) : change;
}
