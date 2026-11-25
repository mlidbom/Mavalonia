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

   internal static FormattedText Of(RenderedMarkdown rendered, RenderedBlock block) => new([..block.Text.Chars.Select(it => FormattedChar.In(rendered, it))]);

   ///<summary>The text in <paramref name="range"/>, which must be in one block.</summary>
   internal static FormattedText In(RenderedRange range) => Of(range.Rendered, range.Start.Block)[range.Start.Index..range.End.Index];

   internal IReadOnlyList<FormattedChar> Chars { get; }

   ///<summary>The chars as a string.</summary>
   internal string Value => new([..Chars.Select(it => it.Value)]);

   internal int Length => Chars.Count;

   internal FormattedText this[Range range] => new([..Chars.Take(range)]);

   internal FormattedText Followed(FormattedText by) => new([..Chars, ..by.Chars]);

   internal FormattedText Select(Func<FormattedChar, FormattedChar> change) => new([..Chars.Select(change)]);

   ///<summary>The text without formatting, written as markdown.</summary>
   internal string MarkdownWithoutFormatting => string.Concat(Chars.Select(it => it.Markdown));

   ///<summary>Whether the text looks like <paramref name="other"/>. White space looks the same in bold, so only the formatting of the other chars counts.</summary>
   internal bool LooksLike(RenderedText other) => Length == other.Length && Chars.Zip(other.Chars).All(it => LookAlike(it.First, it.Second));

   static bool LookAlike(FormattedChar one, RenderedChar other) => one.Value == other.Value && (one.IsWhiteSpace || one.Formatting.LooksLike(other.Formatting));
}
