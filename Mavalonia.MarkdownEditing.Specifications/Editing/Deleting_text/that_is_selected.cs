using Compze.Must;
using Compze.xUnitBDD;
using static Mavalonia.MarkdownEditing.Specifications.Editing.Editing;

namespace Mavalonia.MarkdownEditing.Specifications.Editing.Deleting_text;

public class that_is_selected
{
   static string From(string markdown) => Of(markdown, it => it.DeleteCharacterBefore());

   [XF] public void deletes_what_is_selected_in_a_paragraph() => From("a⟦b⟧c").Must().Be("a‸c");
   [XF] public void deletes_bold_text_with_its_markup() => From("a **⟦b⟧** c").Must().Be("a ‸ c");
   [XF] public void leaves_the_rest_of_the_bold_text_bold() => From("a⟦ **b⟧c**").Must().Be("a‸**c**");
   [XF] public void joins_what_is_left_of_two_paragraphs() => From("a⟦b\n\nc⟧d").Must().Be("a‸d");
   [XF] public void joins_what_is_left_of_two_paragraphs_with_others_between_them() => From("a⟦b\n\nx\n\n- y\n\nc⟧d").Must().Be("a‸d");
   [XF] public void joins_what_is_left_of_two_list_items() => From("- a⟦b\n- c⟧d").Must().Be("- a‸d");
   [XF] public void deletes_all_of_a_document() => Of("a‸\n\n> b\n\n- c", it => it.SelectAll(), it => it.DeleteCharacterBefore()).Must().Be("‸");
   [XF] public void deletes_the_table_between_two_paragraphs() => From("a⟦b\n\n| A |\n|---|\n\nc⟧d").Must().Be("a‸d");
   [XF] public void deletes_the_text_of_the_cells_of_a_table() => From("| ⟦A | B⟧ |\n|---|---|\n| 1 | 2 |").Must().Be("| ‸ |  |\n|---|---|\n| 1 | 2 |");
   [XF] public void deletes_a_table_whose_cells_are_all_selected() => From("a\n\n| ⟦A |\n|---|\n| 1⟧ |").Must().Be("a‸");
   [XF] public void deletes_a_table_up_to_whose_end_is_selected() => From("a⟦b\n\n| A |\n|---|\n| 1⟧ |").Must().Be("a‸");
   [XF] public void deletes_the_code_between_two_paragraphs() => From("a⟦b\n\n```\nx\n```\n\nc⟧d").Must().Be("a‸d");
   [XF] public void deletes_a_part_of_code_and_leaves_it_code() => From("a⟦b\n\n```\nxy⟧z\n```").Must().Be("a‸\n\n```\nz\n```");
}
