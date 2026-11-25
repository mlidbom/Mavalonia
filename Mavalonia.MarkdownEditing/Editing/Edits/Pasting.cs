using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

///<summary>Pastes markdown where the caret is.</summary>
class Pasting
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedPosition _position;
   readonly int _offset;

   internal Pasting(RenderedMarkdown rendered, int offset)
   {
      _rendered = rendered;
      _offset = offset;
      _position = rendered.PositionAt(offset);
   }

   RenderedBlock Block => _position.Block;
   Separator Separator => new(_rendered, Block.Containers);

   internal SourceChange Of(string markdown) => Block switch
   {
      CodeBlock => new Typing(_rendered, _offset, Block.Text.FormattingAt(_offset)).Of(InTheLinesOfTheBlock(markdown)),
      TableCell => new Typing(_rendered, _offset, Block.Text.FormattingAt(_offset)).Of(OnOneLine(markdown)),
      _ when IsTextOfOneParagraph(markdown) => SourceChange.Insert(_rendered, _offset, InTheLinesOfTheBlock(markdown)),
      { Text.IsEmpty: true } and Paragraph => SourceChange.Insert(_rendered, _offset, InTheLinesOfTheBlock(markdown) + SeparatorFromNext),
      _ => BetweenTheTextBeforeAndAfter(markdown)
   };

   static bool IsTextOfOneParagraph(string markdown) => RenderedMarkdown.Of(markdown).Blocks is [Paragraph { Containers.Count: 0 }];

   static string OnOneLine(string markdown) => string.Join(' ', markdown.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries));

   ///<summary>With the prefix that each line must have to be in the block.</summary>
   string InTheLinesOfTheBlock(string markdown) =>
      string.Concat(markdown.ReplaceLineEndings("\n").Split('\n').Select((line, number) => number == 0 ? line : line.Length == 0 ? Separator.BlankLine : Separator.NewLine + line));

   ///<summary>The markdown has blocks of its own, so the text before and after the caret become blocks of theirs.</summary>
   SourceChange BetweenTheTextBeforeAndAfter(string markdown)
   {
      var end = Block.Text.EndOfSourceUpTo(_position.Index);
      var textBefore = _position.IsAtTheStartOfItsBlock ? "" : Separator.OfParagraphs;
      var textAfter = _position.IsAtTheEndOfItsBlock ? SeparatorFromNext : Separator.OfParagraphs;
      var pasted = textBefore + InTheLinesOfTheBlock(markdown.Trim('\r', '\n'));

      return SourceChange.Replace(_rendered, new SourceRange(end, Block.Text.StartOfSourceFrom(_position.Index)), pasted + textAfter).WithTheCaretAt(end + pasted.Length);
   }

   string SeparatorFromNext => _rendered.BlockAfter(Block) is {} next && _rendered.Source.StartOfLineAfter(Block.Source.End) == _rendered.Source.StartOfLineAt(next.Source.Start) ? Separator.BlankLine : "";
}
