using System.Text.RegularExpressions;
using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Edits.Tables;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

///<summary>Types text where the caret is.</summary>
///<remarks>
/// What is typed may be markdown: <c>**</c> after <c>**bold</c> makes it bold. But typing anything that isn't markup changes nothing but that it's shown.
/// </remarks>
partial class Typing
{
   const string CharactersOfMarkup = @"\`*_~[]()<>#+-.!|&:=";

   readonly Caret _caret;
   readonly TextFormatting _formatting;

   ///<param name="caret">Where the text is typed.</param>
   ///<param name="formatting">The formatting that it gets.</param>
   internal Typing(Caret caret, TextFormatting formatting)
   {
      _caret = caret;
      _formatting = formatting;
   }

   internal SourceChange Of(string text) => _caret.Block switch
   {
      CodeBlock code => OfCode(code, text),
      ThematicBreak or SourceBlock => Insert(text),
      TableCell => OfTheTextOfACell(text),
      _ => OfACodeFence(text) ?? OfOrdinaryText(text)
   };

   SourceChange Insert(string text) => SourceChange.Insert(_caret.Rendered, _caret.Offset, text);

   SourceChange OfCode(CodeBlock block, string code) => new CodeReplacement(_caret.Rendered, block, SourceRange.EmptyAt(_caret.Offset), code).Change();

   SourceChange OfTheTextOfACell(string text)
   {
      var written = FormattedText.Typed(text, _formatting).Select(it => it.Value == '|' ? it.WrittenEscaped : it);
      var inserted = Insert(written.MarkdownWithoutFormatting);

      return ReplacementWith(written).ThatLooksRight(tryFirst: inserted) ?? inserted;
   }

   ///<summary>The third backtick of a code fence. The fence that ends the code comes with it: without that, all the rest of the document would be code.</summary>
   SourceChange? OfACodeFence(string text)
   {
      if(text != "`" || _caret.TextBeforeItOnItsLine != "``" || !_caret.IsAtTheEndOfItsBlock) return null;

      var newLine = new LineBreaksInside(_caret.Rendered, _caret.Block.Containers).NewLine;
      return Insert(text + newLine + newLine + CodeBlock.Fence).WithTheCaretAt(_caret.Offset + text.Length + newLine.Length);
   }

   SourceChange OfOrdinaryText(string text)
   {
      var typed = FormattedText.Typed(text, _formatting);
      var replacement = ReplacementWith(typed);
      var inserted = Insert(text);

      var isMarkup = MayBeMarkup(text) && _formatting == _caret.Formatting && !replacement.LooksRight(inserted) && !MakesATable(inserted);
      return isMarkup ? inserted : replacement.ThatLooksRight(tryFirst: inserted) ?? ReplacementWith(typed.WithPunctuationEscaped).ThatLooksRight() ?? inserted;
   }

   ///<summary>The first part of the line under a header is enough for that, so the rest of the line would end up in a cell. See <see cref="MakingATable"/>.</summary>
   bool MakesATable(SourceChange change) => CountOfCellsIn(change.After) > CountOfCellsIn(_caret.Rendered);

   static int CountOfCellsIn(RenderedMarkdown rendered) => rendered.Blocks.Count(it => it is TableCell);

   TextReplacement ReplacementWith(FormattedText text) => new(_caret.EmptyRange, text);

   bool MayBeMarkup(string text) => text.Any(it => CharactersOfMarkup.Contains(it, StringComparison.Ordinal)) || (text == " " && MarkerOfABlock().IsMatch(_caret.TextBeforeItOnItsLine));

   ///<summary>What becomes a heading or a list item when a space follows it.</summary>
   [GeneratedRegex(@"^\s*([-*+]|\d{1,9}[.)]|#{1,6})$")]
   private static partial Regex MarkerOfABlock();
}
