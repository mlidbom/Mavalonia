using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing.Writing;

///<summary>Text to write as markdown, inside a block.</summary>
class FormattedText
{
   internal static readonly FormattedText Empty = new([]);

   internal FormattedText(IReadOnlyList<FormattedCharacter> characters) => Characters = characters;

   internal static FormattedText Typed(string text, TextFormatting formatting) => new([..text.Select(it => FormattedCharacter.Typed(it, formatting))]);

   internal static FormattedText Of(RenderedMarkdown rendered, RenderedBlock block) => new([..block.Text.Characters.Select(it => FormattedCharacter.In(rendered, it))]);

   ///<summary>The text in <paramref name="range"/>, which must be in one block.</summary>
   internal static FormattedText In(RenderedRange range) => Of(range.Rendered, range.Start.Block)[range.Start.Index..range.End.Index];

   internal IReadOnlyList<FormattedCharacter> Characters { get; }

   ///<summary>The characters as a string.</summary>
   internal string Value => new([..Characters.Select(it => it.Value)]);

   internal int Length => Characters.Count;

   internal FormattedText this[Range range] => new([..Characters.Take(range)]);

   internal FormattedText Followed(FormattedText by) => new([..Characters, ..by.Characters]);

   internal FormattedText Select(Func<FormattedCharacter, FormattedCharacter> change) => new([..Characters.Select(change)]);

   ///<summary>The text without formatting, written as markdown.</summary>
   internal string MarkdownWithoutFormatting => string.Concat(Characters.Select(it => it.Markdown));

   ///<summary>Whether the text looks like <paramref name="other"/>. White space looks the same in bold, so only the formatting of the other characters counts.</summary>
   internal bool LooksLike(RenderedText other) => Length == other.Length && Characters.Zip(other.Characters).All(it => LookAlike(it.First, it.Second));

   static bool LookAlike(FormattedCharacter one, RenderedCharacter other) => one.Value == other.Value && (one.IsWhiteSpace || one.Formatting.LooksLike(other.Formatting));
}
