using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.Markdown.Editing;
using Mavalonia.Markdown.Rendering.Texts;
using Mavalonia.Markdown.Specifications.Rendering;
using static Mavalonia.Markdown.Specifications.Editing.Editing;

namespace Mavalonia.Markdown.Specifications.Editing;

public class Typing_text
{
   [XF] public void inserts_it_at_the_caret() => Of("a‸b", Type("x")).Must().Be("ax‸b");
   [XF] public void leaves_all_other_markdown_as_it_is_written() => Of("*  odd‸   list\n*  two\n\n__para__\\\nx", Type("Z")).Must().Be("*  oddZ‸   list\n*  two\n\n__para__\\\nx");
   [XF] public void replaces_what_is_selected() => Of("a⟦bc⟧d", Type("x")).Must().Be("ax‸d");
   [XF] public void replaces_what_is_selected_in_the_format_it_has() => Of("a **⟦bold⟧** b", Type("x")).Must().Be("a **x‸** b");
   [XF] public void replaces_what_is_selected_across_paragraphs() => Of("a⟦b\n\nc⟧d", Type("x")).Must().Be("ax‸d");
   [XF] public void in_an_empty_document_makes_it_all_of_the_document() => Of("‸", Type("x")).Must().Be("x‸");

   [XF] public void inside_bold_text_continues_the_bold_text() => Of("**bo‸ld**", Type("X")).Must().Be("**boX‸ld**");
   [XF] public void at_the_end_of_bold_text_continues_the_bold_text() => Of("**bold‸** b", Type("X")).Must().Be("**boldX‸** b");
   [XF] public void after_the_markup_that_ends_bold_text_does_not_continue_it() => Of("**bold**‸", Type("X")).Must().Be("**bold**X‸");
   [XF] public void at_the_end_of_bold_text_continues_it_over_a_space() => Of("**bold‸** b", Type(" and more")).Must().Be("**bold and more‸** b");
   [XF] public void that_is_a_space_at_the_end_of_bold_text_keeps_the_text_bold() => Of("**bold‸**", Type(" ")).Must().Be("**bold** ‸");
   [XF] public void that_is_a_space_at_the_start_of_italic_text_keeps_the_text_italic() => Of("a *‸b*", Type(" ")).Must().Be("a  ‸*b*");
   [XF] public void inside_a_link_changes_its_label() => Of("[la‸bel](https://example.org)", Type("X")).Must().Be("[laX‸bel](https://example.org)");
   [XF] public void inside_code_is_not_taken_for_markup() => Of("`a‸b`", Type("**")).Must().Be("`a**‸b`");

   [XF] public void between_double_asterisks_makes_it_bold() => RenderedAfterTyping("a **b** c").Must().Be("a [Bold: b] c");
   [XF] public void between_backticks_makes_it_code() => RenderedAfterTyping("a `b` c").Must().Be("a [Code: b] c");
   [XF] public void between_double_tildes_strikes_it_through() => RenderedAfterTyping("a ~~b~~ c").Must().Be("a [Strikethrough: b] c");
   [XF] public void that_is_a_link_in_markdown_makes_it_a_link() => RenderedAfterTyping("a [b](https://example.org) c").Must().Be("a [link to https://example.org: b] c");

   [XF] public void after_a_number_sign_and_a_space_makes_it_a_heading() => Blocks("# a").Must().SequenceEqual(["heading 1 \"a\""]);
   [XF] public void after_a_number_sign_alone_does_not_make_it_a_heading() => Blocks("#5").Must().SequenceEqual(["paragraph \"#5\""]);
   [XF] public void after_a_dash_and_a_space_makes_it_a_list_item() => Blocks("- a").Must().SequenceEqual(["• > paragraph \"a\""]);
   [XF] public void after_a_dash_alone_does_not_make_it_a_list_item() => Blocks("-5").Must().SequenceEqual(["paragraph \"-5\""]);
   [XF] public void after_a_number_a_dot_and_a_space_makes_it_a_numbered_list_item() => Blocks("1. a").Must().SequenceEqual(["1. > paragraph \"a\""]);
   [XF] public void after_a_number_a_parenthesis_and_a_space_makes_it_a_numbered_list_item() => Blocks("1) a").Must().SequenceEqual(["1) > paragraph \"a\""]);
   [XF] public void after_a_quote_mark_makes_it_a_quote() => Blocks("> a").Must().SequenceEqual(["quote > paragraph \"a\""]);
   [XF] public void after_a_quote_mark_and_a_dash_makes_it_a_list_item_in_a_quote() => Blocks("> - a").Must().SequenceEqual(["quote > • > paragraph \"a\""]);
   [XF] public void after_three_backticks_makes_it_code() => Of("‸", Type("```a")).Must().Be("```\na‸\n```");
   [XF] public void after_three_backticks_makes_it_code_without_making_code_of_what_follows() => Of("‸\n\nb", Type("```a")).Must().Be("```\na‸\n```\n\nb");
   [XF] public void after_three_backticks_in_a_quote_makes_it_code_in_the_quote() => Of("> ‸", Type("```a")).Must().Be("> ```\n> a‸\n> ```");
   [XF] public void that_is_a_dash_under_a_line_does_not_make_that_line_a_heading() => Blocks("a", it => it.BreakLine(), Type("-")).Must().SequenceEqual(["paragraph \"a\n-\""]);

