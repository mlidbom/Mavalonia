using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits.NewBlocks;

///<summary>The text of the block that the caret is in, split in two there.</summary>
class TextSplit
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedBlock _block;
   readonly FormattedText _text;
   readonly int _endOfFirst;
   readonly int _startOfSecond;

   internal TextSplit(Caret caret)
   {
      _rendered = caret.Rendered;
      _block = caret.Block;
      _text = FormattedText.Of(_rendered, _block);
      _endOfFirst = _block.Text.SkipLineBreaksBefore(caret.Index);
      _startOfSecond = _block.Text.SkipWhiteSpaceAfter(caret.Index);
   }

   internal FormattedText First => _text[.._endOfFirst];
   internal FormattedText Second => _text[_startOfSecond..];

   ///<summary>The ways to put markdown between the two, the one that changes least first. The caret ends up <paramref name="caret"/> into the markdown.</summary>
   internal IEnumerable<SourceChange> With(string between, int caret) => [OnlyWith(between, caret), ..WithMarkupWrittenAnew(between, caret), ..WithTheRestOfTheBlockWrittenAnew(between, caret)];

   ///<summary>Changes nothing else.</summary>
   SourceChange OnlyWith(string between, int caret)
   {
      var range = new SourceRange(_block.Text.EndOfSourceUpTo(_endOfFirst), _block.Text.StartOfSourceFrom(_startOfSecond));
      return SourceChange.Replace(_rendered, range, between).WithTheCaretAt(range.Start + caret);
   }

   ///<summary>Ends the markup that is split in the first, and starts it again in the second.</summary>
   IEnumerable<SourceChange> WithMarkupWrittenAnew(string between, int caret)
   {
      var end = _block.Text.EndOfMarkupThatTouches(_startOfSecond);
      return WrittenAnew(between, caret, end, _block.Text.EndOfSourceUpTo(end));
   }

   ///<summary>A heading may have marks after its text too, which mean nothing in another block.</summary>
   IEnumerable<SourceChange> WithTheRestOfTheBlockWrittenAnew(string between, int caret) => WrittenAnew(between, caret, _text.Length, _block.Source.End);

   IEnumerable<SourceChange> WrittenAnew(string between, int caret, int end, int endOfSource)
   {
      var start = _block.Text.StartOfMarkupThatTouches(_endOfFirst);
      var range = new SourceRange(_block.Text.StartOfSourceFrom(start), endOfSource);

      return InlineMarkdownWriter.WaysToWrite(_text[start.._endOfFirst]).Zip(InlineMarkdownWriter.WaysToWrite(_text[_startOfSecond..end]))
                                 .Select(it => SourceChange.Replace(_rendered, range, it.First + between + it.Second).WithTheCaretAt(range.Start + it.First.Length + caret));
   }
}
