using Compze.Must;
using Compze.xUnitBDD;
using static Mavalonia.MarkdownEditing.Specifications.Editing.Editing;

namespace Mavalonia.MarkdownEditing.Specifications.Editing;

public class Starting_a_new_block
{
   static string From(string markdown) => Of(markdown, it => it.StartNewBlock());

   [XF] public void at_the_end_of_a_paragraph_adds_an_empty_one() => From("a‸").Must().Be("a\n\n‸");
   [XF] public void in_the_middle_of_a_paragraph_makes_it_two() => From("a‸b").Must().Be("a\n\n‸b");
   [XF] public void before_a_space_leaves_the_space_out() => From("a‸ b").Must().Be("a\n\n‸b");
   [XF] public void at_the_start_of_a_paragraph_adds_an_empty_one_before_it() => From("a\n\n‸b").Must().Be("a\n\n\n\n‸b");
   [XF] public void between_two_paragraphs_adds_an_empty_one() => From("a‸\n\nb").Must().Be("a\n\n‸\n\nb");
   [XF] public void in_an_empty_document_adds_an_empty_paragraph() => From("‸").Must().Be("\n\n‸");
   [XF] public void in_the_middle_of_bold_text_leaves_both_parts_bold() => From("**a‸b**").Must().Be("**a**\n\n‸**b**");
   [XF] public void after_bold_text_leaves_the_markup_with_it() => From("**a‸** b").Must().Be("**a**\n\n‸b");
   [XF] public void at_a_line_break_replaces_the_line_break() => From("a‸\nb").Must().Be("a\n\n‸b");
   [XF] public void after_the_line_break_at_the_end_replaces_the_line_break() => From("a\n‸").Must().Be("a\n\n‸");
   [XF] public void with_windows_line_breaks_adds_one_of_them() => From("a\r\n\r\nb‸").Must().Be("a\r\n\r\nb\r\n\r\n‸");

   [XF] public void at_the_end_of_a_heading_adds_an_empty_paragraph() => From("# a‸").Must().Be("# a\n\n‸");
   [XF] public void in_the_middle_of_a_heading_makes_a_paragraph_of_the_rest() => From("# a‸b").Must().Be("# a\n\n‸b");
   [XF] public void at_the_start_of_a_heading_adds_an_empty_paragraph_before_it() => From("# ‸a").Must().Be("\n\n# ‸a");
   [XF] public void in_an_empty_heading_makes_it_a_paragraph() => From("# ‸").Must().Be("‸");

   [XF] public void in_a_list_item_adds_another() => From("- a‸").Must().Be("- a\n- ‸");
   [XF] public void in_the_middle_of_a_list_item_makes_it_two() => From("- a‸b").Must().Be("- a\n- ‸b");
   [XF] public void in_a_numbered_list_item_adds_the_next_number() => From("9. a‸").Must().Be("9. a\n10. ‸");
   [XF] public void in_a_list_item_of_a_list_in_a_list_adds_another_to_the_inner_list() => From("- a\n  - b‸").Must().Be("- a\n  - b\n  - ‸");
   [XF] public void in_a_list_item_in_a_quote_adds_another_in_the_quote() => From("> - a‸").Must().Be("> - a\n> - ‸");
   [XF] public void in_a_second_paragraph_of_a_list_item_adds_a_third() => From("- a\n\n  b‸").Must().Be("- a\n\n  b\n\n  ‸");
   [XF] public void in_an_empty_list_item_at_the_end_ends_the_list() => From("- a\n- ‸").Must().Be("- a\n\n‸");
   [XF] public void in_an_empty_list_item_in_the_middle_ends_one_list_and_starts_another() => From("- a\n- ‸\n- b").Must().Be("- a\n\n‸\n\n- b");
   [XF] public void in_an_empty_list_item_that_is_all_there_is_leaves_an_empty_document() => From("- ‸").Must().Be("‸");
   [XF] public void in_an_empty_list_item_of_a_list_in_a_list_makes_it_an_item_of_the_outer_list() => From("- a\n  - b\n  - ‸").Must().Be("- a\n  - b\n- ‸");
   [XF] public void in_an_empty_numbered_list_item_of_a_list_in_a_list_makes_it_the_next_item_of_the_outer_list() => From("1. a\n   - b\n   - ‸").Must().Be("1. a\n   - b\n2. ‸");

   [XF] public void in_a_quote_adds_a_paragraph_to_it() => From("> a‸").Must().Be("> a\n>\n> ‸");
   [XF] public void in_an_empty_paragraph_at_the_end_of_a_quote_leaves_the_quote() => From("> a\n>\n> ‸").Must().Be("> a\n\n‸");
   [XF] public void in_an_empty_paragraph_in_the_middle_of_a_quote_makes_it_two_quotes() => From("> a\n>\n> ‸\n>\n> b").Must().Be("> a\n\n‸\n\n> b");
   [XF] public void in_an_empty_quote_makes_it_a_paragraph() => From("> ‸").Must().Be("‸");
   [XF] public void in_an_empty_paragraph_of_a_quote_in_a_quote_leaves_the_inner_quote() => From("> > a\n> >\n> > ‸").Must().Be("> > a\n>\n> ‸");

   [XF] public void in_code_adds_a_line() => From("```\na‸b\n```").Must().Be("```\na\n‸b\n```");
   [XF] public void in_code_in_a_quote_adds_a_line_in_the_quote() => From("> ```\n> a‸\n> ```").Must().Be("> ```\n> a\n> ‸\n> ```");
   [XF] public void on_an_empty_last_line_of_code_leaves_the_code() => From("```\na\n‸\n```").Must().Be("```\na\n```\n\n‸");
   [XF] public void on_an_empty_last_line_of_code_before_a_paragraph_adds_a_paragraph_between_them() => From("```\na\n‸\n```\n\nb").Must().Be("```\na\n```\n\n‸\n\nb");
   [XF] public void on_an_empty_last_line_of_code_that_is_not_closed_closes_it() => From("```\na\n‸").Must().Be("```\na\n```\n\n‸");
   [XF] public void on_the_only_line_of_code_adds_a_line() => From("```\n‸\n```").Must().Be("```\n\n‸\n```");

   [XF] public void in_a_cell_goes_to_the_cell_below() => From("| A | B |\n|---|---|\n| 1‸ | 2 |\n| 3 | 4 |").Must().Be("| A | B |\n|---|---|\n| 1 | 2 |\n| 3‸ | 4 |");
   [XF] public void in_a_cell_of_the_last_row_adds_a_paragraph_after_the_table() => From("| A |\n|---|\n| 1‸ |").Must().Be("| A |\n|---|\n| 1 |\n\n‸");
   [XF] public void in_a_cell_of_the_last_row_before_an_empty_paragraph_goes_there() => From("| A |\n|---|\n| 1‸ |\n\n").Must().Be("| A |\n|---|\n| 1 |\n\n‸");
   [XF] public void at_a_thematic_break_adds_a_paragraph_after_it() => From("a\n\n---‸\n\nb").Must().Be("a\n\n---\n\n‸\n\nb");

   [XF] public void after_what_is_selected_replaces_it() => From("a⟦b⟧c").Must().Be("a\n\n‸c");
}
