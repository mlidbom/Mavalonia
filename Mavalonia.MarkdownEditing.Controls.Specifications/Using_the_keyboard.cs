using Avalonia.Input;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

public class Using_the_keyboard
{
   const RawInputModifiers Ctrl = RawInputModifiers.Control;
   const RawInputModifiers Shift = RawInputModifiers.Shift;
   const RawInputModifiers None = RawInputModifiers.None;

   static string Press(PhysicalKey key, string markdown, RawInputModifiers modifiers = None, int times = 1) => new EditorInAWindow(markdown).Press(key, modifiers, times).Markdown;

   [XFOnUIThread] public void types_text() => new EditorInAWindow("a‸b").Type("x **y**").Markdown.Must().Be("ax **y**‸b");
   [XFOnUIThread] public void starts_a_new_block_with_Enter() => Press(PhysicalKey.Enter, "- a‸").Must().Be("- a\n- ‸");
   [XFOnUIThread] public void breaks_the_line_with_Shift_and_Enter() => Press(PhysicalKey.Enter, "a‸", Shift).Must().Be("a\n‸");

   [XFOnUIThread] public void starts_a_new_block_with_the_keys_that_the_application_chooses()
   {
      var editor = new EditorInAWindow("a‸") { Editor = { StartNewBlockGesture = new KeyGesture(Key.Enter, KeyModifiers.Control) } };
      editor.Press(PhysicalKey.Enter, Ctrl).Markdown.Must().Be("a\n\n‸");
   }

   [XFOnUIThread] public void leaves_keys_to_the_application_that_it_does_not_use()
   {
      var editor = new EditorInAWindow("a‸");
      var received = new List<Key>();
      editor.Editor.KeyDown += (_, key) => received.Add(key.Key);
      editor.Press(PhysicalKey.Enter, Ctrl);
      new { received, editor.Markdown }.Must().Satisfy(it => it.received.Contains(Key.Enter) && it.Markdown == "a‸");
   }

   [XFOnUIThread] public void deletes_the_character_before_the_caret_with_Backspace() => Press(PhysicalKey.Backspace, "ab‸c").Must().Be("a‸c");
   [XFOnUIThread] public void deletes_the_word_before_the_caret_with_Ctrl_and_Backspace() => Press(PhysicalKey.Backspace, "one two‸", Ctrl).Must().Be("one ‸");
   [XFOnUIThread] public void deletes_the_character_after_the_caret_with_Delete() => Press(PhysicalKey.Delete, "a‸bc").Must().Be("a‸c");
   [XFOnUIThread] public void makes_text_bold_with_Ctrl_and_B() => Press(PhysicalKey.B, "⟦a⟧", Ctrl).Must().Be("⟦**a⟧**");
   [XFOnUIThread] public void makes_text_italic_with_Ctrl_and_I() => Press(PhysicalKey.I, "⟦a⟧", Ctrl).Must().Be("⟦*a⟧*");
   [XFOnUIThread] public void makes_text_code_with_Ctrl_and_E() => Press(PhysicalKey.E, "⟦a⟧", Ctrl).Must().Be("⟦`a⟧`");
   [XFOnUIThread] public void strikes_text_through_with_Ctrl_Shift_and_X() => Press(PhysicalKey.X, "⟦a⟧", Ctrl | Shift).Must().Be("⟦~~a⟧~~");
   [XFOnUIThread] public void undoes_with_Ctrl_and_Z() => new EditorInAWindow("a‸").Type("bc").Press(PhysicalKey.Z, Ctrl).Markdown.Must().Be("a‸");
   [XFOnUIThread] public void redoes_with_Ctrl_and_Y() => new EditorInAWindow("a‸").Type("bc").Press(PhysicalKey.Z, Ctrl).Press(PhysicalKey.Y, Ctrl).Markdown.Must().Be("abc‸");
   [XFOnUIThread] public void indents_a_list_item_with_Tab() => Press(PhysicalKey.Tab, "- a\n- b‸").Must().Be("- a\n  - b‸");
   [XFOnUIThread] public void takes_a_list_item_out_with_Shift_and_Tab() => Press(PhysicalKey.Tab, "- a\n  - b‸", Shift).Must().Be("- a\n- b‸");
   [XFOnUIThread] public void goes_to_the_end_of_the_next_cell_with_Tab() => Press(PhysicalKey.Tab, "| A‸ | B |\n|---|---|").Must().Be("| A | B‸ |\n|---|---|");

