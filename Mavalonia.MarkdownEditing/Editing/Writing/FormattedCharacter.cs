using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing.Writing;

///<summary>A character to write as markdown.</summary>
///<param name="Value">What is to be shown.</param>
///<param name="Formatting">How it is to look.</param>
///<param name="Markdown">What it is written as, formatting apart. In code, every character is written as it is.</param>
readonly record struct FormattedCharacter(char Value, TextFormatting Formatting, string Markdown)
{
   const string SpecialCharacters = @"\`*_~[]<>&|";

   internal static FormattedCharacter Typed(char character, TextFormatting formatting) => new(character, formatting, character.ToString());

   internal static FormattedCharacter In(RenderedMarkdown rendered, RenderedCharacter character) =>
      new(character.Value, character.Formatting, rendered.Source[character.Source.Start..character.Source.End]);

   internal bool IsWhiteSpace => char.IsWhiteSpace(Value);
   internal bool IsCode => Formatting.Has(TextFormats.Code);

   internal FormattedCharacter With(TextFormats format) => this with { Formatting = Formatting.With(format) };

   internal FormattedCharacter Without(TextFormats format) => this with { Formatting = Formatting.Without(format), Markdown = IsCode && format == TextFormats.Code ? Escaped : Markdown };

   ///<summary>The character escaped, so that it isn't taken for markup.</summary>
   string Escaped => SpecialCharacters.Contains(Value, StringComparison.Ordinal) ? @"\" + Value : Value.ToString();
}
