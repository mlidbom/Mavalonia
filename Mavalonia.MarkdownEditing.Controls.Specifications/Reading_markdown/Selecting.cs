using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.Reading_markdown;

public class Selecting
{
   const RawInputModifiers Ctrl = RawInputModifiers.Control;
   const RawInputModifiers Shift = RawInputModifiers.Shift;

   [XFOnUIThread] public void with_the_pointer_selects_what_it_is_dragged_over() => new ViewInAWindow("# one\n\na **two** b").Select("ne", to: "tw").View.SelectedMarkdown.Must().Be("ne\n\na **tw**");

   [XFOnUIThread] public void with_the_pointer_to_the_right_of_a_line_that_is_wrapped_selects_to_the_end_of_that_line()
   {
      var view = new ViewInAWindow(string.Join(' ', Enumerable.Range(0, 60).Select(it => $"word{it}")));
      view.Window.Drag(view.StartOf("word0"), new Point(789, view.StartOf("word0").Y));
      view.View.SelectedMarkdown.Must().Be("word0 word1 word2 word3 word4 word5 word6 word7 word8 word9 word10 word11 word12 word13 word14 word15");
   }

   [XFOnUIThread] public void with_the_pointer_below_the_text_of_a_block_selects_to_the_char_above()
   {
      var view = new ViewInAWindow("```\nabcdef\n```");
      view.Window.Drag(view.StartOf("a"), view.StartOf("c") + new Vector(0, 14));
      view.View.SelectedMarkdown.Must().Be("ab");
   }

   [XFOnUIThread] public void with_the_pointer_selects_a_word_that_is_pressed_twice()
   {
      var view = new ViewInAWindow("a **two** b");
      view.Window.Click(view.CenterOf("two"));
      view.Window.Click(view.CenterOf("two"));
      view.View.SelectedMarkdown.Must().Be("**two**");
   }

   [XFOnUIThread] public void with_the_pointer_selects_to_where_it_is_pressed_with_Shift()
   {
      var view = new ViewInAWindow("one two three");
      view.Window.Click(view.StartOf("two"));
      view.Window.Click(view.EndOf("three"), Shift);
      view.View.SelectedMarkdown.Must().Be("two three");
   }

   [XFOnUIThread] public void with_Ctrl_and_A_selects_all()
   {
      var view = new ViewInAWindow("# one\n\n- two");
      view.Window.Click(view.CenterOf("two"));
      view.Press(PhysicalKey.A, Ctrl).View.SelectedMarkdown.Must().Be("# one\n\n- two");
   }

   [XFOnUIThread] public void with_Shift_and_an_arrow_selects_more() => new ViewInAWindow("one two").Select("one", to: "one").Press(PhysicalKey.ArrowRight, Shift, times: 2).View.SelectedMarkdown.Must().Be("one t");
   [XFOnUIThread] public void ends_with_Escape() => new ViewInAWindow("one two").Select("one", to: "one").Press(PhysicalKey.Escape).View.IsAnythingSelected.Must().BeFalse();
   [XFOnUIThread] public void ends_with_a_click() => new ViewInAWindow("one two").Select("one", to: "one").Click("two").View.IsAnythingSelected.Must().BeFalse();

   [XFOnUIThread] public void with_Shift_and_arrows_goes_on_to_the_other_side_of_where_it_started() => new ViewInAWindow("one two").Select("t", to: "t").Press(PhysicalKey.ArrowLeft, Shift, times: 3).View.SelectedMarkdown.Must().Be("e ");

   [XFOnUIThread] public void is_not_changed_by_dragging_with_the_right_button()
   {
      var view = new ViewInAWindow("one two three").Select("one", to: "one");
      view.Window.DragWithTheRightButton(view.CenterOf("one"), view.CenterOf("three"));
      view.View.SelectedMarkdown.Must().Be("one");
   }

   [XFOnUIThread] public void in_a_view_ends_the_selection_in_another_of_the_window()
   {
      var (one, other) = (new MarkdownView { Markdown = "one" }, new MarkdownView { Markdown = "other" });
      _ = new HeadlessWindow(new StackPanel { Children = { one, other } });
      one.SelectAll();
      other.SelectAll();
      new { One = one.SelectedMarkdown, Other = other.SelectedMarkdown }.Must().Satisfy(it => it.One == "" && it.Other == "other");
   }

   [XFOnUIThread] public void in_a_view_ends_when_another_comes_into_the_window_with_something_selected()
   {
      var (one, other) = (new MarkdownView { Markdown = "one" }, new MarkdownView { Markdown = "other" });
      var panel = new StackPanel { Children = { one } };
      _ = new HeadlessWindow(panel);
      one.SelectAll();
      other.SelectAll();
      panel.Children.Add(other);
      new { One = one.SelectedMarkdown, Other = other.SelectedMarkdown }.Must().Satisfy(it => it.One == "" && it.Other == "other");
   }

   [XFOnUIThread] public async Task goes_on_and_scrolls_while_the_pointer_is_held_below_what_is_seen()
   {
      var view = new ViewInAWindow(string.Join("\n\n", Enumerable.Range(0, 100).Select(it => $"line{it}")));
      view.Window.DragAndHold(view.StartOf("line3"), new Point(300, 640));
      for(var time = 0; time < 100 && view.ScrollViewer.Offset.Y < 300; time++) await Task.Delay(10);

      new { view.ScrollViewer.Offset.Y, view.View.SelectedMarkdown }.Must().Satisfy(it => it.Y >= 300 && it.SelectedMarkdown.Contains("line30"));
   }

   [XFOnUIThread] public void tells_that_something_is_selected_and_that_it_no_longer_is()
   {
      var view = new ViewInAWindow("one two");
      var told = new List<bool>();
      view.View.PropertyChanged += (_, change) =>
      {
         if(change.Property == MarkdownView.IsAnythingSelectedProperty) told.Add(change.GetNewValue<bool>());
      };
      view.Select("one", to: "two").Press(PhysicalKey.Escape);
      told.Must().SequenceEqual([true, false]);
   }
}