   [XF] public void in_a_cell_changes_that_cell() => Of("| A | B |\n|---|---|\n| 1‸ | 2 |", Type("x")).Must().Be("| A | B |\n|---|---|\n| 1x‸ | 2 |");
   [XF] public void in_an_empty_cell_changes_that_cell() => Of("| A | B |\n|---|---|\n| 1 | ‸ |", Type("x")).Must().Be("| A | B |\n|---|---|\n| 1 | x‸ |");
   [XF] public void that_is_a_pipe_in_a_cell_escapes_it() => Of("| A |\n|---|\n| 1‸ |", Type("|")).Must().Be("| A |\n|---|\n| 1\\|‸ |");
   [XF] public void that_is_a_pipe_in_code_in_a_cell_escapes_it() => Of("| A |\n|---|\n| `1‸` |", Type("|")).Must().Be("| A |\n|---|\n| `1\\|‸` |");
   [XF] public void in_code_inserts_it() => Of("```\na‸\n```", Type("*b*")).Must().Be("```\na*b*‸\n```");
   [XF] public void in_code_with_no_line_adds_the_line() => Of("```‸\n```", Type("a")).Must().Be("```\na‸\n```");
   [XF] public void on_an_empty_line_of_code_in_a_list_item_indents_the_line() => Of("- ```\n  a\n‸\n  ```", Type("b")).Must().Be("- ```\n  a\n  b‸\n  ```");
   [XF] public void in_an_empty_paragraph_leaves_it_a_paragraph_of_its_own() => Of("a\n\n‸\n\nb", Type("x")).Must().Be("a\n\nx‸\n\nb");
   [XF] public void in_an_empty_paragraph_before_a_quote_leaves_it_outside_the_quote() => Of("‸\n\n> b", Type("x")).Must().Be("x‸\n\n> b");

   [XF] public void after_toggling_bold_makes_it_bold() => Of("a ‸", it => it.Toggle(TextFormats.Bold), Type("b c")).Must().Be("a **b c‸**");
   [XF] public void after_toggling_bold_twice_does_not_make_it_bold() => Of("a ‸", it => it.Toggle(TextFormats.Bold), it => it.Toggle(TextFormats.Bold), Type("b")).Must().Be("a b‸");
   [XF] public void after_toggling_bold_at_the_end_of_bold_text_does_not_continue_the_bold_text() => Of("**a‸**", it => it.Toggle(TextFormats.Bold), Type(" b")).Must().Be("**a** b‸");
   [XF] public void after_toggling_bold_inside_bold_text_makes_it_not_bold() => Of("**a‸b**", it => it.Toggle(TextFormats.Bold), Type("x")).Must().Be("**a**x‸**b**");
   [XF] public void after_toggling_code_makes_it_code() => Of("a ‸", it => it.Toggle(TextFormats.Code), Type("b")).Must().Be("a `b‸`");
   [XF] public void after_toggling_code_at_the_end_of_code_does_not_continue_the_code() => Of("`a‸`", it => it.Toggle(TextFormats.Code), Type(" b")).Must().Be("`a` b‸");
   [XF] public void after_toggling_code_inside_code_makes_it_not_code() => Of("`a‸b`", it => it.Toggle(TextFormats.Code), Type(" ")).Must().Be("`a` ‸`b`");
   [XF] public void inside_the_end_of_code_makes_it_code() => Of("`a‸`", Type(" b")).Must().Be("`a b‸`");
   [XF] public void outside_the_end_of_code_does_not_make_it_code() => Of("`a`‸", Type(" b")).Must().Be("`a` b‸");
   [XF] public void after_moving_the_caret_out_of_the_end_of_code_does_not_make_it_code() => Of("`a‸`", it => it.Move(CaretMovement.ToTheCharacterAfter), Type(" b")).Must().Be("`a` b‸");
   [XF] public void inside_the_start_of_code_makes_it_code() => Of("`‸a`", Type("b ")).Must().Be("`b ‸a`");
   [XF] public void after_moving_the_caret_into_the_start_of_code_makes_it_code() => Of("x ‸`a`", it => it.Move(CaretMovement.ToTheCharacterAfter), Type("b")).Must().Be("x `b‸a`");

   static string RenderedAfterTyping(string text)
   {
      using var editor = EditorOf("‸");
      Type(text)(editor);
      return editor.Rendered.Blocks.Single().Text.DescribedWithFormatting;
   }

   static IReadOnlyList<string> Blocks(string typed, params Action<RenderedEditor>[] then)
   {
      using var editor = EditorOf("‸");
      Type(typed)(editor);
      foreach(var edit in then) edit(editor);
      return editor.Rendered.Described;
   }

   [XF] public void that_is_the_line_under_the_header_of_a_table_shows_it_as_text_until_a_new_block_is_started() =>
      Of("| a | b |‸", it => it.BreakLine(), Type("|---|---|")).Must().Be("| a | b |\n|---\\|---|‸");
}
