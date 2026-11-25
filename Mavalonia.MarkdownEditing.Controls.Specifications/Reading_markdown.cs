using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.MarkdownEditing.Html;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

public class Reading_markdown
{
   const RawInputModifiers Ctrl = RawInputModifiers.Control;
   const RawInputModifiers Shift = RawInputModifiers.Shift;

   public class Selecting : Reading_markdown
   {
      [XFOnUIThread] public void with_the_pointer_selects_what_it_is_dragged_over() => new ViewInAWindow("# one\n\na **two** b").Select("ne", to: "tw").View.SelectedMarkdown.Must().Be("ne\n\na **tw**");

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
      [XFOnUIThread] public void ends_with_a_click() => Clicked(new ViewInAWindow("one two").Select("one", to: "one"), "two").View.IsAnythingSelected.Must().BeFalse();

      [XFOnUIThread] public void in_a_view_ends_the_selection_in_another_of_the_window()
      {
         var (one, other) = (new MarkdownView { Markdown = "one" }, new MarkdownView { Markdown = "other" });
         _ = new HeadlessWindow(new StackPanel { Children = { one, other } });
         one.SelectAll();
         other.SelectAll();
         new { One = one.SelectedMarkdown, Other = other.SelectedMarkdown }.Must().Satisfy(it => it.One == "" && it.Other == "other");
      }

      [XFOnUIThread] public async Task goes_on_and_scrolls_while_the_pointer_is_held_below_what_is_seen()
      {
         var view = new ViewInAWindow(string.Join("\n\n", Enumerable.Range(0, 100).Select(it => $"line{it}")));
         view.Window.DragAndHold(view.StartOf("line3"), new Point(300, 640));
         for(var time = 0; time < 100 && view.ScrollViewer.Offset.Y < 300; time++) await Task.Delay(10);

         new { view.ScrollViewer.Offset.Y, view.View.SelectedMarkdown }.Must().Satisfy(it => it.Y >= 300 && it.SelectedMarkdown.Contains("line30"));
      }
   }

   public class Copying_with_Ctrl_and_C : Reading_markdown
   {
      static readonly DataFormat<byte[]> HtmlFormat = DataFormat.CreateBytesPlatformFormat("HTML Format");

      [XFOnUIThread] public async Task copies_the_markdown_of_what_is_selected() =>
         (await CopiedFrom(new ViewInAWindow("- a **two** b").Select("a", to: "two")).TryGetTextAsync()).Must().Be("a **two**");

      [XFOnUIThread] public async Task copies_rich_text_too() =>
         CopiedHtml.ParseHtmlFormatOfWindows((await CopiedFrom(new ViewInAWindow("a **two** b").Select("a", to: "two")).TryGetValueAsync(HtmlFormat))!).Markdown.Must().Be("a **two**");

      [XFOnUIThread] public async Task copies_code_as_text_only() =>
         new { Text = await CopiedFrom(Code()).TryGetTextAsync(), Html = await CopiedFrom(Code()).TryGetValueAsync(HtmlFormat) }.Must().Satisfy(it => it.Text == "two" && it.Html == null);

      [XFOnUIThread] public async Task and_pasting_in_an_editor_pastes_the_markdown_as_it_is_written()
      {
         var copied = await CopiedFrom(new ViewInAWindow("__a__ *  b\n\n*  c").Select("a", to: "c")).TryGetDataAsync();
         var editor = new EditorInAWindow("‸");
         await editor.Window.Clipboard.SetDataAsync(copied);
         editor.Press(PhysicalKey.V, Ctrl);
         await Task.Yield();
         editor.Markdown.Must().Be("__a__ *  b\n\n*  c‸");
      }

      static ViewInAWindow Code() => new ViewInAWindow("```\none two\n```").Select("two", to: "two");

      static IClipboard CopiedFrom(ViewInAWindow view) => view.Press(PhysicalKey.C, Ctrl).Window.Clipboard;
   }

   public class A_link : Reading_markdown
   {
      [XFOnUIThread] public void tells_that_it_is_clicked() => Clicked(new ViewInAWindow("a [link](https://b.c) d"), "link").ClickedLinks.Must().SequenceEqual(["https://b.c"]);
      [XFOnUIThread] public void that_is_a_bare_URL_tells_that_it_is_clicked() => Clicked(new ViewInAWindow("a https://b.c d"), "b.c").ClickedLinks.Must().SequenceEqual(["https://b.c"]);
      [XFOnUIThread] public void tells_nothing_when_the_text_beside_it_is_clicked() => Clicked(new ViewInAWindow("[link](https://b.c) other"), "other").ClickedLinks.Must().BeEmpty();
      [XFOnUIThread] public void tells_nothing_when_text_is_selected_from_it() => new ViewInAWindow("[link](https://b.c) other").Select("ink", to: "other").ClickedLinks.Must().BeEmpty();
      [XFOnUIThread] public void has_the_pointer_look_like_a_hand() => CursorOver("link").Must().Be("Hand");
      [XFOnUIThread] public void has_the_pointer_look_as_over_text_beside_it() => CursorOver("other").Must().Be("Ibeam");

      static string CursorOver(string text)
      {
         var view = new ViewInAWindow("[link](https://b.c) other");
         view.Window.MoveThePointerTo(view.CenterOf(text));
         return view.View.Cursor!.ToString()!;
      }
   }

   public class The_keys : Reading_markdown
   {
      [XFOnUIThread] public void that_select_nothing_are_left_to_others()
      {
         var view = new ViewInAWindow("one two");
         var left = new List<Key>();
         view.ScrollViewer.KeyDown += (_, press) => left.Add(press.Key);
         view.Window.Click(view.CenterOf("one"));
         view.Press(PhysicalKey.ArrowDown).Press(PhysicalKey.Escape).Press(PhysicalKey.C, Ctrl);
         left.Must().Satisfy(it => it.Contains(Key.Down) && it.Contains(Key.Escape) && it.Contains(Key.C));
      }
   }

   public class Setting_the_markdown_again : Reading_markdown
   {
      [XFOnUIThread] public void leaves_what_is_selected_selected_when_the_text_before_is_the_same()
      {
         var view = new ViewInAWindow("one two").Select("two", to: "two");
         view.View.Markdown = "one two three";
         view.View.SelectedMarkdown.Must().Be("two");
      }

      [XFOnUIThread] public void makes_the_view_higher_when_there_is_more_to_show()
      {
         var view = new ViewInAWindow("one");
         var before = view.View.DesiredSize.Height;
         view.View.Markdown = "one\n\ntwo";
         view.Window.RenderFrame();
         new { Before = before, After = view.View.DesiredSize.Height }.Must().Satisfy(it => it.After > it.Before + 10);
      }

      [XFOnUIThread] public void in_the_dialect_of_plain_text_shows_it_as_it_is_written()
      {
         var view = new ViewInAWindow("**a**");
         view.View.Dialect = MarkdownDialect.PlainText;
         view.View.PlainText.Value.Must().Be("**a**");
      }
   }

   [XFOnUIThread] public void does_not_scroll_when_a_view_that_is_higher_than_the_window_is_clicked()
   {
      var view = new ViewInAWindow(string.Join("\n\n", Enumerable.Range(0, 100).Select(it => $"line{it}")));
      view.ScrollViewer.Offset = new Vector(0, 500);
      view.Window.RenderFrame();
      view.Window.Click(new Point(100, 300));
      view.ScrollViewer.Offset.Must().Be(new Vector(0, 500));
   }

   static ViewInAWindow Clicked(ViewInAWindow view, string text)
   {
      view.Window.Click(view.CenterOf(text));
      return view;
   }
}
