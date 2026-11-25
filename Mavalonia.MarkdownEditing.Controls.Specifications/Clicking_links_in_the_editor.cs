using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.TextFormatting;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

public class Clicking_links_in_the_editor
{
   const RawInputModifiers Ctrl = RawInputModifiers.Control;

   readonly List<string> _clicked = [];
   readonly List<KeyModifiers> _modifiers = [];

   EditorInAWindow Editor(string markdown, bool inTheSource = false)
   {
      var editor = new EditorInAWindow(markdown) { Editor = { ShowsSource = inTheSource } };
      editor.Editor.LinkClicked += (_, click) =>
      {
         _clicked.Add(click.Target);
         _modifiers.Add(click.KeyModifiers);
      };
      editor.Editor.FocusTheView();
      return editor;
   }

   public class In_the_rendered_view : Clicking_links_in_the_editor
   {
      readonly EditorInAWindow _editor;
      readonly Point _link;

      public In_the_rendered_view()
      {
         _editor = Editor("[a long link](https://b.c) text‸");
         _link = _editor.PointInTheText(30);
      }

      [XFOnUIThread] public void tells_where_the_link_leads()
      {
         _editor.Window.Click(_link);
         _clicked.Must().SequenceEqual(["https://b.c"]);
      }

      [XFOnUIThread] public void puts_the_caret_in_the_link()
      {
         _editor.Window.Click(_link);
         _editor.Markdown.Must().Satisfy(it => it.IndexOf('‸') < it.IndexOf(']'));
      }

      [XFOnUIThread] public void tells_that_no_modifier_keys_are_pressed()
      {
         _editor.Window.Click(_link);
         _modifiers.Must().SequenceEqual([KeyModifiers.None]);
      }

      [XFOnUIThread] public void with_Ctrl_tells_where_the_link_leads()
      {
         _editor.Window.Click(_link, Ctrl);
         _clicked.Must().SequenceEqual(["https://b.c"]);
      }

      [XFOnUIThread] public void with_Ctrl_tells_that_it_is_pressed()
      {
         _editor.Window.Click(_link, Ctrl);
         _modifiers.Must().SequenceEqual([KeyModifiers.Control]);
      }

      [XFOnUIThread] public void twice_tells_it_once_and_selects_the_word()
      {
         _editor.Window.Click(_link);
         _editor.Window.Click(_link);
         new { _clicked, _editor.Markdown }.Must().Satisfy(it => it._clicked.Count == 1 && it.Markdown.Contains("[a ⟦long⟧", StringComparison.Ordinal));
      }

      [XFOnUIThread] public void beside_the_link_tells_nothing()
      {
         _editor.Window.Click(_editor.PointInTheText(300));
         _clicked.Must().BeEmpty();
      }

      [XFOnUIThread] public void with_the_right_button_tells_nothing()
      {
         _editor.Window.RightClick(_link);
         _clicked.Must().BeEmpty();
      }

      [XFOnUIThread] public void with_Shift_tells_nothing()
      {
         _editor.Window.Click(_link, RawInputModifiers.Shift);
         _clicked.Must().BeEmpty();
      }

      [XFOnUIThread] public void and_dragging_tells_nothing()
      {
         _editor.Window.Drag(_link, _link + new Vector(30, 0));
         _clicked.Must().BeEmpty();
      }

      [XFOnUIThread] public void has_the_pointer_look_like_a_hand_with_Ctrl()
      {
         _editor.Window.MoveThePointerTo(_link, Ctrl);
         _editor.RenderedView.Cursor!.ToString().Must().Be("Hand");
      }

      [XFOnUIThread] public void has_the_pointer_look_as_over_text_without_Ctrl()
      {
         _editor.Window.MoveThePointerTo(_link);
         _editor.RenderedView.Cursor!.ToString().Must().Be("Ibeam");
      }

      [XFOnUIThread] public void is_told_to_what_the_editor_is_in_too()
      {
         var told = new List<string>();
         ((Interactive)_editor.Editor.Parent!).AddHandler(MarkdownView.LinkClickedEvent, (_, click) => told.Add(click.Target));
         _editor.Window.Click(_link);
         told.Must().SequenceEqual(["https://b.c"]);
      }
   }

