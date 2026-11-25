using Compze.Must;
using Compze.xUnitBDD;
using static Mavalonia.MarkdownEditing.Specifications.Editing.Editing;

namespace Mavalonia.MarkdownEditing.Specifications.Editing.Deleting_text;

public class before_the_caret
{
   static string From(string markdown) => Of(markdown, it => it.DeleteCharacterBefore());

   [XF] public void deletes_the_character_before_it() => From("ab‸c").Must().Be("a‸c");
   [XF] public void deletes_a_character_of_bold_text() => From("**ab‸c**").Must().Be("**a‸c**");
   [XF] public void deletes_the_markup_with_the_last_character_of_bold_text() => From("a **b‸** c").Must().Be("a ‸ c");
   [XF] public void leaves_the_rest_bold_when_it_ends_with_a_space() => From("**a b‸**").Must().Be("**a** ‸");
   [XF] public void deletes_an_emoji_of_several_chars_as_one() => From("a👩‍👩‍👧‸b").Must().Be("a‸b");
   [XF] public void deletes_an_entity_as_one() => From("a&amp;‸b").Must().Be("a‸b");
   [XF] public void deletes_an_escaped_character_with_its_backslash() => From(@"a\*‸b").Must().Be("a‸b");
   [XF] public void deletes_a_line_break() => From("a\n‸b").Must().Be("a‸b");
   [XF] public void deletes_a_line_break_in_a_quote_with_the_quote_mark_after_it() => From("> a\n> ‸b").Must().Be("> a‸b");
   [XF] public void at_the_start_of_the_document_changes_nothing() => From("‸a").Must().Be("‸a");

   [XF] public void leaves_a_list_item_that_becomes_empty_a_list_item() => From("- a\n- b‸").Must().Be("- a\n- ‸");
   [XF] public void leaves_the_item_of_a_list_in_a_list_that_becomes_empty_a_list_item() => From("- a\n  - b‸").Must().Be("- a\n\n  - ‸");
   [XF] public void joins_two_paragraphs() => From("a\n\n‸b").Must().Be("a‸b");
   [XF] public void joins_two_bold_paragraphs_as_bold_text() => From("**a**\n\n‸**b**").Must().Be("**a‸b**");
   [XF] public void joins_two_paragraphs_of_a_quote() => From("> a\n>\n> ‸b").Must().Be("> a‸b");
   [XF] public void deletes_an_empty_paragraph() => From("a\n\n‸").Must().Be("a‸");
   [XF] public void deletes_an_empty_paragraph_between_two() => From("a\n\n‸\n\nb").Must().Be("a‸\n\nb");
   [XF] public void deletes_an_empty_paragraph_before_a_heading() => From("a\n\n\n\n# ‸b").Must().Be("a\n\n\n\n‸b");
   [XF] public void deletes_an_empty_paragraph_before_a_paragraph() => From("a\n\n\n\n‸b").Must().Be("a\n\n‸b");
   [XF] public void makes_a_heading_a_paragraph() => From("# ‸a").Must().Be("‸a");
   [XF] public void deletes_a_thematic_break() => From("a\n\n---\n\n‸b").Must().Be("a\n\n‸b");
   [XF] public void deletes_an_empty_code_block() => From("a\n\n```\n‸\n```").Must().Be("a\n\n‸");
   [XF] public void after_a_code_block_goes_to_its_end() => From("```\na\n```\n\n‸b").Must().Be("```\na‸\n```\n\nb");
   [XF] public void deletes_an_empty_paragraph_after_a_code_block() => From("```\na\n```\n\n‸").Must().Be("```\na‸\n```");
   [XF] public void after_a_table_goes_to_its_last_cell() => From("| A |\n|---|\n| 1 |\n\n‸b").Must().Be("| A |\n|---|\n| 1‸ |\n\nb");
   [XF] public void in_an_empty_cell_goes_to_the_cell_before() => From("| A | ‸ |\n|---|---|").Must().Be("| A‸ |  |\n|---|---|");
   [XF] public void deletes_the_last_character_of_a_cell() => From("| A | B‸ |\n|---|---|").Must().Be("| A | ‸ |\n|---|---|");

   [XF] public void takes_a_paragraph_out_of_a_list() => From("- ‸a").Must().Be("‸a");
   [XF] public void takes_the_last_item_out_of_a_list() => From("- a\n- ‸b").Must().Be("- a\n\n‸b");
   [XF] public void takes_an_item_in_the_middle_out_of_a_list() => From("- a\n- ‸b\n- c").Must().Be("- a\n\n‸b\n\n- c");
   [XF] public void makes_an_item_of_a_list_in_a_list_one_of_the_outer_list() => From("- a\n  - ‸b").Must().Be("- a\n- ‸b");
   [XF] public void takes_a_paragraph_of_several_lines_out_of_a_list() => From("- ‸a\n  b").Must().Be("‸a\nb");
   [XF] public void joins_a_second_paragraph_of_a_list_item_with_the_first() => From("- a\n\n  ‸b").Must().Be("- a‸b");
   [XF] public void takes_a_paragraph_out_of_a_quote() => From("> ‸a").Must().Be("‸a");
   [XF] public void takes_a_paragraph_of_several_lines_out_of_a_quote() => From("> ‸a\n> b").Must().Be("‸a\nb");
   [XF] public void takes_the_first_paragraph_out_of_a_quote() => From("> ‸a\n>\n> b").Must().Be("‸a\n\n> b");
   [XF] public void takes_a_paragraph_out_of_a_quote_after_another_paragraph() => From("x\n\n> ‸a").Must().Be("x\n\n‸a");
}
