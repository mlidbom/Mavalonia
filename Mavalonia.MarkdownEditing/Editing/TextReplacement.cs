using Mavalonia.MarkdownEditing.Editing.Edits;
using Mavalonia.MarkdownEditing.Editing.Looks;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing;

///<summary>Replaces what is shown between two positions with other text. The blocks of the positions become one.</summary>
///<remarks>
/// To replace only the source between the positions is often wrong: taking the last letter out of <c>**b**</c> leaves <c>****</c>, and adding a space to <c>**b**</c> gives <c>**b **</c>, which isn't bold.<br/>
/// So it's tried first, and what comes of it is compared with how the result is to look. If it looks different, markup around the positions is written anew, until it looks right.
/// </remarks>
class TextReplacement
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedPosition _from;
   readonly RenderedPosition _to;
   readonly FormattedText _text;
   readonly FormattedText _textBefore;
   readonly FormattedText _textAfter;
   readonly ExpectedLooks _expectedLooks;

   internal TextReplacement(RenderedMarkdown rendered, RenderedPosition from, RenderedPosition to, FormattedText text)
   {
      _rendered = rendered;
      _from = from;
      _to = to;
      _text = text;
      _textBefore = FormattedText.Of(rendered, from.Block)[..from.Index];
      _textAfter = FormattedText.Of(rendered, to.Block)[to.Index..];
      _expectedLooks = ExpectedLooks.OfReplacing(rendered, from.Block, to.Block, BlockLooks.Of(from.Block, _textBefore.Followed(by: text).Followed(by: _textAfter)));
   }

   ///<summary>The least change that looks right. Null when none does.</summary>
   internal SourceChange? ThatLooksRight() => ThatLooksRight(Plain());

   ///<summary>The same, but tries to insert the text at <paramref name="offset"/> first, where there may be several offsets at the position, such as inside and outside of bold markup.</summary>
   internal SourceChange? ThatLooksRightInsertedAt(int offset) => ThatLooksRight(SourceChange.Insert(_rendered, offset, _text.MarkdownWithoutFormatting));

   SourceChange? ThatLooksRight(SourceChange plain)
   {
      if(LooksRight(plain)) return plain;

      return MarkupAtThePositionsWrittenAnew().Where(LooksRight).MinBy(it => it.Replacement.Length - it.Replaced.Length)
          ?? AllMarkupWrittenAnew().Concat(AfterABlankLine()).FirstOrDefault(LooksRight);
   }

   internal bool LooksRight(SourceChange change) => _expectedLooks.AreThoseAfter(change);

   ///<summary>Replaces only the source between the positions.</summary>
   internal SourceChange Plain() => SourceChange.Replace(_rendered, PlainRange, _text.MarkdownWithoutFormatting);

   SourceRange PlainRange => new(_from.IsAtTheEndOfItsBlock ? _from.Block.Text.Source.End : _from.Offset, _to.Offset);

   ///<summary>The markup that the positions touch, or that only white space separates from them.</summary>
   IEnumerable<SourceChange> MarkupAtThePositionsWrittenAnew() =>
   [
      ..WrittenAnew(From.StartOfMarkupThatTouches(_from.Index), To.EndOfMarkupThatTouches(_to.Index)),
      ..WrittenAnew(From.StartOfMarkupThatTouches(From.SkipWhiteSpaceBefore(_from.Index)), To.EndOfMarkupThatTouches(To.SkipWhiteSpaceAfter(_to.Index)))
   ];

   IEnumerable<SourceChange> AllMarkupWrittenAnew() => WrittenAnew(0, To.Length);

   IEnumerable<SourceChange> WrittenAnew(int startIndex, int endIndex)
   {
      var range = new SourceRange(From.StartOfSourceFrom(startIndex), To.EndOfSourceUpTo(endIndex));
      var text = _textBefore[startIndex..].Followed(by: _text).Followed(by: _textAfter[..(endIndex - _to.Index)]);

      return InlineMarkdownWriter.WaysToWrite(text)
                                 .Select(it => SourceChange.Replace(_rendered, range, it))
                                 .Where(it => NumberOfTheBlock < it.After.Blocks.Count)
                                 .Select(it => it.WithTheCaretIn(NumberOfTheBlock, _from.Index + _text.Length));
   }

   ///<summary>In markdown, an empty list item can't follow a paragraph without a blank line between them.</summary>
   IEnumerable<SourceChange> AfterABlankLine()
   {
      if(_from.Block.Containers.Innermost is not ListItem || _rendered.BlockBefore(_from.Block) is not {} previous) yield break;

      var blankLine = new Separator(_rendered, _from.Block.Containers.SharedWith(previous.Containers)).BlankLine;
      var change = SourceChange.Replace(_rendered, PlainRange with { Start = previous.Source.End }, blankLine + _rendered.Source[previous.Source.End..PlainRange.Start] + _text.MarkdownWithoutFormatting);
      if(NumberOfTheBlock < change.After.Blocks.Count) yield return change.WithTheCaretIn(NumberOfTheBlock, _from.Index + _text.Length);
   }

   RenderedText From => _from.Block.Text;
   RenderedText To => _to.Block.Text;

   int NumberOfTheBlock => _rendered.NumberOf(_from.Block);
}