   public class In_the_source_view_of_markdown_that_has_links : Clicking_links_in_the_editor
   {
      ///<summary>Clicks on the char before the caret in <paramref name="markdown"/>.</summary>
      IReadOnlyList<string> ClickedIn(string markdown)
      {
         var editor = Editor(markdown, inTheSource: true);
         editor.Window.RenderFrame();
         var textView = editor.Editor.GetVisualDescendants().OfType<TextView>().Single();
         var end = editor.Window.PointOf(textView, editor.Editor.GetVisualDescendants().OfType<TextEditor>().Single().TextArea.Caret.CalculateCaretRectangle().Center);
         editor.Window.Click(end - new Vector(3, 0));
         return _clicked;
      }

      [XFOnUIThread] public void tells_where_a_link_leads_when_its_label_is_clicked() => ClickedIn("a [the lo‸g](src/x.cs:12) b").Must().SequenceEqual(["src/x.cs:12"]);
      [XFOnUIThread] public void tells_where_a_link_leads_when_its_target_is_clicked() => ClickedIn("a [the log](src/x‸.cs:12) b").Must().SequenceEqual(["src/x.cs:12"]);
      [XFOnUIThread] public void tells_where_a_link_leads_when_its_first_char_is_clicked() => ClickedIn("a [‸b](c) d").Must().SequenceEqual(["c"]);
      [XFOnUIThread] public void tells_where_a_link_leads_when_its_last_char_is_clicked() => ClickedIn("a [b](c)‸ d").Must().SequenceEqual(["c"]);
      [XFOnUIThread] public void tells_nothing_when_the_char_before_a_link_is_clicked() => ClickedIn("a ‸[b](c) d").Must().BeEmpty();
      [XFOnUIThread] public void tells_nothing_when_the_char_after_a_link_is_clicked() => ClickedIn("a [b](c) ‸d").Must().BeEmpty();
      [XFOnUIThread] public void tells_where_a_link_leads_rather_than_the_URL_that_its_label_is() => ClickedIn("a [https://b.inv‸alid](c) d").Must().SequenceEqual(["c"]);
      [XFOnUIThread] public void tells_where_a_link_of_any_protocol_leads() => ClickedIn("a [b](clo‸yd://session/5) d").Must().SequenceEqual(["cloyd://session/5"]);
      [XFOnUIThread] public void tells_where_a_link_in_angle_brackets_leads() => ClickedIn("a <vsco‸de://file/x> d").Must().SequenceEqual(["vscode://file/x"]);
      [XFOnUIThread] public void tells_where_a_link_that_is_defined_elsewhere_leads() => ClickedIn("a [b‸][c] d\n\n[c]: src/x.cs").Must().SequenceEqual(["src/x.cs"]);
      [XFOnUIThread] public void tells_where_a_link_in_bold_text_leads() => ClickedIn("**a [b‸](c)** d").Must().SequenceEqual(["c"]);
      [XFOnUIThread] public void tells_where_a_link_in_a_cell_of_a_table_leads() => ClickedIn("| A |\n|---|\n| [b‸](c) |").Must().SequenceEqual(["c"]);
      [XFOnUIThread] public void tells_where_a_link_in_a_list_in_a_quote_leads() => ClickedIn("> - a\n> - [b‸](c)").Must().SequenceEqual(["c"]);
      [XFOnUIThread] public void tells_nothing_of_a_link_in_code() => ClickedIn("`[b‸](c)`").Must().BeEmpty();
      [XFOnUIThread] public void tells_of_a_link_that_was_typed_there()
      {
         var editor = Editor("‸", inTheSource: true).Type("[b](c)");
         editor.Window.RenderFrame();
         editor.Window.Click(editor.Window.PointOf(editor.Editor.GetVisualDescendants().OfType<TextView>().Single(), new Point(3, 8)));
         _clicked.Must().SequenceEqual(["c"]);
      }
   }

   public class In_the_source_view_of_a_line_that_wraps_before_a_URL : Clicking_links_in_the_editor
   {
      const string URL = "https://b.invalid/abcdefghijklmnopqrstuvwxyz";

      readonly EditorInAWindow _editor;
      readonly TextView _textView;
      readonly TextLine _firstRow;

      public In_the_source_view_of_a_line_that_wraps_before_a_URL()
      {
         _editor = Editor(string.Concat(Enumerable.Repeat("word ", 16)) + URL + "‸", inTheSource: true);
         _editor.Window.RenderFrame();
         _textView = _editor.Editor.GetVisualDescendants().OfType<TextView>().Single();
         _firstRow = _textView.VisualLines.Single().TextLines[0];
      }

