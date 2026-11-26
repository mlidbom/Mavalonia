using Avalonia;
using Avalonia.Input;
using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.Markdown.Controls.Specifications;

public class Using_the_pointer
{
   const double FarToTheRight = 600;

   [XFOnUIThread] public void puts_the_caret_where_it_is_pressed()
   {
      var editor = new EditorInAWindow("‸one\n\n**two**");
      editor.Window.Click(editor.PointInTheText(FarToTheRight, line: 1));
      editor.Markdown.Must().Be("one\n\n**two‸**");
   }

   [XFOnUIThread] public void puts_the_caret_at_the_end_when_pressed_below_the_text()
   {
      var editor = new EditorInAWindow("‸one\n\ntwo");
      editor.Window.Click(editor.PointInTheText(0, line: 15) + new Vector(FarToTheRight, 0));
      editor.Markdown.Must().Be("one\n\ntwo‸");
   }

   [XFOnUIThread] public void selects_from_the_caret_with_Shift()
   {
      var editor = new EditorInAWindow("o‸ne two");
      editor.Window.Click(editor.PointInTheText(FarToTheRight), RawInputModifiers.Shift);
      editor.Markdown.Must().Be("o⟦ne two⟧");
   }

   [XFOnUIThread] public void selects_what_it_is_dragged_over()
   {
      var editor = new EditorInAWindow("‸one\n\n**two**");
      editor.Window.Drag(editor.PointInTheText(0), editor.PointInTheText(FarToTheRight, line: 1));
      editor.Markdown.Must().Be("⟦one\n\n**two⟧**");
   }

   [XFOnUIThread] public void selects_the_word_it_is_pressed_on_twice()
   {
      var editor = new EditorInAWindow("‸one two three");
      var point = editor.PointInTheText(40);
      editor.Window.Click(point);
      editor.Window.Click(point);
      editor.Markdown.Must().Be("one ⟦two⟧ three");
   }

   [XFOnUIThread] public void selects_the_block_it_is_pressed_on_three_times()
   {
      var editor = new EditorInAWindow("‸one two three\n\nfour");
      var point = editor.PointInTheText(40);
      for(var time = 0; time < 3; time++) editor.Window.Click(point);
      editor.Markdown.Must().Be("⟦one two three⟧\n\nfour");
   }

   [XFOnUIThread] public void shows_what_is_selected()
   {
      var editor = new EditorInAWindow("‸one\n\n| two |\n|---|\n| three |");
      var before = editor.Window.RenderFrame();
      editor.Press(PhysicalKey.A, RawInputModifiers.Control);
      var after = editor.Window.RenderFrame();
      after.SaveForInspectionAs("All selected");
      after.Fingerprint.Must().NotBe(before.Fingerprint);
   }
}
