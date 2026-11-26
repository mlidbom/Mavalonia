using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.Markdown.Editing;
using static Mavalonia.Markdown.Specifications.Editing.Editing;

namespace Mavalonia.Markdown.Specifications.Editing;

public class A_change_made_by_another_editor
{
   [XF] public void moves_the_caret_along_with_the_text_it_is_in() => Of("a‸b", it => it.Document.Insert(0, "xyz")).Must().Be("xyza‸b");
   [XF] public void that_leaves_the_caret_inside_an_entity_can_be_followed_by_typing_before_it() => Of("‸&amp;", it => it.Select(TextSelection.CaretAt(2)), it => it.Type("x")).Must().Be("x‸&amp;");
   [XF] public void that_leaves_the_caret_in_a_code_fence_can_be_followed_by_typing_in_the_code() => Of("```\na\n```‸", it => it.Select(TextSelection.CaretAt(8)), it => it.Type("x")).Must().Be("```\nax‸\n```");
   [XF] public void is_rendered() => RenderedAfter("a", it => it.Document.Insert(0, "# ")).Must().Be("a");

   static string RenderedAfter(string markdown, Action<RenderedEditor> edit)
   {
      using var editor = EditorOf(markdown + "‸");
      _ = editor.Rendered;
      edit(editor);
      return editor.Rendered.Blocks.Single().Text.Value;
   }
}
