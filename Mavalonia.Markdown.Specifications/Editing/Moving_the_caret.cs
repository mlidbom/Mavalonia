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

   [XF] public void to_the_character_after_it_inside_the_end_of_code_goes_outside_the_code() => To(CaretMovement.ToTheCharacterAfter, "`a‸` b").Must().Be("`a`‸ b");
   [XF] public void to_the_character_after_it_outside_the_end_of_code_goes_past_the_next_character() => To(CaretMovement.ToTheCharacterAfter, "`a`‸ b").Must().Be("`a` ‸b");
   [XF] public void to_the_character_before_it_outside_the_end_of_code_goes_inside_the_code() => To(CaretMovement.ToTheCharacterBefore, "`a`‸ b").Must().Be("`a‸` b");
   [XF] public void to_the_character_before_it_from_after_code_stops_outside_the_code() => To(CaretMovement.ToTheCharacterBefore, "`a` ‸b").Must().Be("`a`‸ b");
   [XF] public void to_the_character_after_it_outside_the_start_of_code_goes_inside_the_code() => To(CaretMovement.ToTheCharacterAfter, "b ‸`a`").Must().Be("b `‸a`");
   [XF] public void to_the_character_after_it_from_before_code_stops_outside_the_code() => To(CaretMovement.ToTheCharacterAfter, "b‸ `a`").Must().Be("b ‸`a`");
   [XF] public void to_the_character_before_it_inside_the_start_of_code_goes_outside_the_code() => To(CaretMovement.ToTheCharacterBefore, "b `‸a`").Must().Be("b ‸`a`");
   [XF] public void to_the_character_before_it_from_inside_code_stops_inside_its_start() => To(CaretMovement.ToTheCharacterBefore, "b `a‸c`").Must().Be("b `‸ac`");
   [XF] public void to_the_character_after_it_inside_the_end_of_code_in_bold_stays_inside_the_bold() => To(CaretMovement.ToTheCharacterAfter, "**`a‸`**").Must().Be("**`a`‸**");
   [XF] public void to_the_character_after_it_outside_code_at_the_end_of_the_text_stays_there() => To(CaretMovement.ToTheCharacterAfter, "`a`‸").Must().Be("`a`‸");
   [XF] public void to_the_character_after_it_outside_code_at_the_end_of_a_paragraph_goes_to_the_next() => To(CaretMovement.ToTheCharacterAfter, "`a`‸\n\nb").Must().Be("`a`\n\n‸b");
   [XF] public void to_the_character_before_it_at_the_start_of_a_paragraph_stops_outside_code_that_ends_the_one_before() => To(CaretMovement.ToTheCharacterBefore, "`a`\n\n‸b").Must().Be("`a`‸\n\nb");
   [XF] public void to_the_character_before_it_outside_code_at_the_start_of_the_text_stays_there() => To(CaretMovement.ToTheCharacterBefore, "‸`a`").Must().Be("‸`a`");
   [XF] public void to_the_word_after_it_goes_past_the_backticks_of_code() => To(CaretMovement.ToTheWordAfter, "‸a `b` c").Must().Be("a ‸`b` c");
   [XF] public void passes_the_backticks_of_code_when_it_extends_what_is_selected() => Of("`a‸` b", it => it.Move(CaretMovement.ToTheCharacterAfter, extendSelection: true)).Must().Be("`a⟦` ⟧b");
}
