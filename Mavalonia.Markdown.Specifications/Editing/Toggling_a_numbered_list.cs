using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.Markdown.Rendering.Containers;
using static Mavalonia.Markdown.Specifications.Editing.Editing;

namespace Mavalonia.Markdown.Specifications.Editing;

public class Toggling_a_numbered_list
{
   static string Numbered(string markdown) => Of(markdown, it => it.ToggleList(ListKind.Numbered));

   [XF] public void makes_the_paragraph_at_the_caret_the_first_item_of_a_list() => Numbered("a‸").Must().Be("1. a‸");
   [XF] public void numbers_the_selected_paragraphs_in_order() => Numbered("⟦a\n\nb\n\nc⟧").Must().Be("1. ⟦a\n2. b\n3. c⟧");
   [XF] public void numbers_the_paragraph_after_a_list_as_its_next_item() => Numbered("1. a\n2. b\n\nc‸").Must().Be("1. a\n2. b\n3. c‸");
   [XF] public void takes_the_item_at_the_caret_out_of_its_list() => Numbered("1. a‸").Must().Be("a‸");
   [XF] public void turns_the_whole_bulleted_list_of_the_item_at_the_caret_into_a_numbered_one() => Numbered("- a\n- b‸\n- c").Must().Be("1. a\n2. b‸\n3. c");
   [XF] public void turns_a_bulleted_list_into_a_numbered_one_that_keeps_the_list_inside_its_item() => Numbered("- a‸\n  - b").Must().Be("1. a‸\n   - b");
   [XF] public void turns_a_bulleted_list_into_a_numbered_one_that_keeps_the_paragraphs_of_its_items() => Numbered("- a‸\n\n  b\n- c").Must().Be("1. a‸\n\n   b\n2. c");
   [XF] public void makes_a_paragraph_after_a_bulleted_list_the_first_item_of_another_list() => Numbered("- a\n\nb‸").Must().Be("- a\n\n1. b‸");
}
