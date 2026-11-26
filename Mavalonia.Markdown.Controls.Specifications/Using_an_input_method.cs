using Avalonia;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.Markdown.Controls.Specifications;

///<summary>Asks the rendered view where its caret is as soon as it tells that it has moved, as Avalonia does on a desktop for an input method. The headless platform has none.</summary>
public class Using_an_input_method
{
   readonly EditorInAWindow _editor = new("a‸b\n\nc");
   readonly List<Rect> _placesOfTheCaret = [];

   public Using_an_input_method()
   {
      var request = new TextInputMethodClientRequestedEventArgs { RoutedEvent = InputElement.TextInputMethodClientRequestedEvent };
      _editor.RenderedView.RaiseEvent(request);
      var client = request.Client!;
      client.CursorRectangleChanged += (_, _) => _placesOfTheCaret.Add(client.CursorRectangle);
   }

   [XFOnUIThread] public void tells_it_where_the_caret_is_after_each_character_that_is_typed()
   {
      _editor.Type("xyz");
      _placesOfTheCaret.Select(it => it.X).ToList().Must().Satisfy(it => it.Count == 3 && it.Order().SequenceEqual(it) && it.Distinct().Count() == 3);
   }

   [XFOnUIThread] public void tells_it_that_the_caret_is_further_down_after_a_new_block_is_started()
   {
      _editor.Type("x").Press(PhysicalKey.Enter);
      _placesOfTheCaret.Select(it => it.Y).ToList().Must().Satisfy(it => it.Last() > it.First());
   }

   [XFOnUIThread] public void tells_it_where_the_caret_is_after_what_is_selected_across_blocks_is_replaced_and_that_is_undone()
   {
      _editor.Press(PhysicalKey.A, RawInputModifiers.Control).Type("x").Press(PhysicalKey.Z, RawInputModifiers.Control);
      _editor.Markdown.Must().Satisfy(it => it.Replace("‸", "") == "ab\n\nc");
   }

   [XFOnUIThread] public void tells_it_where_the_caret_is_after_the_document_is_changed_by_others()
   {
      _editor.Editor.Document.Insert(0, "# x\n\n");
      _placesOfTheCaret.Must().HaveCount(1);
   }
}
