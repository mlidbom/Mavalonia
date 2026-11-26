using Mavalonia.Markdown.Editing.Changes;
using Mavalonia.Markdown.Editing.Writing;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Editing.Edits.NewBlocks;

///<summary>Inserts blocks where the caret is in a paragraph or a heading. The text before and after the caret become blocks of their own.</summary>
class InsertingBlocks
{
   readonly Caret _caret;
   readonly string _markdown;

   internal InsertingBlocks(Caret caret, string markdown)
   {
      _caret = caret;
      _markdown = markdown;
   }

   RenderedMarkdown Rendered => _caret.Rendered;
   RenderedBlock Block => _caret.Block;
   LineBreaksInside LineBreaks => new(Rendered, Block.Containers);

   ///<summary>Inserts the blocks, and puts the caret at the end of them.</summary>
   internal SourceChange WithTheCaretAtTheirEnd() => Insert(upToTheCaret: LinesOfTheBlocks, SeparatorFromTextAfter);

   ///<summary>Inserts the blocks, and puts the caret after them: in what was the text after the caret, or else in an empty paragraph.</summary>
   internal SourceChange WithTheCaretAfterThem() =>
      Insert(upToTheCaret: LinesOfTheBlocks + LineBreaks.BetweenParagraphs, _caret.IsAtTheEndOfItsBlock ? SeparatorFromNext : "");

   SourceChange Insert(string upToTheCaret, string afterTheCaret) => _caret switch
   {
      { Block: Heading { Text.IsEmpty: true } heading } => Replace(heading.Marks, upToTheCaret, afterTheCaret),
      { Block.Text.IsEmpty: true } => Replace(SourceRange.EmptyAt(_caret.Offset), upToTheCaret, afterTheCaret),
      { Block: Heading heading, IsAtTheStartOfItsBlock: true } => InsertBefore(heading, upToTheCaret, afterTheCaret),
      { Block: Heading heading, IsAtTheEndOfItsBlock: true } => Replace(SourceRange.EmptyAt(heading.Source.End), LineBreaks.BetweenParagraphs + upToTheCaret, afterTheCaret),
      _ => InsertIntoTheText(upToTheCaret, afterTheCaret)
   };

   SourceChange Replace(SourceRange range, string upToTheCaret, string afterTheCaret) => SourceChange.Replace(Rendered, range, upToTheCaret + afterTheCaret).WithTheCaretAt(range.Start + upToTheCaret.Length);

   ///<summary>The caret can't be before the marks of a heading, only before its text.</summary>
   SourceChange InsertBefore(Heading heading, string upToTheCaret, string afterTheCaret)
   {
      var insertion = Replace(SourceRange.EmptyAt(heading.Source.Start), upToTheCaret, afterTheCaret);
      return afterTheCaret.Length == 0 ? insertion.WithTheCaretAt(insertion.Caret + heading.Marks.Length) : insertion;
   }

   SourceChange InsertIntoTheText(string upToTheCaret, string afterTheCaret)
   {
      var split = new TextSplit(_caret);
      var separatorFromTextBefore = _caret.IsAtTheStartOfItsBlock ? "" : LineBreaks.BetweenParagraphs;
      var changes = split.With(separatorFromTextBefore + upToTheCaret + afterTheCaret, caret: separatorFromTextBefore.Length + upToTheCaret.Length).ToList();

      return changes.FirstOrDefault(it => it.After.Text == TextAfterwards(split)) ?? changes[0];
   }

   ///<summary>All the text that is to be shown afterwards. Markup that is split without being written anew is shown as text.</summary>
   string TextAfterwards(TextSplit split)
   {
      var number = Rendered.NumberOf(Block);
      return RenderedMarkdown.TextOf(Rendered.Blocks.Take(number)) + split.First.Value + RenderedMarkdown.Of(_markdown).Text + split.Second.Value + RenderedMarkdown.TextOf(Rendered.Blocks.Skip(number + 1));
   }

   string LinesOfTheBlocks => LineBreaks.Between(_markdown.Trim('\r', '\n').ReplaceLineEndings("\n").Split('\n'));

   string SeparatorFromTextAfter => _caret.IsAtTheEndOfItsBlock ? SeparatorFromNext : LineBreaks.BetweenParagraphs;
   string SeparatorFromNext => Rendered.IsFollowedByABlockOnTheNextLine(Block) ? LineBreaks.BlankLine : "";
}
