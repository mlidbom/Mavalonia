using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.Markdown.Editing;
using static Mavalonia.Markdown.Specifications.Editing.Editing;

namespace Mavalonia.Markdown.Specifications.Editing;

public class Undoing
{
   [XF] public void undoes_all_that_was_typed_without_a_break() => Of("a‸", Type("bcd"), it => it.Undo()).Must().Be("a‸");
   [XF] public void undoes_only_what_was_typed_after_the_caret_was_moved() => Of("a‸", Type("b"), it => it.Move(CaretMovement.ToTheStart), Type("c"), it => it.Undo()).Must().Be("‸ab");
   [XF] public void undoes_a_new_block() => Of("a‸", it => it.StartNewBlock(), it => it.Undo()).Must().Be("a‸");
   [XF] public void can_be_redone() => Of("a‸", Type("b"), it => it.Undo(), it => it.Redo()).Must().Be("ab‸");
   [XF] public void undoes_the_deletion_of_what_is_selected_across_blocks_as_one() => Of("a⟦b\n\n```\nx\n```\n\nc⟧d", it => it.DeleteCharacterBefore(), it => it.Undo()).Must().Satisfy(it => it.Replace("‸", "") == "ab\n\n```\nx\n```\n\ncd");
}
