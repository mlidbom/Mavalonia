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

   internal IReadOnlyList<FormattedCharacter> Characters { get; }

   internal int Length => Characters.Count;

   internal FormattedText this[Range range] => new([..Characters.Take(range)]);

   internal FormattedText Followed(FormattedText by) => new([..Characters, ..by.Characters]);

   internal FormattedText Select(Func<FormattedCharacter, FormattedCharacter> change) => new([..Characters.Select(change)]);

   ///<summary>The text without formatting, written as markdown.</summary>
   internal string MarkdownWithoutFormatting => string.Concat(Characters.Select(it => it.Markdown));

   ///<summary>Whether the text looks like <paramref name="other"/>. White space looks the same in bold, so only the formatting of the other characters counts.</summary>
   internal bool LooksLike(RenderedText other) => Length == other.Length && Characters.Zip(other.Characters).All(it => LookAlike(it.First, it.Second));

   static bool LookAlike(FormattedCharacter one, RenderedCharacter other) => one.Value == other.Value && (one.IsWhiteSpace || LookAlike(one.Formatting, other.Formatting));

   static bool LookAlike(TextFormatting one, TextFormatting other) => one.Formats == other.Formats && one.Link?.Target == other.Link?.Target && one.IsShownAsSource == other.IsShownAsSource;
}
