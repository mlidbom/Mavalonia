using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Specifications.Rendering;

public class Rendered_blocks
{
   static void Rendered(string markdown, params string[] blocks) => RenderedMarkdown.Of(markdown).Described.Must().SequenceEqual(blocks);
   static string Repeated(string text, int times) => string.Concat(Enumerable.Repeat(text, times));

   [XF] public void are_paragraphs_for_texts_with_a_blank_line_between_them() => Rendered("one\n\ntwo", "paragraph \"one\"", "paragraph \"two\"");
   [XF] public void have_a_single_line_break_in_the_text_of_the_paragraph() => Rendered("one\ntwo", "paragraph \"one\ntwo\"");
   [XF] public void have_the_level_of_a_heading_from_its_number_signs() => Rendered("### title", "heading 3 \"title\"");
   [XF] public void are_not_headings_for_text_underlined_with_dashes() => Rendered("one\n---", "paragraph \"one\n---\"");
   [XF] public void are_not_headings_for_text_underlined_with_equals_signs() => Rendered("one\n===", "paragraph \"one\n===\"");
   [XF] public void are_not_code_for_indented_text() => Rendered("    one", "paragraph \"one\"");
   [XF] public void show_what_is_between_two_code_fences_as_code() => Rendered("```cs\none\n\ntwo\n```", "code cs \"one\n\ntwo\"");
   [XF] public void show_what_follows_a_code_fence_that_is_not_closed_as_code() => Rendered("```\none", "code \"one\"");
   [XF] public void show_three_dashes_as_a_thematic_break() => Rendered("one\n\n---\n\ntwo", "paragraph \"one\"", "break \"\"", "paragraph \"two\"");
   [XF] public void show_HTML_as_source() => Rendered("<div>\none\n</div>", "source \"<div>\none\n</div>\"");
   [XF] public void show_the_definition_of_a_link_as_source() => Rendered("[one][1]\n\n[1]: https://example.org", "paragraph \"one\"", "source \"[1]: https://example.org\"");

   [XF] public void are_inside_the_quotes_they_are_written_in() => Rendered("> one\n>\n> > two", "quote > paragraph \"one\"", "quote > quote > paragraph \"two\"");
   [XF] public void leave_out_the_quote_mark_of_every_line_of_a_paragraph() => Rendered("> one\n> two", "quote > paragraph \"one\ntwo\"");
   [XF] public void are_inside_the_list_items_they_are_written_in() => Rendered("- one\n- two\n  - three", "• > paragraph \"one\"", "• > paragraph \"two\"", "• > • > paragraph \"three\"");
   [XF] public void number_the_items_of_a_list_from_the_number_of_the_first() => Rendered("3. one\n3. two", "3. > paragraph \"one\"", "4. > paragraph \"two\"");
   [XF] public void show_the_numbers_of_a_list_with_the_parenthesis_that_they_are_written_with() => Rendered("3) one\n4) two", "3) > paragraph \"one\"", "4) > paragraph \"two\"");
   [XF] public void can_be_several_in_a_list_item() => Rendered("- one\n\n  two\n\n  ```\n  three\n  ```", "• > paragraph \"one\"", "• > paragraph \"two\"", "• > code \"three\"");
   [XF] public void can_be_inside_a_list_inside_a_quote() => Rendered("> 1. one\n>\n>    two", "quote > 1. > paragraph \"one\"", "quote > 1. > paragraph \"two\"");

   [XF] public void are_the_cells_of_a_table() => Rendered("| A | B |\n|---|---|\n| 1 | 2 |", "header 0,0 \"A\"", "header 0,1 \"B\"", "cell 1,0 \"1\"", "cell 1,1 \"2\"");
   [XF] public void are_the_cells_of_a_table_inside_a_quote() => Rendered("> | A | B |\n> |---|---|\n> | 1 | 2 |", "quote > header 0,0 \"A\"", "quote > header 0,1 \"B\"", "quote > cell 1,0 \"1\"", "quote > cell 1,1 \"2\"");
   [XF] public void are_empty_cells_too() => Rendered("| A |  |\n|---|---|\n|   | 2 |", "header 0,0 \"A\"", "header 0,1 \"\"", "cell 1,0 \"\"", "cell 1,1 \"2\"");
   [XF] public void are_in_order_in_a_table_with_rows_that_are_all_empty() => RenderedMarkdown.Of("a\n\n| A |  |\n|---|---|\n|  |  |\n|  | b |\n|  |  |\n\nc").Blocks.Select(it => it.Text.Source.Start).Must().SequenceEqual([0, 5, 9, 24, 27, 32, 35, 41, 44, 48]);
   [XF] public void are_only_the_cells_that_a_row_of_a_table_has() => Rendered("| A | B |\n|---|---|\n| 1\n\na", "header 0,0 \"A\"", "header 0,1 \"B\"", "cell 1,0 \"1\"", "paragraph \"a\"");
   [XF] public void are_only_cells_where_the_lines_of_a_table_end_with_a_tab() => Rendered("| A |\t\n|---|\n| 1 |\t\n| 2 |", "header 0,0 \"A\"", "cell 1,0 \"1\"", "cell 2,0 \"2\"");
   [XF] public void show_a_table_with_no_cell_at_all_as_source() => Rendered("a\n\n|\n-|", "paragraph \"a\"", "source \"|\n-|\"");
   [XF] public void show_all_as_source_when_a_link_goes_on_into_the_header_of_a_table() => Rendered("*[|\n]()-|-", "source \"*[|\n]()-|-\"");
   [XF] public void show_a_table_as_source_when_a_link_goes_on_outside_its_cell() => Rendered("| A | B[**#** |\n|---|---|\n|   | 2](x) |", "source \"| A | B[**#** |\n|---|---|\n|   | 2](x) |\"");
   [XF] public void show_all_as_source_when_quotes_are_inside_one_another_hundreds_deep() => Rendered(new string('>', 500) + "a", $"source \"{new string('>', 500)}a\"");
   [XF] public void show_all_as_source_when_list_items_are_inside_one_another_hundreds_deep() => Rendered(Repeated("- ", 500) + "a", $"source \"{Repeated("- ", 500)}a\"");
   [XF] public void show_all_as_source_when_formats_are_inside_one_another_hundreds_deep() => Rendered(Repeated("*a **a ", 250) + Repeated("a** a* ", 250) + "b", $"source \"{Repeated("*a **a ", 250) + Repeated("a** a* ", 250)}b\"");
   [XF] public void show_an_escaped_pipe_in_a_cell_without_its_backslash() => Rendered("| A |\n|---|\n| a \\| b |", "header 0,0 \"A\"", "cell 1,0 \"a | b\"");
   [XF] public void show_an_escaped_pipe_in_code_in_a_cell_without_its_backslash() => Rendered("| A |\n|---|\n| `a \\| b` |", "header 0,0 \"A\"", "cell 1,0 \"a | b\"");
   [XF] public void show_an_escaped_pipe_in_code_outside_of_a_table_with_its_backslash() => Rendered("`a \\| b`", "paragraph \"a \\| b\"");

   [XF] public void are_the_same_with_windows_line_breaks() => Rendered("> one\r\n> two\r\n\r\n- three\r\n- four", "quote > paragraph \"one\ntwo\"", "• > paragraph \"three\"", "• > paragraph \"four\"");
}
