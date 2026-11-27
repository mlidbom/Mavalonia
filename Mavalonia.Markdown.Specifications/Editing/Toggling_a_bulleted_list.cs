using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.Markdown.Rendering.Containers;
using static Mavalonia.Markdown.Specifications.Editing.Editing;

namespace Mavalonia.Markdown.Specifications.Editing;

public class Toggling_a_bulleted_list
{
   static string Bulleted(string markdown) => Of(markdown, it => it.ToggleList(ListKind.Bulleted));

   [XF] public void makes_the_paragraph_at_the_caret_an_item_of_a_list() => Bulleted("a‸").Must().Be("- a‸");
   [XF] public void makes_the_heading_at_the_caret_an_item_of_a_list() => Bulleted("# a‸").Must().Be("- # a‸");
   [XF] public void makes_a_paragraph_in_a_quote_an_item_of_a_list_in_the_quote() => Bulleted("> a‸").Must().Be("> - a‸");
   [XF] public void makes_each_selected_paragraph_an_item_of_one_list() => Bulleted("⟦a\n\nb⟧").Must().Be("- ⟦a\n- b⟧");
   [XF] public void makes_the_paragraph_after_a_list_its_next_item() => Bulleted("- a\n\nb‸").Must().Be("- a\n- b‸");
   [XF] public void makes_the_paragraph_before_a_list_its_first_item() => Bulleted("a‸\n\n- b").Must().Be("- a‸\n- b");
   [XF] public void makes_the_selected_paragraphs_items_when_some_of_them_are() => Bulleted("⟦a\n\n- b⟧").Must().Be("- ⟦a\n- b⟧");
   [XF] public void makes_a_paragraph_after_the_first_of_a_list_item_the_first_item_of_a_list_inside_the_item() => Bulleted("- a\n\n  b‸").Must().Be("- a\n  - b‸");
   [XF] public void takes_the_item_at_the_caret_out_of_its_list() => Bulleted("- a‸").Must().Be("a‸");
   [XF] public void takes_an_item_in_the_middle_of_a_list_out_of_it() => Bulleted("- a\n- b‸\n- c").Must().Be("- a\n\nb‸\n\n- c");
   [XF] public void takes_an_item_of_a_list_inside_another_item_into_the_list_of_that_item() => Bulleted("- a\n  - b‸").Must().Be("- a\n- b‸");
   [XF] public void takes_each_selected_item_out_of_its_list() => Bulleted("- ⟦a\n- b⟧").Must().Be("⟦a\n\nb⟧");
   [XF] public void turns_a_numbered_list_into_a_bulleted_one() => Bulleted("1. a‸\n2. b").Must().Be("- a‸\n- b");
   [XF] public void leaves_code_blocks_alone() => Bulleted("```\na‸\n```").Must().Be("```\na‸\n```");
   [XF] public void is_undone_as_one() => Of("⟦a\n\nb⟧", it => it.ToggleList(ListKind.Bulleted), it => it.Undo()).Must().Satisfy(it => it.Replace("‸", "") == "a\n\nb");
}
