using Compze.Must;
using Compze.xUnitBDD;
using static Mavalonia.MarkdownEditing.Specifications.Editing.Editing;

namespace Mavalonia.MarkdownEditing.Specifications.Editing;

public class Copying
{
   static string Markdown(string markdown)
   {
      using var editor = EditorOf(markdown);
      return editor.SelectedMarkdown;
   }

   [XF] public void a_part_of_bold_text_copies_bold_text() => Markdown("**a⟦b⟧c**").Must().Be("**b**");
   [XF] public void a_part_of_a_paragraph_copies_that_part() => Markdown("a ⟦b **c** d⟧ e").Must().Be("b **c** d");
   [XF] public void a_part_of_a_link_copies_a_link() => Markdown("[a⟦b⟧](https://example.org)").Must().Be("[b](https://example.org)");
   [XF] public void two_paragraphs_copies_both() => Markdown("a⟦b\n\nc⟧d").Must().Be("b\n\nc");
   [XF] public void items_of_a_list_copies_a_list() => Markdown("- ⟦a\n- b⟧\n- c").Must().Be("- a\n- b");
   [XF] public void a_part_of_one_item_copies_text() => Markdown("- a⟦b⟧").Must().Be("b");
   [XF] public void paragraphs_of_a_quote_copies_them_without_the_quote() => Markdown("> ⟦a\n>\n> b⟧").Must().Be("a\n\nb");
   [XF] public void what_is_in_and_outside_a_quote_copies_it_with_the_quote() => Markdown("⟦x\n\n> a⟧").Must().Be("x\n\n> a");
   [XF] public void a_list_in_a_quote_copies_the_list() => Markdown("> - ⟦a\n> - b⟧").Must().Be("- a\n- b");
   [XF] public void cells_of_a_table_copies_the_table() => Markdown("| A | B |\n|---|---|\n| ⟦1 | 2⟧ |").Must().Be("| A | B |\n|---|---|\n| 1 | 2 |");
   [XF] public void a_table_in_a_quote_copies_the_table() => Markdown("> | ⟦A |\n> |---|\n> | 1⟧ |").Must().Be("| A |\n|---|\n| 1 |");
   [XF] public void a_part_of_one_cell_copies_text() => Markdown("| A⟦B⟧ |\n|---|").Must().Be("B");
   [XF] public void a_part_of_code_copies_the_code() => Markdown("```\na ⟦*b*\nc⟧\n```").Must().Be("*b*\nc");
   [XF] public void the_line_break_before_a_table_on_the_next_line_copies_it() => Markdown("> a⟦\n> | ⟧b | c |\n|---|---|").Must().Be("\n");
   [XF] public void text_and_a_part_of_code_copies_the_code_block() => Markdown("⟦a\n\n```cs\nb⟧c\n```").Must().Be("a\n\n```cs\nbc\n```");
}
