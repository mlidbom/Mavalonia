using Avalonia;
using Avalonia.Input;
using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.Markdown.Controls.Specifications;

///<summary>
/// The caret where inline code ends or starts: it can be inside the code there or outside it, which decides whether what is typed is code.<br/>
/// The arrow keys move it from one to the other, and it's drawn on the side it's on: over the edge of the code's background when it's inside, beside it when it's outside.
///</summary>
public class The_caret_at_an_edge_of_inline_code
{
   [XFOnUIThread] public void is_moved_out_of_the_end_of_the_code_by_the_right_arrow_key() =>
      new EditorInAWindow("`code‸` b").Press(PhysicalKey.ArrowRight).Markdown.Must().Be("`code`‸ b");

   [XFOnUIThread] public void moved_back_into_the_end_of_the_code_and_out_again_lets_what_is_typed_after_the_code_be_outside_it() =>
      new EditorInAWindow("‸").Type("`c`").Press(PhysicalKey.ArrowLeft).Type("ode").Press(PhysicalKey.ArrowRight).Type(" more").Markdown.Must().Be("`code` more‸");

   [XFOnUIThread] public void inside_the_end_of_the_code_is_drawn_left_of_where_it_is_drawn_outside_it() =>
      new { Inside = WhereTheCaretIsDrawnIn("`code‸` b"), Outside = WhereTheCaretIsDrawnIn("`code`‸ b") }.Must().Satisfy(it => it.Inside.Right <= it.Outside.Left);

   [XFOnUIThread] public void outside_the_start_of_the_code_is_drawn_left_of_where_it_is_drawn_inside_it() =>
      new { Outside = WhereTheCaretIsDrawnIn("a ‸`code`"), Inside = WhereTheCaretIsDrawnIn("a `‸code`") }.Must().Satisfy(it => it.Outside.Right <= it.Inside.Left);

   static Rect WhereTheCaretIsDrawnIn(string markdown) => new EditorInAWindow(markdown).InputMethodClient.CursorRectangle;
}
