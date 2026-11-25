using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

///<summary>Pastes markdown where the caret is.</summary>
class Pasting
{
   readonly Caret _caret;

   internal Pasting(Caret caret) => _caret = caret;

   RenderedMarkdown Rendered => _caret.Rendered;
   RenderedBlock Block => _caret.Block;
   LineBreaksInside LineBreaks => new(Rendered, Block.Containers);

   internal SourceChange Of(string markdown) => Block switch
   {
      CodeBlock => new Typing(_caret, _caret.Formatting).Of(InTheLinesOfTheBlock(markdown)),
      TableCell => new Typing(_caret, _caret.Formatting).Of(OnOneLine(markdown)),
      _ when IsTextOfOneParagraph(markdown) => SourceChange.Insert(Rendered, _caret.Offset, InTheLinesOfTheBlock(markdown)),
      { Text.IsEmpty: true } and Paragraph => SourceChange.Insert(Rendered, _caret.Offset, InTheLinesOfTheBlock(markdown) + SeparatorFromNext),
      _ => BetweenTheTextBeforeAndAfter(markdown)
   };

   static bool IsTextOfOneParagraph(string markdown) => RenderedMarkdown.Of(markdown).Blocks is [Paragraph { Containers.Count: 0 }];

   static string OnOneLine(string markdown) => string.Join(' ', markdown.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries));

   ///<summary>With the prefix that each line must have to be in the block.</summary>
   string InTheLinesOfTheBlock(string markdown) =>
      string.Concat(markdown.ReplaceLineEndings("\n").Split('\n').Select((line, number) => number == 0 ? line : line.Length == 0 ? LineBreaks.BlankLine : LineBreaks.NewLine + line));

   ///<summary>The markdown has blocks of its own, so the text before and after the caret become blocks of theirs.</summary>
   SourceChange BetweenTheTextBeforeAndAfter(string markdown)
   {
      var end = Block.Text.EndOfSourceUpTo(_caret.Index);
      var textBefore = _caret.IsAtTheStartOfItsBlock ? "" : LineBreaks.BetweenParagraphs;
      var textAfter = _caret.IsAtTheEndOfItsBlock ? SeparatorFromNext : LineBreaks.BetweenParagraphs;
      var pasted = textBefore + InTheLinesOfTheBlock(markdown.Trim('\r', '\n'));

      return SourceChange.Replace(Rendered, new SourceRange(end, Block.Text.StartOfSourceFrom(_caret.Index)), pasted + textAfter).WithTheCaretAt(end + pasted.Length);
   }

   string SeparatorFromNext => Rendered.IsFollowedByABlockOnTheNextLine(Block) ? LineBreaks.BlankLine : "";
}