   [XFOnUIThread] public void moves_the_caret_with_the_arrow_keys() => new EditorInAWindow("a‸b **c**").Press(PhysicalKey.ArrowRight, times: 3).Press(PhysicalKey.ArrowLeft).Markdown.Must().Be("ab ‸**c**");
   [XFOnUIThread] public void selects_with_Shift_and_the_arrow_keys() => Press(PhysicalKey.ArrowRight, "a‸bcd", Shift, times: 2).Must().Be("a⟦bc⟧d");
   [XFOnUIThread] public void goes_to_the_end_of_what_is_selected_with_Right() => Press(PhysicalKey.ArrowRight, "a⟦bc⟧d").Must().Be("abc‸d");
   [XFOnUIThread] public void goes_to_the_start_of_what_is_selected_with_Left() => Press(PhysicalKey.ArrowLeft, "a⟦bc⟧d").Must().Be("a‸bcd");
   [XFOnUIThread] public void goes_to_the_word_after_the_caret_with_Ctrl_and_Right() => Press(PhysicalKey.ArrowRight, "‸one two", Ctrl).Must().Be("one ‸two");
   [XFOnUIThread] public void selects_all_with_Ctrl_and_A() => Press(PhysicalKey.A, "> a‸\n\nb", Ctrl).Must().Be("> ⟦a\n\nb⟧");
   [XFOnUIThread] public void goes_to_the_start_of_the_line_with_Home() => Press(PhysicalKey.Home, "a\nb **c** d‸").Must().Be("a\n‸b **c** d");
   [XFOnUIThread] public void goes_to_the_end_of_the_line_with_End() => Press(PhysicalKey.End, "‸a **b** c\nd").Must().Be("a **b** c‸\nd");
   [XFOnUIThread] public void goes_to_the_start_of_the_document_with_Ctrl_and_Home() => Press(PhysicalKey.Home, "a\n\nb‸", Ctrl).Must().Be("‸a\n\nb");
   [XFOnUIThread] public void goes_to_the_end_of_the_document_with_Ctrl_and_End() => Press(PhysicalKey.End, "‸a\n\nb", Ctrl).Must().Be("a\n\nb‸");

   [XFOnUIThread] public void goes_to_the_line_above_with_Up() => Press(PhysicalKey.ArrowUp, "a\nb‸").Must().Be("a‸\nb");
   [XFOnUIThread] public void goes_to_the_line_below_with_Down() => Press(PhysicalKey.ArrowDown, "a‸\n\n> b").Must().Be("a\n\n> ‸b");
   [XFOnUIThread] public void goes_past_a_shorter_line_to_below_where_it_was_with_Down() => Press(PhysicalKey.ArrowDown, "aaaaaa‸\n\na\n\naaaaaaaa", times: 2).Must().Be("aaaaaa\n\na\n\naaaaaa‸aa");
   [XFOnUIThread] public void goes_to_the_cell_below_with_Down() => Press(PhysicalKey.ArrowDown, "| aa | bb‸ |\n|----|----|\n| cc | dd |").Must().Be("| aa | bb |\n|----|----|\n| cc | dd‸ |");
   [XFOnUIThread] public void goes_to_the_start_of_the_document_with_Up_on_the_first_line() => Press(PhysicalKey.ArrowUp, "ab‸c").Must().Be("‸abc");
   [XFOnUIThread] public void goes_to_the_next_line_of_a_wrapped_paragraph_with_Down() => Press(PhysicalKey.ArrowDown, "‸" + string.Join(' ', Enumerable.Repeat("word", 60))).Must().Satisfy(it => it.IndexOf('‸') > 40 && it.IndexOf('‸') < 200);
}
