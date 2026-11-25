using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing.Writing;

///<summary>A char to write as markdown.</summary>
///<param name="Value">What is to be shown.</param>
///<param name="Formatting">How it is to look.</param>
///<param name="Markdown">What it is written as, formatting apart. In code, every char is written as it is.</param>
readonly record struct FormattedChar(char Value, TextFormatting Formatting, string Markdown)
{
   const string SpecialCharacters = @"\`*_~[]<>&|";

   internal static FormattedChar Typed(char value, TextFormatting formatting) => new(value, formatting, value.ToString());

   internal static FormattedChar In(RenderedMarkdown rendered, RenderedChar renderedChar) =>
      new(renderedChar.Value, renderedChar.Formatting, rendered.Source[renderedChar.Source.Start..renderedChar.Source.End]);

   internal bool IsWhiteSpace => char.IsWhiteSpace(Value);
   internal bool IsCode => Formatting.Has(TextFormats.Code);

   internal FormattedChar With(TextFormats format) => this with { Formatting = Formatting.With(format) };

   internal FormattedChar Without(TextFormats format) => this with { Formatting = Formatting.Without(format), Markdown = IsCode && format == TextFormats.Code ? Escaped : Markdown };

   ///<summary>The char, escaped if it could be taken for markup inside a block.</summary>
   internal FormattedChar WrittenEscaped => this with { Markdown = Escaped };

   ///<summary>The char, escaped whatever it is, as far as markdown lets it be.</summary>
   internal FormattedChar WrittenEscapedIfPunctuation => char.IsAscii(Value) && (char.IsPunctuation(Value) || char.IsSymbol(Value)) ? this with { Markdown = @"\" + Value } : this;

   ///<summary>The char escaped, so that it isn't taken for markup.</summary>
   string Escaped => SpecialCharacters.Contains(Value, StringComparison.Ordinal) ? @"\" + Value : Value.ToString();
}
