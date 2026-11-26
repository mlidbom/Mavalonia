using Compze.Must;
using Compze.xUnitBDD;
using static Mavalonia.Markdown.Specifications.Editing.Editing;

namespace Mavalonia.Markdown.Specifications.Editing;

public class Quoting
{
   static string Quote(string markdown, string into) => Of(into, it => it.Quote(markdown));

   [XF] public void in_an_empty_document_adds_the_quote_and_puts_the_caret_in_a_paragraph_after_it() => Quote("x", into: "‸").Must().Be("> x\n\n‸");
   [XF] public void several_blocks_puts_them_all_in_the_quote() => Quote("x\n\n- y\n", into: "‸").Must().Be("> x\n>\n> - y\n\n‸");
   [XF] public void lines_that_end_as_in_Windows_puts_them_all_in_the_quote() => Quote("x\r\ny", into: "‸").Must().Be("> x\n> y\n\n‸");
   [XF] public void a_table_puts_it_in_the_quote() => Quote("| A |\n|---|\n| 1 |", into: "‸").Must().Be("> | A |\n> |---|\n> | 1 |\n\n‸");
   [XF] public void again_adds_another_quote_after_the_first() => Of("‸", it => it.Quote("x"), it => it.Quote("y")).Must().Be("> x\n\n> y\n\n‸");
   [XF] public void and_then_typing_writes_after_the_quote() => Of("‸", it => it.Quote("x"), Type("y")).Must().Be("> x\n\ny‸");
   [XF] public void at_the_end_of_a_paragraph_adds_the_quote_after_it() => Quote("x", into: "a‸").Must().Be("a\n\n> x\n\n‸");
   [XF] public void at_the_end_of_a_paragraph_before_another_adds_a_paragraph_between_the_quote_and_the_other() => Of("a‸\n\nb", it => it.Quote("x"), Type("y")).Must().Be("a\n\n> x\n\ny‸\n\nb");
   [XF] public void in_the_middle_of_a_paragraph_puts_the_quote_between_the_halves_and_the_caret_in_the_second() => Quote("x", into: "a‸b").Must().Be("a\n\n> x\n\n‸b");
   [XF] public void replaces_what_is_selected() => Quote("x", into: "a⟦b⟧c").Must().Be("a\n\n> x\n\n‸c");
   [XF] public void in_a_list_item_puts_the_quote_in_the_item() => Quote("x", into: "- a‸\n- b").Must().Be("- a\n\n  > x\n\n  ‸\n\n- b");
   [XF] public void in_code_adds_the_quote_after_the_code() => Quote("x", into: "```\na‸b\n```").Must().Be("```\nab\n```\n\n> x\n\n‸");
   [XF] public void in_a_table_adds_the_quote_after_the_table() => Quote("x", into: "| a‸ |\n|---|\n| b |").Must().Be("| a |\n|---|\n| b |\n\n> x\n\n‸");
   [XF] public void is_undone_at_once() => Of("a⟦b⟧c", it => it.Quote("x"), it => it.Undo()).Must().Satisfy(it => it.Replace("‸", "") == "abc");

   [XF] public void anywhere_adds_a_quote_with_nothing_else_in_it_and_puts_the_caret_outside_of_it_and_keeps_how_all_other_text_looks()
   {
      foreach(var insertion in InsertionOfAQuote.AtEachOffset(_ => true, it => it.Quote(InsertionOfAQuote.TextOfTheQuote)))
         insertion.Must().Satisfy(it => it.CountOfQuotes == 1 && !it.IsTheCaretInTheQuote && it.OtherTextAfterwards == it.OtherTextBefore);
   }
}
