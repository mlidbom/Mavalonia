using Compze.Must;
using Compze.xUnitBDD;
using static Mavalonia.MarkdownEditing.Specifications.Editing.Editing;

namespace Mavalonia.MarkdownEditing.Specifications.Editing;

public class Pasting
{
   static string Paste(string markdown, string into) => Of(into, it => it.Paste(markdown));

   [XF] public void text_inserts_it() => Paste("x", into: "a‸b").Must().Be("ax‸b");
   [XF] public void text_replaces_what_is_selected() => Paste("x", into: "a⟦b⟧c").Must().Be("ax‸c");
   [XF] public void markdown_formats_it() => Paste("**x**", into: "a ‸b").Must().Be("a **x**‸b");
   [XF] public void several_lines_into_a_quote_puts_them_all_in_the_quote() => Paste("x\ny", into: "> a ‸").Must().Be("> a x\n> y‸");
   [XF] public void several_paragraphs_into_an_empty_paragraph_of_a_quote_puts_them_all_in_the_quote() => Paste("x\n\ny", into: "> a\n>\n> ‸").Must().Be("> a\n>\n> x\n>\n> y‸");
   [XF] public void a_list_into_an_empty_paragraph_adds_the_list() => Paste("- x\n- y", into: "a\n\n‸").Must().Be("a\n\n- x\n- y‸");
   [XF] public void a_list_in_the_middle_of_a_paragraph_puts_it_between_the_two_halves() => Paste("- x\n- y", into: "a‸b").Must().Be("a\n\n- x\n- y‸\n\nb");
   [XF] public void a_table_into_a_quote_puts_it_in_the_quote() => Paste("| A |\n|---|\n| 1 |", into: "> a‸").Must().Be("> a\n>\n> | A |\n> |---|\n> | 1 |‸");
   [XF] public void several_lines_into_code_in_a_quote_puts_them_all_in_the_code() => Paste("x\n- y", into: "> ```\n> ‸\n> ```").Must().Be("> ```\n> x\n> - y‸\n> ```");
   [XF] public void several_lines_into_a_cell_puts_them_on_one_line() => Paste("x\ny|z", into: "| ‸ |\n|---|").Must().Be("| x y\\|z‸ |\n|---|");
}