      void Click(Point inTheTextView) => _editor.Window.Click(_editor.Window.PointOf(_textView, inTheTextView));

      [XFOnUIThread] public void at_the_start_of_the_next_row_tells_where_the_URL_leads()
      {
         Click(new Point(5, _firstRow.Height * 1.5));
         _clicked.Must().SequenceEqual([URL]);
      }

      [XFOnUIThread] public void beside_the_end_of_its_first_row_tells_nothing()
      {
         Click(new Point(_firstRow.WidthIncludingTrailingWhitespace + 30, _firstRow.Height / 2));
         _clicked.Must().BeEmpty();
      }
   }

   public class In_the_source_view : Clicking_links_in_the_editor
   {
      readonly EditorInAWindow _editor;
      readonly Point _link;
      readonly List<bool> _isLeftToTheWindowToOpen = [];

      public In_the_source_view()
      {
         _editor = Editor("https://b.invalid/c text‸", inTheSource: true);
         _editor.Window.RenderFrame();
         _link = _editor.Window.PointOf(_editor.Editor.GetVisualDescendants().OfType<TextEditor>().Single(), new Point(60, 18));
         ((Interactive)_editor.Editor.Parent!).AddHandler(VisualLineLinkText.OpenUriEvent, (_, open) => _isLeftToTheWindowToOpen.Add(!open.Handled), handledEventsToo: true);
      }

      [XFOnUIThread] public void tells_where_the_link_leads()
      {
         _editor.Window.Click(_link);
         _clicked.Must().SequenceEqual(["https://b.invalid/c"]);
      }

      [XFOnUIThread] public void puts_the_caret_in_the_link()
      {
         _editor.Window.Click(_link);
         _editor.Markdown.Must().Satisfy(it => it.IndexOf('‸') < it.IndexOf(' '));
      }

      [XFOnUIThread] public void tells_that_no_modifier_keys_are_pressed()
      {
         _editor.Window.Click(_link);
         _modifiers.Must().SequenceEqual([KeyModifiers.None]);
      }

      [XFOnUIThread] public void with_Ctrl_tells_where_the_link_leads()
      {
         _editor.Window.Click(_link, Ctrl);
         _clicked.Must().SequenceEqual(["https://b.invalid/c"]);
      }

      [XFOnUIThread] public void with_Ctrl_tells_that_it_is_pressed()
      {
         _editor.Window.Click(_link, Ctrl);
         _modifiers.Must().SequenceEqual([KeyModifiers.Control]);
      }

      [XFOnUIThread] public void with_Ctrl_does_not_leave_it_to_the_window_to_open_the_link()
      {
         _editor.Window.Click(_link, Ctrl);
         _isLeftToTheWindowToOpen.Must().SequenceEqual([false]);
      }

      [XFOnUIThread] public void twice_tells_it_once()
      {
         _editor.Window.Click(_link);
         _editor.Window.Click(_link);
         _clicked.Must().HaveCount(1);
      }

      [XFOnUIThread] public void beside_the_link_tells_nothing()
      {
         _editor.Window.Click(_link + new Vector(500, 0));
         _clicked.Must().BeEmpty();
      }

      [XFOnUIThread] public void below_the_text_tells_nothing()
      {
         _editor.Window.Click(_link + new Vector(0, 300));
         _clicked.Must().BeEmpty();
      }

      [XFOnUIThread] public void with_the_right_button_tells_nothing()
      {
         _editor.Window.RightClick(_link);
         _clicked.Must().BeEmpty();
      }

      [XFOnUIThread] public void with_Ctrl_and_the_right_button_tells_nothing()
      {
         _editor.Window.RightClick(_link, Ctrl);
         _clicked.Must().BeEmpty();
      }

      [XFOnUIThread] public void with_Shift_tells_nothing()
      {
         _editor.Window.Click(_link, RawInputModifiers.Shift);
         _clicked.Must().BeEmpty();
      }

      [XFOnUIThread] public void and_dragging_tells_nothing()
      {
         _editor.Window.Drag(_link, _link + new Vector(30, 0));
         _clicked.Must().BeEmpty();
      }
   }
}
