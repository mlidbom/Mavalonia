using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing.Writing;

///<summary>Text to write as markdown, inside a block.</summary>
class FormattedText
{
   internal static readonly FormattedText Empty = new([]);

   internal FormattedText(IReadOnlyList<FormattedChar> chars) => Chars = chars;

   internal static FormattedText Typed(string text, TextFormatting formatting) => new([..text.Select(it => FormattedChar.Typed(it, formatting))]);

   ///<summary>Several chars may stand for the same markdown, such as those of an image's description. It is written once.</summary>
   internal static FormattedText Of(RenderedMarkdown rendered, RenderedBlock block)
   {
      if(block is AlertTitle) return Typed(block.Text.Value, TextFormatting.None); // A part of its markdown, such as NO of [!NOTE], isn't what is shown.

      var chars = block.Text.Chars;
      return new FormattedText([..chars.Select((it, index) => index > 0 && chars[index - 1].Source == it.Source ? FormattedChar.In(rendered, it) with { Markdown = "" } : FormattedChar.In(rendered, it))]);
   }

   ///<summary>The text in <paramref name="range"/>, which must be in one block.</summary>
   internal static FormattedText In(RenderedRange range) => Of(range.Rendered, range.Start.Block)[range.Start.Index..range.End.Index];

   internal IReadOnlyList<FormattedChar> Chars { get; }

   ///<summary>The chars as a string.</summary>
   internal string Value => new([..Chars.Select(it => it.Value)]);

   internal int Length => Chars.Count;

   internal FormattedText this[Range range] => new([..Chars.Take(range)]);

   internal FormattedText Followed(FormattedText by) => new([..Chars, ..by.Chars]);

   internal FormattedText Select(Func<FormattedChar, FormattedChar> change) => new([..Chars.Select(change)]);

   internal FormattedText WithoutFormatting => Typed(Value, TextFormatting.None);
   internal FormattedText WithoutBold => Select(it => it.Without(TextFormats.Bold));

   ///<summary>The text, with what could be taken for markup inside a block escaped.</summary>
   internal FormattedText WithMarkupEscaped => Select(it => it.WrittenEscaped);

   ///<summary>The text, with all of its punctuation escaped.</summary>
   internal FormattedText WithPunctuationEscaped => Select(it => it.WrittenEscapedIfPunctuation);

   ///<summary>The text, with what could make a line the start of another block escaped, such as the <c>-</c> of a list item or the <c>.</c> after its number.</summary>
   internal FormattedText WithTheStartOfItsLinesEscaped
   {
      get
      {
         var couldBecomeABlock = true;
         return Select(it =>
         {
            var toEscape = couldBecomeABlock && !char.IsAsciiDigit(it.Value) && !it.IsWhiteSpace;
            couldBecomeABlock = it.Value == '\n' || (couldBecomeABlock && !toEscape);
            return toEscape ? it.WrittenEscapedIfPunctuation : it;
         });
      }
   }

   ///<summary>The text as it would be had a person just typed it: without the escapes it was written with.</summary>
   internal FormattedText AsIfTyped => Select(it => FormattedChar.Typed(it.Value, it.Formatting));

   ///<summary>The text without the white space at its start and its end.</summary>
   internal FormattedText Trimmed => new([..Chars.SkipWhile(it => it.IsWhiteSpace).Reverse().SkipWhile(it => it.IsWhiteSpace).Reverse()]);

   ///<summary>What is before, between and after the chars that <paramref name="isASeparator"/> says are separators.</summary>
   internal IReadOnlyList<FormattedText> SplitAt(Func<FormattedChar, bool> isASeparator)
   {
      var parts = new List<FormattedText>();
      var start = 0;
      foreach(var (index, formattedChar) in Chars.Index())
         if(isASeparator(formattedChar))
         {
            parts.Add(this[start..index]);
            start = index + 1;
         }

      return [..parts, this[start..]];
   }

   ///<summary>The text without formatting, written as markdown.</summary>
   internal string MarkdownWithoutFormatting => string.Concat(Chars.Select(it => it.Markdown));

   ///<summary>Whether the text looks like <paramref name="other"/>. White space looks the same in bold, so only the formatting of the other chars counts.</summary>
   internal bool LooksLike(RenderedText other) => Length == other.Length && Chars.Zip(other.Chars).All(it => LookAlike(it.First, it.Second));

   static bool LookAlike(FormattedChar one, RenderedChar other) => one.Value == other.Value && (one.IsWhiteSpace || one.Formatting.LooksLike(other.Formatting));
}
