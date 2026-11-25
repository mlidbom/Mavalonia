using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Specifications.CommonMark;

namespace Mavalonia.MarkdownEditing.Specifications.Rendering;

public class The_source_of_a_rendered_character
{
   static SourceRange Of(char character, string markdown) => RenderedMarkdown.Of(markdown).Blocks.SelectMany(it => it.Text.Characters).Single(it => it.Value == character).Source;
   static SourceRange Range(string markdown, string ofText) => new(markdown.IndexOf(ofText, StringComparison.Ordinal), markdown.IndexOf(ofText, StringComparison.Ordinal) + ofText.Length);

   [XF] public void includes_the_backslash_of_an_escaped_character() => Of('*', @"a \* b").Must().Be(Range(@"a \* b", @"\*"));
   [XF] public void includes_both_backslashes_of_an_escaped_backslash() => Of('\\', @"a \\ b").Must().Be(Range(@"a \\ b", @"\\"));
   [XF] public void is_all_of_an_entity() => Of('&', "a &amp; b").Must().Be(Range("a &amp; b", "&amp;"));
   [XF] public void includes_the_quote_mark_after_a_line_break() => Of('\n', "> a\n> b").Must().Be(Range("> a\n> b", "\n> "));
   [XF] public void includes_the_quote_mark_after_a_line_break_with_windows_line_breaks() => Of('\n', "> a\r\n> b").Must().Be(Range("> a\r\n> b", "\r\n> "));
   [XF] public void includes_the_indentation_after_a_line_break_in_a_list_item() => Of('\n', "- a\n  b").Must().Be(Range("- a\n  b", "\n  "));
   [XF] public void includes_the_indentation_after_a_line_break_in_code_in_a_list_item() => Of('\n', "- ```\n  a\n  b\n  ```").Must().Be(new SourceRange(9, 12));
   [XF] public void includes_the_two_spaces_before_a_hard_line_break() => Of('\n', "a  \nb").Must().Be(Range("a  \nb", "  \n"));
   [XF] public void includes_the_backslash_before_a_hard_line_break() => Of('\n', "a\\\nb").Must().Be(Range("a\\\nb", "\\\n"));
   [XF] public void includes_the_line_break_and_quote_mark_in_code_of_several_lines_in_a_quote() => Of(' ', "> `a\n> b`").Must().Be(Range("> `a\n> b`", "\n> "));

   ///<summary>Changes each letter that is shown of each example, at its source, and describes each time that anything else than that letter came to look different.</summary>
   static IReadOnlyList<string> LettersWhoseSourceIsNotWhereTheyAre(Func<string, string> placed) =>
   [
      ..from example in CommonMarkExample.All
        where !example.DefinesLinks // Changing the letters of a link's label there makes it another label, and so no link.
        let markdown = placed(example.Markdown)
        let rendered = RenderedMarkdown.Of(markdown)
        from change in ChangesOfOneLetterIn(rendered)
        where TextsOf(RenderedMarkdown.Of(change.Markdown)) != change.ExpectedTexts
        select $"#{example.Number} {change.Markdown}"
   ];

   static string TextsOf(RenderedMarkdown rendered) => string.Join('\u0001', rendered.Blocks.Select(it => it.Text.Value));

   static IEnumerable<(string Markdown, string ExpectedTexts)> ChangesOfOneLetterIn(RenderedMarkdown rendered)
   {
      var texts = TextsOf(rendered);
      var startOfText = 0;
      foreach(var block in rendered.Blocks)
      {
         foreach(var (index, character) in block.Text.Characters.Index().Where(it => char.IsAsciiLetter(it.Item.Value) && !it.Item.Formatting.IsShownAsSource && it.Item.Source.Length == 1))
         {
            var another = AnotherLetterThan(character.Value);
            yield return (With(rendered.Source, character.Source.Start, another), With(texts, startOfText + index, another));
         }

         startOfText += block.Text.Length + 1;
      }

      static char AnotherLetterThan(char letter) => char.IsUpper(letter) ? letter == 'Z' ? 'Y' : 'Z' : letter == 'q' ? 'k' : 'q';
      static string With(string text, int index, char letter) => text[..index] + letter + text[(index + 1)..];
   }

   static string EachLineOf(string markdown, string ofTheFirst, string ofTheOthers) =>
      string.Join('\n', markdown.TrimEnd('\n').Split('\n').Select((line, number) => (number == 0 ? ofTheFirst : ofTheOthers) + line)) + "\n";

   [XF] public void is_where_changing_it_changes_that_character_only_in_every_CommonMark_example() => LettersWhoseSourceIsNotWhereTheyAre(it => it).Must().BeEmpty();
   [XF] public void is_where_changing_it_changes_that_character_only_in_every_CommonMark_example_with_windows_line_breaks() => LettersWhoseSourceIsNotWhereTheyAre(it => it.Replace("\n", "\r\n")).Must().BeEmpty();
   [XF] public void is_where_changing_it_changes_that_character_only_in_every_CommonMark_example_inside_a_quote() => LettersWhoseSourceIsNotWhereTheyAre(it => EachLineOf(it, "> ", "> ")).Must().BeEmpty();
   [XF] public void is_where_changing_it_changes_that_character_only_in_every_CommonMark_example_inside_a_list_item() => LettersWhoseSourceIsNotWhereTheyAre(it => EachLineOf(it, "- ", "  ")).Must().BeEmpty();

   [XF] public void is_where_changing_it_changes_that_character_only_in_a_table() =>
      ChangesOfOneLetterIn(RenderedMarkdown.Of("| Ab | **Cd** |\n|---|---|\n| `ef` | [gh](https://example.org) |\n| ij \\| kl | ~~mn~~ |"))
        .Where(it => TextsOf(RenderedMarkdown.Of(it.Markdown)) != it.ExpectedTexts).Must().BeEmpty();

   [XF] public void starts_where_the_source_of_the_character_before_it_ends_or_after_that_in_every_CommonMark_example() =>
      CommonMarkExample.All.Where(example => RenderedMarkdown.Of(example.Markdown).Blocks.Any(block => block.Text.Characters.Zip(block.Text.Characters.Skip(1)).Any(it => it.First.Source != it.Second.Source && it.First.Source.End > it.Second.Source.Start)))
                       .Select(it => it.Markdown).Must().BeEmpty();

   [XF] public void leads_from_an_index_to_an_offset_and_back_to_that_index_in_every_CommonMark_example() =>
      CommonMarkExample.All.Where(example => !example.HasEntities) // An entity may be two chars, between which there is no offset.
                       .Where(example => RenderedMarkdown.Of(example.Markdown).Blocks.Any(block => Enumerable.Range(0, block.Text.Length + 1).Any(it => block.Text.IndexAt(block.Text.OffsetAt(it)) != it)))
                       .Select(it => it.Markdown).Must().BeEmpty();
}
