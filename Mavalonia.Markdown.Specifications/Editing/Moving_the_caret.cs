using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.Markdown.Editing;
using static Mavalonia.Markdown.Specifications.Editing.Editing;

namespace Mavalonia.Markdown.Specifications.Editing;

public class Moving_the_caret
{
   static string To(CaretMovement movement, string markdown) => Of(markdown, it => it.Move(movement));

   [XF] public void to_the_character_after_it_passes_the_markup_between_them() => To(CaretMovement.ToTheCharacterAfter, "a‸ **b**").Must().Be("a ‸**b**");
   [XF] public void to_the_character_after_the_last_bold_one_stays_inside_the_markup() => To(CaretMovement.ToTheCharacterAfter, "**a‸b** c").Must().Be("**ab‸** c");
   [XF] public void to_the_character_after_it_goes_past_a_whole_emoji() => To(CaretMovement.ToTheCharacterAfter, "‸👩‍👩‍👧b").Must().Be("👩‍👩‍👧‸b");
   [XF] public void to_the_character_after_it_at_the_end_of_a_paragraph_goes_to_the_next() => To(CaretMovement.ToTheCharacterAfter, "a‸\n\n> b").Must().Be("a\n\n> ‸b");
   [XF] public void to_the_character_after_it_at_the_end_of_a_cell_goes_to_the_next() => To(CaretMovement.ToTheCharacterAfter, "| A‸ | B |\n|---|---|").Must().Be("| A | ‸B |\n|---|---|");
   [XF] public void to_the_character_before_it_at_the_start_of_a_paragraph_goes_to_the_end_of_the_one_before() => To(CaretMovement.ToTheCharacterBefore, "a\n\n‸b").Must().Be("a‸\n\nb");
   [XF] public void to_the_word_after_it_goes_to_the_start_of_that_word() => To(CaretMovement.ToTheWordAfter, "o‸ne two").Must().Be("one ‸two");
   [XF] public void to_the_word_before_it_goes_to_the_start_of_that_word() => To(CaretMovement.ToTheWordBefore, "one two‸").Must().Be("one ‸two");
   [XF] public void to_the_start_goes_to_the_start_of_what_is_shown() => To(CaretMovement.ToTheStart, "> a\n\nb‸").Must().Be("> ‸a\n\nb");
   [XF] public void to_the_end_goes_to_the_end_of_what_is_shown() => To(CaretMovement.ToTheEnd, "‸a\n\n- b").Must().Be("a\n\n- b‸");
   [XF] public void can_extend_what_is_selected() => Of("a‸bc", it => it.Move(CaretMovement.ToTheCharacterAfter, extendSelection: true)).Must().Be("a⟦b⟧c");
}
