using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.MarkdownEditing.Html;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Texts;

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
         new { Text = await CopiedFrom(SelectedCode()).TryGetTextAsync(), Html = await CopiedFrom(SelectedCode()).TryGetValueAsync(HtmlFormat) }.Must().Satisfy(it => it.Text == "two" && it.Html == null);

      [XFOnUIThread] public async Task and_pasting_in_an_editor_pastes_the_markdown_as_it_is_written()
      {
         var copied = await CopiedFrom(new ViewInAWindow("__a__ *  b\n\n*  c").Select("a", to: "c")).TryGetDataAsync();
         var editor = new EditorInAWindow("‸");
         await editor.Window.Clipboard.SetDataAsync(copied);
         editor.Press(PhysicalKey.V, Ctrl);
         await Task.Yield();
         editor.Markdown.Must().Be("__a__ *  b\n\n*  c‸");
      }

      static ViewInAWindow SelectedCode() => new ViewInAWindow("```\none two\n```").Select("two", to: "two");

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

   public class Code : Reading_markdown
   {
      const string Markdown = "text\n\n```csharp\nvar one = 1;\n```\n\n```\ntwo\n```";

      readonly ViewInAWindow _view = new(Markdown);

      Button? CopyButton => _view.View.GetVisualDescendants().OfType<Button>().SingleOrDefault(it => it.IsVisible);

      [XFOnUIThread] public void has_no_copy_button_while_the_pointer_is_elsewhere() => CopyButtonAt("text").Must().BeNull();
      [XFOnUIThread] public void has_a_copy_button_that_says_its_language_while_the_pointer_is_over_it() => CopyButtonAt("one")!.Content.Must().Be("csharp · Copy");
      [XFOnUIThread] public void without_a_language_has_a_copy_button_while_the_pointer_is_over_it() => CopyButtonAt("two")!.Content.Must().Be("Copy");
      [XFOnUIThread] public void has_the_copy_button_at_its_top_right() => CenterOf(CopyButtonAt("one")!).Must().Satisfy(it => it.X > 700 && Math.Abs(it.Y - _view.CenterOf("one").Y) < 15);

      [XFOnUIThread] public async Task is_copied_as_text_with_the_copy_button()
      {
         _view.Window.Click(CenterOf(CopyButtonAt("one")!));
         new { Text = await _view.Window.Clipboard.TryGetTextAsync(), CopyButton!.Content }.Must().Satisfy(it => it.Text == "var one = 1;" && (string?)it.Content == "Copied");
      }

      [XFOnUIThread] public async Task in_a_language_that_is_known_gets_colours()
      {
         var withoutColours = new ViewInAWindow(Markdown) { View = { ColoursCode = false } }.Window.RenderFrame().Fingerprint;
         for(var time = 0; time < 400 && _view.Window.RenderFrame().Fingerprint == withoutColours; time++) await Task.Delay(10);

         _view.Window.RenderFrame().SaveForInspectionAs("Code with colours");
         _view.Window.RenderFrame().Fingerprint.Must().NotBe(withoutColours);
      }

      [XFOnUIThread] public async Task is_as_high_with_colours_as_without_them()
      {
         var height = _view.View.DesiredSize.Height;
         await Task.Delay(500);
         _view.View.InvalidateMeasure();
         _view.Window.RenderFrame();
         _view.View.DesiredSize.Height.Must().Be(height);
      }

      Button? CopyButtonAt(string text)
      {
         _view.Window.MoveThePointerTo(_view.CenterOf(text));
         return CopyButton;
      }

      Point CenterOf(Control control) => _view.Window.PointOf(control, new Rect(control.Bounds.Size).Center);
   }

   public class An_image : Reading_markdown
   {
      static async Task<ViewInAWindow> Show(string markdown, double width = 120)
      {
         var view = new ViewInAWindow(markdown) { View = { LoadImage = source => Task.FromResult<IImage?>(source == "missing.png" ? null : PictureThatIs(width)) } };
         await Task.Delay(50);
         view.Window.RenderFrame();
         return view;
      }

      static RenderTargetBitmap PictureThatIs(double width)
      {
         var picture = new RenderTargetBitmap(new PixelSize((int)width, 60));
         using var context = picture.CreateDrawingContext();
         context.FillRectangle(Brushes.Orange, new Rect(picture.Size));
         return picture;
      }

      [XFOnUIThread] public void is_shown_as_its_description_when_none_are_loaded() => new ViewInAWindow("a ![b c](d.png)").BoundsOf("b c").Must().Satisfy(it => it.Height < 30 && it.Width < 60);
      [XFOnUIThread] public async Task that_is_loaded_is_shown_in_its_size() => (await Show("a ![b c](d.png) e")).BoundsOf("b c").Must().Satisfy(it => Math.Abs(it.Width - 120) < 1);
      [XFOnUIThread] public async Task that_is_loaded_makes_its_line_as_high_as_it_is() => (await Show("a ![b c](d.png) e")).View.DesiredSize.Height.Must().BeGreaterThan(75);
      [XFOnUIThread] public async Task that_is_wider_than_the_view_is_shown_as_wide_as_that() => (await Show("![b](d.png)", width: 5000)).BoundsOf("b").Must().Satisfy(it => Math.Abs(it.Width - 780) < 2);
      [XFOnUIThread] public async Task that_there_is_none_of_is_shown_as_its_description() => (await Show("![b c](missing.png)")).BoundsOf("b c").Height.Must().BeGreaterThan(0);
      [XFOnUIThread] public async Task is_copied_as_its_markdown() => (await Show("a ![b c](d.png) e")).Select("a", to: "e").View.SelectedMarkdown.Must().Be("a ![b c](d.png) e");
      [XFOnUIThread] public async Task tells_that_it_is_clicked() => Clicked(await Show("![b c](d.png)"), "b c").ClickedLinks.Must().SequenceEqual(["d.png"]);
      [XFOnUIThread] public async Task in_a_link_tells_that_the_link_is_clicked() => Clicked(await Show("[![b c](d.png)](https://e.f)"), "b c").ClickedLinks.Must().SequenceEqual(["https://e.f"]);
      [XFOnUIThread] public async Task is_seen() => (await Show("- a ![b c](d.png) e\n- [x] ticked\n- [ ] not ticked")).Window.RenderFrame().SaveForInspectionAs("Images and boxes");
   }

   public class Highlighting : Reading_markdown
   {
      readonly ViewInAWindow _view = new("# one two\n\na **two** b\n\n- three");

      PlainTextRange RangeOf(string text) => new(_view.View.PlainText.Value.IndexOf(text, StringComparison.Ordinal), text.Length);

      [XFOnUIThread] public void some_text_shows_it_otherwise()
      {
         var before = _view.Window.RenderFrame().Fingerprint;
         _view.View.Highlights = [new TextHighlight(RangeOf("two"), Brushes.Yellow)];
         _view.Window.RenderFrame().SaveForInspectionAs("A highlight");
         _view.Window.RenderFrame().Fingerprint.Must().NotBe(before);
      }

      [XFOnUIThread] public void nothing_any_more_shows_it_as_before()
      {
         var before = _view.Window.RenderFrame().Fingerprint;
         _view.View.Highlights = [new TextHighlight(RangeOf("two"), Brushes.Yellow)];
         _view.Window.RenderFrame();
         _view.View.Highlights = [];
         _view.Window.RenderFrame().Fingerprint.Must().Be(before);
      }

      [XFOnUIThread] public void the_same_text_twice_shows_the_last() => FrameWith(Brushes.Yellow, Brushes.Orange).Must().NotBe(FrameWith(Brushes.Orange, Brushes.Yellow));
      [XFOnUIThread] public void text_across_blocks_says_where_all_of_it_is() => _view.View.BoundsOf(RangeOf("two\na two b\nth")).Must().Satisfy(it => it.Count >= 3 && it.Zip(it.Skip(1)).All(pair => pair.First.Top <= pair.Second.Top));

      string FrameWith(params IBrush[] brushes)
      {
         _view.View.Highlights = [..brushes.Select(it => new TextHighlight(RangeOf("two"), it))];
         return _view.Window.RenderFrame().Fingerprint;
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
