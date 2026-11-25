using Compze.Must;
using Compze.xUnitBDD;
using static Mavalonia.MarkdownEditing.Specifications.Editing.Editing;

namespace Mavalonia.MarkdownEditing.Specifications.Editing;

public class Indenting
{
   static string Indent(string markdown) => Of(markdown, it => it.Indent());
   static string Outdent(string markdown) => Of(markdown, it => it.Outdent());

   [XF] public void a_list_item_puts_it_in_a_list_inside_the_one_before_it() => Indent("- a\n- b‸").Must().Be("- a\n  - b‸");
   [XF] public void a_list_item_puts_it_in_the_list_inside_the_one_before_it() => Indent("- a\n  - b\n- c‸").Must().Be("- a\n  - b\n  - c‸");
   [XF] public void a_list_item_takes_the_list_inside_it_along() => Indent("- a\n- b‸\n  - c").Must().Be("- a\n  - b‸\n    - c");
   [XF] public void a_list_item_takes_its_other_lines_along() => Indent("- a\n- b‸\n  c\n\n  d").Must().Be("- a\n  - b‸\n    c\n\n    d");
   [XF] public void a_numbered_list_item_numbers_it_from_1() => Indent("1. a\n2. b‸").Must().Be("1. a\n   1. b‸");
   [XF] public void an_empty_list_item_puts_it_in_a_list_inside_the_one_before_it() => Indent("- a\n- ‸").Must().Be("- a\n\n  - ‸");
   [XF] public void a_list_item_in_a_quote_leaves_it_in_the_quote() => Indent("> - a\n> - b‸").Must().Be("> - a\n>   - b‸");
   [XF] public void the_first_item_of_a_list_is_not_possible() => EditorOf("- a‸").CanIndent.Must().BeFalse();
   [XF] public void a_paragraph_outside_lists_is_not_possible() => EditorOf("a‸").CanIndent.Must().BeFalse();
   [XF] public void less_puts_an_item_of_a_list_in_a_list_in_the_outer_list() => Outdent("- a\n  - b‸").Must().Be("- a\n- b‸");
   [XF] public void less_takes_an_item_out_of_its_list() => Outdent("- a\n- b‸").Must().Be("- a\n\nb‸");
}
