using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.Markdown.Rendering.Blocks;
using static Mavalonia.Markdown.Specifications.Editing.Editing;

namespace Mavalonia.Markdown.Specifications.Editing;

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
   [XF] public void blocks_at_the_start_of_a_heading_puts_them_before_it() => Paste("- x", into: "# ‸a").Must().Be("- x‸\n\n# a");
   [XF] public void blocks_in_the_middle_of_bold_text_leaves_both_halves_bold() => Paste("- x", into: "**a‸b**").Must().Be("**a**\n\n- x‸\n\n**b**");

   [XF] public void a_quote_anywhere_in_a_paragraph_or_a_heading_adds_a_quote_with_nothing_else_in_it_and_puts_the_caret_in_it_and_keeps_how_all_other_text_looks()
   {
      foreach(var insertion in InsertionOfAQuote.AtEachOffset(it => it is Paragraph or Heading, it => it.Paste("> " + InsertionOfAQuote.TextOfTheQuote)))
         insertion.Must().Satisfy(it => it.CountOfQuotes == 1 && it.IsTheCaretInTheQuote && it.OtherTextAfterwards == it.OtherTextBefore);
   }
}
