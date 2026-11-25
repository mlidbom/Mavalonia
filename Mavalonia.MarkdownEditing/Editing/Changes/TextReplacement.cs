using Mavalonia.MarkdownEditing.Editing.Looks;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing.Changes;

///<summary>Replaces what is shown in a range with other text. The blocks that the range starts and ends in become one.</summary>
///<remarks>
/// To replace only the source of the range is often wrong: taking the last letter out of <c>**b**</c> leaves <c>****</c>, and adding a space to <c>**b**</c> gives <c>**b **</c>, which isn't bold.<br/>
/// So it's tried first, and what comes of it is compared with how the result is to look. If it looks different, markup around the range is written anew, until it looks right.
/// </remarks>
class TextReplacement
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedPosition _start;
   readonly RenderedPosition _end;
   readonly FormattedText _text;
   readonly FormattedText _textBefore;
   readonly FormattedText _textAfter;
   readonly ExpectedLooks _expectedLooks;

   internal TextReplacement(RenderedRange replaced, FormattedText text)
   {
      (_rendered, _start, _end) = replaced;
      _text = text;
      _textBefore = FormattedText.Of(_rendered, _start.Block)[.._start.Index];
      _textAfter = FormattedText.Of(_rendered, _end.Block)[_end.Index..];
      _expectedLooks = ExpectedLooks.OfReplacing(_rendered, _start.Block, _end.Block, BlockLooks.Of(_start.Block, _textBefore.Followed(by: text).Followed(by: _textAfter)));
   }

   ///<summary>The least change that looks right. Null when none does.</summary>
   internal SourceChange? ThatLooksRight() => ThatLooksRight(tryFirst: Plain());

   ///<summary>The least change that looks right: <paramref name="tryFirst"/>, if it does. Null when none does.</summary>
   internal SourceChange? ThatLooksRight(SourceChange tryFirst)
   {
      if(LooksRight(tryFirst)) return tryFirst;

      return MarkupThatTouchesTheRangeWrittenAnew().Where(LooksRight).MinBy(it => it.Replacement.Length - it.Replaced.Length)
          ?? AllMarkupWrittenAnew().Concat(AfterABlankLine()).FirstOrDefault(LooksRight);
   }

   internal bool LooksRight(SourceChange change) => _expectedLooks.AreThoseOf(change.After);

   ///<summary>Replaces only the source of the range.</summary>
   internal SourceChange Plain() => SourceChange.Replace(_rendered, SourceOfTheRange, _text.MarkdownWithoutFormatting);

   SourceRange SourceOfTheRange => new(_start.IsAtTheEndOfItsBlock && _start.Block != _end.Block ? _start.Block.Text.Source.End : _start.Offset, _end.Offset);

   ///<summary>The markup that the range touches, or that only white space separates from it.</summary>
   IEnumerable<SourceChange> MarkupThatTouchesTheRangeWrittenAnew() =>
   [
      ..MarkupWrittenAnew(FirstText.StartOfMarkupThatTouches(_start.Index), LastText.EndOfMarkupThatTouches(_end.Index)),
      ..MarkupWrittenAnew(FirstText.StartOfMarkupThatTouches(FirstText.SkipWhiteSpaceBefore(_start.Index)), LastText.EndOfMarkupThatTouches(LastText.SkipWhiteSpaceAfter(_end.Index)))
   ];

   IEnumerable<SourceChange> AllMarkupWrittenAnew() => MarkupWrittenAnew(0, LastText.Length);

   ///<summary>The markup from <paramref name="startIndex"/> of the text of the first block to <paramref name="endIndex"/> of that of the last.</summary>
   IEnumerable<SourceChange> MarkupWrittenAnew(int startIndex, int endIndex)
   {
      var range = new SourceRange(FirstText.StartOfSourceFrom(startIndex), LastText.EndOfSourceUpTo(endIndex));
      var text = _textBefore[startIndex..].Followed(by: _text).Followed(by: _textAfter[..(endIndex - _end.Index)]);

      return InlineMarkdownWriter.WaysToWrite(text).Select(it => SourceChange.Replace(_rendered, range, it).WithTheCaretAt(EndOfTheNewText));
   }

   ///<summary>In markdown, an empty list item can't follow a paragraph without a blank line between them.</summary>
   IEnumerable<SourceChange> AfterABlankLine()
   {
      if(FirstBlock.Containers.Innermost is not ListItem item || !_rendered.IsTheFirstBlockInside(item, FirstBlock) || _rendered.BlockBefore(FirstBlock) is not {} previous) return [];

      var blankLine = new LineBreaksInside(_rendered, FirstBlock.Containers.SharedWith(previous.Containers)).BlankLine;
      var range = SourceOfTheRange with { Start = previous.Source.End };
      var sourceUpToTheRange = _rendered.Source[range.Start..SourceOfTheRange.Start];

      return [SourceChange.Replace(_rendered, range, blankLine + sourceUpToTheRange + _text.MarkdownWithoutFormatting).WithTheCaretAt(EndOfTheNewText)];
   }

   RenderedBlock FirstBlock => _start.Block;
   RenderedText FirstText => _start.Block.Text;
   RenderedText LastText => _end.Block.Text;

   PositionByNumbers EndOfTheNewText => _rendered.NumbersOf(_start with { Index = _start.Index + _text.Length });
}
