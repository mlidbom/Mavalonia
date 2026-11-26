using Mavalonia.Markdown.Editing.Changes;
using Mavalonia.Markdown.Editing.Writing;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Editing.Edits.NewBlocks;

///<summary>Goes to the empty paragraph after a block, or adds one when there is none.</summary>
class GoingToAnEmptyParagraphAfter
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedBlock _block;

   internal GoingToAnEmptyParagraphAfter(RenderedMarkdown rendered, RenderedBlock block)
   {
      _rendered = rendered;
      _block = block;
   }

   internal SourceChange Change() =>
      _rendered.BlockAfter(_block) is Paragraph { Text.IsEmpty: true } next
         ? SourceChange.OnlyMoveTheCaretTo(_rendered, RenderedPosition.AtTheStartOf(next))
         : AddAfterReplacing(SourceRange.EmptyAt(_block.Source.End), by: "");

   ///<summary>Adds an empty paragraph, and replaces the end of the block, which is in <paramref name="endOfTheBlock"/>.</summary>
   internal SourceChange AddAfterReplacing(SourceRange endOfTheBlock, string by)
   {
      var lineBreaks = new LineBreaksInside(_rendered, _block.Containers);
      var textUpToTheParagraph = by + lineBreaks.BetweenParagraphs;

      return SourceChange.Replace(_rendered, endOfTheBlock, textUpToTheParagraph + (_rendered.IsFollowedByABlockOnTheNextLine(_block) ? lineBreaks.BlankLine : ""))
                         .WithTheCaretAt(endOfTheBlock.Start + textUpToTheParagraph.Length);
   }
}
