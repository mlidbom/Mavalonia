using Compze.Must;
using Compze.xUnitBDD;
using static Mavalonia.Markdown.Specifications.Editing.Editing;

namespace Mavalonia.Markdown.Specifications.Editing.Deleting_text;

public class after_the_caret
{
   static string From(string markdown) => Of(markdown, it => it.DeleteCharacterAfter());

   [XF] public void deletes_the_character_after_it() => From("a‸bc").Must().Be("a‸c");
   [XF] public void deletes_an_emoji_of_several_chars_as_one() => From("a‸👩‍👩‍👧b").Must().Be("a‸b");
   [XF] public void joins_two_paragraphs() => From("a‸\n\nb").Must().Be("a‸b");
   [XF] public void joins_an_item_of_a_list_with_the_next() => From("- a‸\n- b").Must().Be("- a‸b");
   [XF] public void deletes_an_empty_paragraph() => From("a‸\n\n").Must().Be("a‸");
   [XF] public void at_the_end_of_the_document_changes_nothing() => From("a‸").Must().Be("a‸");
}
