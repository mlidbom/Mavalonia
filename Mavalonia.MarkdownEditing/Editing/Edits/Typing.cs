using System.Text.RegularExpressions;
using Mavalonia.MarkdownEditing.Editing.Changes;
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

   readonly RenderedMarkdown _rendered;
   readonly int _offset;
   readonly RenderedPosition _position;
   readonly TextFormatting _formatting;

   ///<param name="rendered">What is typed in.</param>
   ///<param name="offset">Where the caret is.</param>
   ///<param name="formatting">The formatting that the text gets.</param>
   internal Typing(RenderedMarkdown rendered, int offset, TextFormatting formatting)
   {
      _rendered = rendered;
      _offset = offset;
      _position = rendered.PositionAt(offset);
      _formatting = formatting;
   }

   internal SourceChange Of(string text) => _position.Block switch
   {
      CodeBlock code => OfCode(code, text),
      ThematicBreak or SourceBlock => Insert(text),
      TableCell => OfTheTextOfACell(text),
      _ => OfACodeFence(text) ?? OfOrdinaryText(text)
   };

   SourceChange Insert(string text) => SourceChange.Insert(_rendered, _offset, text);

   SourceChange OfCode(CodeBlock block, string code) => new CodeReplacement(_rendered, block, SourceRange.EmptyAt(_offset), code).Change();

   SourceChange OfTheTextOfACell(string text)
   {
      var written = FormattedText.Typed(text, _formatting).Select(it => it.Value == '|' ? it with { Markdown = @"\|" } : it);
      var inserted = Insert(written.MarkdownWithoutFormatting);

      return ReplacementWith(written).ThatLooksRight(tryFirst: inserted) ?? inserted;
   }

   ///<summary>The third backtick of a code fence. The fence that ends the code comes with it: without that, all the rest of the document would be code.</summary>
   SourceChange? OfACodeFence(string text)
   {
      if(text != "`" || TextOfTheLineBeforeTheCaret != "``" || !_position.IsAtTheEndOfItsBlock) return null;

      var newLine = new LineBreaksInside(_rendered, _position.Block.Containers).NewLine;
      return Insert("`" + newLine + newLine + "```").WithTheCaretAt(_offset + 1 + newLine.Length);
   }

   SourceChange OfOrdinaryText(string text)
   {
      var replacement = ReplacementWith(FormattedText.Typed(text, _formatting));
      var inserted = Insert(text);

      var isMarkup = MayBeMarkup(text) && IsFormattedAsWhereTheCaretIs && !replacement.LooksRight(inserted);
      return isMarkup ? inserted : replacement.ThatLooksRight(tryFirst: inserted) ?? inserted;
   }

   TextReplacement ReplacementWith(FormattedText text) => new(RenderedRange.EmptyAt(_rendered, _position), text);

   bool IsFormattedAsWhereTheCaretIs => _formatting == _position.Block.Text.FormattingAt(_offset);

   bool MayBeMarkup(string text) => text.Any(it => CharactersOfMarkup.Contains(it, StringComparison.Ordinal)) || (text == " " && MarkerOfABlock().IsMatch(TextOfTheLineBeforeTheCaret));

   string TextOfTheLineBeforeTheCaret => _position.Block.Text.Value[.._position.Index].Split('\n')[^1];

   ///<summary>What becomes a heading or a list item when a space follows it.</summary>
   [GeneratedRegex(@"^\s*([-*+]|\d{1,9}[.)]|#{1,6})$")]
   private static partial Regex MarkerOfABlock();
}
