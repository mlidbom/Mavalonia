using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Specifications.Rendering;

public class Rendered_text
{
   static string Of(string markdown) => RenderedMarkdown.Of(markdown).Blocks.Single().Text.DescribedWithFormatting;

   [XF] public void is_bold_between_two_double_asterisks() => Of("a **b** c").Must().Be("a [Bold: b] c");
   [XF] public void is_italic_between_two_asterisks() => Of("a *b* c").Must().Be("a [Italic: b] c");
   [XF] public void is_bold_and_italic_between_two_triple_asterisks() => Of("***b***").Must().Be("[Bold, Italic: b]");
   [XF] public void is_struck_through_between_two_double_tildes() => Of("a ~~b~~ c").Must().Be("a [Strikethrough: b] c");
   [XF] public void is_not_struck_through_between_two_single_tildes() => Of("a ~b~ c").Must().Be("a ~b~ c");
   [XF] public void is_code_between_two_backticks() => Of("a `**b**` c").Must().Be("a [Code: **b**] c");
   [XF] public void shows_the_label_of_a_link_only() => Of("a [b *c*](https://example.org \"title\") d").Must().Be("a [link to https://example.org: b ][Italic, link to https://example.org: c] d");
   [XF] public void shows_a_link_in_angle_brackets_without_them() => Of("<https://example.org>").Must().Be("[link to https://example.org: https://example.org]");
   [XF] public void shows_a_link_without_a_label_as_source() => Of("a [](b)").Must().Be("a [source: [](b)]");
   [XF] public void shows_a_link_whose_label_starts_with_a_line_break() => Of("a [\nb](c)").Must().Be("a [link to c: \nb]");
   [XF] public void shows_a_heading_with_a_tab_after_the_number_sign() => Of("#\ta **b**").Must().Be("a [Bold: b]");
   [XF] public void shows_an_image_as_source() => Of("a ![b](c.png)").Must().Be("a [source: ![b](c.png)]");
   [XF] public void shows_inline_HTML_as_source() => Of("a <b>c</b>").Must().Be("a [source: <b>]c[source: </b>]");
   [XF] public void shows_an_entity_as_what_it_stands_for() => Of("a &amp; b").Must().Be("a & b");
   [XF] public void shows_an_escaped_character_without_its_backslash() => Of(@"a \* b").Must().Be("a * b");
   [XF] public void shows_a_line_break_after_two_spaces_without_the_spaces() => Of("a  \nb").Must().Be("a\nb");
   [XF] public void shows_a_line_break_after_a_backslash_without_the_backslash() => Of("a\\\nb").Must().Be("a\nb");

   [XF] public void shows_an_opening_bracket_that_none_closes() => Of("a [ b").Must().Be("a [ b");
   [XF] public void shows_an_opening_bracket_that_none_closes_before_a_line_break() => Of("[\na").Must().Be("[\na");
   [XF] public void shows_a_closing_bracket_that_none_opens() => Of("a ] b").Must().Be("a ] b");
   [XF] public void shows_a_pipe_that_is_not_in_a_table() => Of("a | b").Must().Be("a | b");
   [XF] public void shows_asterisks_that_delimit_nothing() => Of("a * b ** c").Must().Be("a * b ** c");
   [XF] public void shows_underscores_inside_words() => Of("a_b_c").Must().Be("a_b_c");
   [XF] public void shows_tildes_that_delimit_nothing() => Of("a ~~ b ~ c").Must().Be("a ~~ b ~ c");
   [XF] public void shows_a_backtick_that_none_closes() => Of("a ` b").Must().Be("a ` b");
   [XF] public void shows_a_less_than_sign() => Of("a < b").Must().Be("a < b");
   [XF] public void shows_a_NUL_as_the_character_that_replaces_it() => Of("a" + (char)0 + "b").Must().Be("a" + (char)0xFFFD + "b");
   [XF] public void shows_thousands_of_opening_brackets_that_none_closes() => Of(new string('[', 5_000)).Must().Be(new string('[', 5_000));

   [XF] public void shows_the_spaces_at_the_end_of_a_paragraph() => Of("a  ").Must().Be("a  ");
   [XF] public void shows_the_spaces_after_bold_text_at_the_end_of_a_paragraph() => Of("**a** ").Must().Be("[Bold: a] ");
   [XF] public void shows_the_space_at_the_end_of_a_line_of_a_paragraph() => Of("a \nb").Must().Be("a \nb");
   [XF] public void shows_the_line_break_at_the_end_of_the_last_paragraph() => Of("a\n").Must().Be("a\n");

   [XF] public void shows_a_lone_dash_as_it_is() => Of("-").Must().Be("-");
   [XF] public void shows_a_lone_number_and_dot_as_they_are() => Of("1.").Must().Be("1.");
   [XF] public void shows_a_lone_number_sign_as_it_is() => Of("#").Must().Be("#");
}
