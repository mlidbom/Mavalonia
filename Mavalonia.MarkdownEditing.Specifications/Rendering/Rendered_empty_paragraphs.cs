using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Specifications.Rendering;

public class Rendered_empty_paragraphs
{
   static void Rendered(string markdown, params string[] blocks) => RenderedMarkdown.Of(markdown).Described.Must().SequenceEqual(blocks);

   [XF] public void are_the_one_block_of_an_empty_text() => Rendered("", "paragraph \"\"");
   [XF] public void are_there_for_a_blank_line_after_the_blank_line_that_ends_the_last_paragraph() => Rendered("a\n\n", "paragraph \"a\"", "paragraph \"\"");
   [XF] public void are_there_for_every_other_blank_line_after_the_last_paragraph() => Rendered("a\n\n\n\n", "paragraph \"a\"", "paragraph \"\"", "paragraph \"\"");
   [XF] public void are_there_for_the_middle_one_of_three_blank_lines_between_paragraphs() => Rendered("a\n\n\n\nb", "paragraph \"a\"", "paragraph \"\"", "paragraph \"b\"");
   [XF] public void are_not_there_for_the_one_blank_line_between_paragraphs() => Rendered("a\n\nb", "paragraph \"a\"", "paragraph \"b\"");
   [XF] public void are_not_there_for_two_blank_lines_between_paragraphs() => Rendered("a\n\n\nb", "paragraph \"a\"", "paragraph \"b\"");
   [XF] public void are_there_for_a_blank_line_before_the_blank_line_that_precedes_the_first_paragraph() => Rendered("\n\na", "paragraph \"\"", "paragraph \"a\"");
   [XF] public void are_there_for_the_line_after_a_heading_at_the_end() => Rendered("# a\n", "heading 1 \"a\"", "paragraph \"\"");
   [XF] public void are_there_for_the_line_after_code_at_the_end() => Rendered("```\na\n```\n", "code \"a\"", "paragraph \"\"");
   [XF] public void are_not_there_for_the_line_after_a_table_at_the_end() => Rendered("| A |\n|---|\n", "header 0,0 \"A\"");
   [XF] public void are_there_for_the_second_line_after_a_table_at_the_end() => Rendered("| A |\n|---|\n\n", "header 0,0 \"A\"", "paragraph \"\"");

   [XF] public void are_inside_a_quote_for_a_lone_quote_mark() => Rendered("> ", "quote > paragraph \"\"");
   [XF] public void are_inside_the_quote_whose_mark_is_on_their_line() => Rendered("> a\n>\n> ", "quote > paragraph \"a\"", "quote > paragraph \"\"");
   [XF] public void are_outside_the_quote_whose_mark_is_not_on_their_line() => Rendered("> a\n\n", "quote > paragraph \"a\"", "paragraph \"\"");
   [XF] public void are_inside_the_list_item_for_a_marker_and_a_space() => Rendered("- a\n- ", "• > paragraph \"a\"", "• > paragraph \"\"");
   [XF] public void are_inside_the_list_item_whose_indentation_is_on_their_line() => Rendered("- a\n\n  ", "• > paragraph \"a\"", "• > paragraph \"\"");
   [XF] public void are_inside_a_list_inside_a_quote() => Rendered("> - a\n> - ", "quote > • > paragraph \"a\"", "quote > • > paragraph \"\"");
   [XF] public void are_empty_headings_for_a_number_sign_and_a_space() => Rendered("# ", "heading 1 \"\"");
   [XF] public void are_empty_code_for_an_empty_line_between_fences() => Rendered("```\n\n```", "code \"\"");
   [XF] public void are_empty_code_for_fences_with_no_line_between_them() => Rendered("```\n```", "code \"\"");
}
