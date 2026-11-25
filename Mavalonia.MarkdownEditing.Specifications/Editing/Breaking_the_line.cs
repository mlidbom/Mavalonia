using Compze.Must;
using Compze.xUnitBDD;
using static Mavalonia.MarkdownEditing.Specifications.Editing.Editing;

namespace Mavalonia.MarkdownEditing.Specifications.Editing;

public class Breaking_the_line
{
   static string BreakingTheLine(string markdown) => Of(markdown, it => it.BreakLine());

   [XF] public void adds_a_line_break_to_a_paragraph() => BreakingTheLine("a‸b").Must().Be("a\n‸b");
   [XF] public void adds_a_line_break_at_the_end_of_a_paragraph() => BreakingTheLine("a‸").Must().Be("a\n‸");
   [XF] public void adds_a_line_break_inside_bold_text() => BreakingTheLine("**a‸b**").Must().Be("**a\n‸b**");
   [XF] public void adds_a_line_break_in_a_list_item() => BreakingTheLine("- a‸").Must().Be("- a\n  ‸");
   [XF] public void adds_a_line_break_in_a_quote() => BreakingTheLine("> a‸").Must().Be("> a\n> ‸");
   [XF] public void adds_a_line_break_to_code() => BreakingTheLine("```\na‸\n```").Must().Be("```\na\n‸\n```");
   [XF] public void starts_a_new_block_in_a_heading() => BreakingTheLine("# a‸").Must().Be("# a\n\n‸");
   [XF] public void changes_nothing_in_a_cell() => BreakingTheLine("| A‸ |\n|---|").Must().Be("| A‸ |\n|---|");
}
