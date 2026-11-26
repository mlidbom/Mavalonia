using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.Markdown.Rendering.Texts;
using static Mavalonia.Markdown.Specifications.Editing.Editing;

namespace Mavalonia.Markdown.Specifications.Editing;

public class Formatting_text
{
   static string Bold(string markdown) => Of(markdown, it => it.Toggle(TextFormats.Bold));
   static string Italic(string markdown) => Of(markdown, it => it.Toggle(TextFormats.Italic));
   static string Code(string markdown) => Of(markdown, it => it.Toggle(TextFormats.Code));

   [XF] public void makes_what_is_selected_bold() => Bold("a ⟦b⟧ c").Must().Be("a ⟦**b⟧** c");
   [XF] public void makes_what_is_selected_bold_without_the_spaces_around_it() => Bold("a⟦ b ⟧c").Must().Be("a⟦ **b** ⟧c");
   [XF] public void takes_bold_away_from_what_is_selected_and_bold() => Bold("a **⟦b⟧** c").Must().Be("a ⟦b⟧ c");
   [XF] public void takes_bold_away_from_a_part_of_what_is_bold() => Bold("**a⟦b⟧c**").Must().Be("**a⟦**b⟧**c**");
   [XF] public void makes_all_of_what_is_selected_bold_when_some_of_it_is() => Bold("⟦a **b** c⟧").Must().Be("⟦**a b c⟧**");
   [XF] public void makes_italic_what_is_bold() => Italic("**a ⟦b⟧ c**").Must().Be("**a ⟦*b⟧* c**");
   [XF] public void makes_bold_next_to_italic_text() => Bold("*a*⟦b⟧").Must().Satisfy(it => it.Contains("**b") && it.Contains('a'));
   [XF] public void makes_code_of_what_is_selected() => Code("a ⟦b⟧ c").Must().Be("a ⟦`b⟧` c");
   [XF] public void makes_code_of_what_has_backticks_with_more_backticks_around_it() => Code("⟦a\\`b⟧").Must().Be("⟦``a`b⟧``");
   [XF] public void escapes_what_would_be_markup_in_what_is_no_longer_code() => Code("`⟦*a*⟧`").Must().Be("⟦\\*a\\*⟧");
   [XF] public void makes_bold_in_each_of_several_paragraphs() => Bold("a⟦b\n\nc⟧d").Must().Be("a⟦**b**\n\n**c⟧**d");
   [XF] public void makes_bold_in_a_cell() => Bold("| ⟦A⟧ |\n|---|").Must().Be("| ⟦**A⟧** |\n|---|");
   [XF] public void leaves_code_blocks_alone() => Bold("```\n⟦a⟧\n```").Must().Be("```\n⟦a⟧\n```");
   [XF] public void is_undone_as_one() => Of("a⟦b\n\nc⟧d", it => it.Toggle(TextFormats.Bold), it => it.Undo()).Must().Satisfy(it => it.Replace("‸", "") == "ab\n\ncd");
}
