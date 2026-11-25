using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.MarkdownEditing.Editing;
using static Mavalonia.MarkdownEditing.Specifications.Editing.Editing;

namespace Mavalonia.MarkdownEditing.Specifications.Editing;

public class A_change_made_by_another_editor
{
   [XF] public void moves_the_caret_along_with_the_text_it_is_in() => Of("a‸b", it => it.Document.Insert(0, "xyz")).Must().Be("xyza‸b");
   [XF] public void is_rendered() => RenderedAfter("a", it => it.Document.Insert(0, "# ")).Must().Be("a");

   static string RenderedAfter(string markdown, Action<RenderedEditor> edit)
   {
      using var editor = EditorOf(markdown + "‸");
      _ = editor.Rendered;
      edit(editor);
      return editor.Rendered.Blocks.Single().Text.Value;
   }
}
