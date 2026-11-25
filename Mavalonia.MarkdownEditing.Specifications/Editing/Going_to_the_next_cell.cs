using Compze.Must;
using Compze.xUnitBDD;
using static Mavalonia.MarkdownEditing.Specifications.Editing.Editing;

namespace Mavalonia.MarkdownEditing.Specifications.Editing;

public class Going_to_the_next_cell
{
   static string From(string markdown) => Of(markdown, it => it.GoToTheNextCell());

   [XF] public void goes_to_the_end_of_its_text() => From("| a‸ | b |\n|---|---|").Must().Be("| a | b‸ |\n|---|---|");
   [XF] public void from_the_last_of_a_row_goes_to_the_first_of_the_next_row() => From("| a | b‸ |\n|---|---|\n| 1 | 2 |").Must().Be("| a | b |\n|---|---|\n| 1‸ | 2 |");
   [XF] public void from_the_last_of_the_table_adds_a_row_and_goes_to_the_first_of_it() => From("| a | b |\n|---|---|\n| 1 | 2‸ |").Must().Be("| a | b |\n|---|---|\n| 1 | 2 |\n| ‸ |  |");
   [XF] public void from_the_last_of_a_table_with_only_a_header_adds_a_row() => From("| a | b‸ |\n|---|---|").Must().Be("| a | b |\n|---|---|\n| ‸ |  |");
   [XF] public void from_the_last_of_a_table_in_a_quote_adds_the_row_in_the_quote() => From("> | a | b‸ |\n> |---|---|\n\nc").Must().Be("> | a | b |\n> |---|---|\n> | ‸ |  |\n\nc");
   [XF] public void from_the_last_of_a_row_with_too_few_cells_adds_a_row_with_all_of_them() => From("| a | b |\n|---|---|\n| 1‸ |").Must().Be("| a | b |\n|---|---|\n| 1 |\n| ‸ |  |");
   [XF] public void that_adds_a_row_is_undone_at_once() => Of("| a‸ |\n|---|", it => it.GoToTheNextCell(), it => it.Undo()).Must().Satisfy(it => it.Replace("‸", "") == "| a |\n|---|");
}
