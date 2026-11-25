using Compze.Must;
using Compze.xUnitBDD;
using static Mavalonia.MarkdownEditing.Specifications.Editing.Editing;

namespace Mavalonia.MarkdownEditing.Specifications.Editing.Deleting_text;

public class by_the_word
{
   [XF] public void deletes_the_word_before_the_caret() => Of("one two‸ three", it => it.DeleteWordBefore()).Must().Be("one ‸ three");
   [XF] public void deletes_the_word_and_the_space_before_the_caret() => Of("one two ‸three", it => it.DeleteWordBefore()).Must().Be("one ‸three");
   [XF] public void deletes_the_word_after_the_caret_and_the_space_after_it() => Of("one ‸two three", it => it.DeleteWordAfter()).Must().Be("one ‸three");
   [XF] public void deletes_a_bold_word_with_its_markup() => Of("one **two‸** three", it => it.DeleteWordBefore()).Must().Be("one ‸ three");
}
