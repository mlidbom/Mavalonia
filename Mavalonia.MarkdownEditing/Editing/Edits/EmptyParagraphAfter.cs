using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

///<summary>Adds an empty paragraph after a block, or goes to the one that is there.</summary>
class EmptyParagraphAfter
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedBlock _block;

   internal EmptyParagraphAfter(RenderedMarkdown rendered, RenderedBlock block)
   {
      _rendered = rendered;
      _block = block;
   }

   internal SourceChange Change() => Next is Paragraph { Text.IsEmpty: true } ? SourceChange.OnlyMoveTheCaretTo(_rendered, RenderedPosition.AtTheStartOf(Next)) : Insert(textBefore: "", SourceRange.EmptyAt(_block.Source.End));

   ///<summary>Adds the paragraph after writing the end of the block, which is in <paramref name="endOfBlock"/>, as <paramref name="textBefore"/>.</summary>
   internal SourceChange Insert(string textBefore, SourceRange endOfBlock)
   {
      var separator = new Separator(_rendered, _block.Containers);
      var textUpToTheParagraph = textBefore + separator.OfParagraphs;

      return SourceChange.Replace(_rendered, endOfBlock, textUpToTheParagraph + (IsFollowedByABlockOnTheNextLine ? separator.BlankLine : ""))
                         .WithTheCaretAt(endOfBlock.Start + textUpToTheParagraph.Length);
   }

   RenderedBlock? Next => _rendered.BlockAfter(_block);

   bool IsFollowedByABlockOnTheNextLine => Next is not null && _rendered.Source.StartOfLineAfter(_block.Source.End) == _rendered.Source.StartOfLineAt(Next.Source.Start);
}
