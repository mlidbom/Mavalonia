using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
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
