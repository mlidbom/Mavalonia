using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;
using AvaloniaEdit;
using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.Markdown.Controls.Specifications;

public class Using_the_menu
{
   static EditorInAWindow Editor(string markdown, bool inTheSource = false)
   {
      var editor = new EditorInAWindow(markdown) { Editor = { ShowsSource = inTheSource } };
      editor.Editor.FocusTheView();
      return editor;
   }

   ///<summary>Opens the menu with the keyboard, so that what is selected stays so.</summary>
   static MenuOfAControl OpenMenuOf(EditorInAWindow editor)
   {
      editor.Press(PhysicalKey.ContextMenu);
      return new MenuOfAControl(editor.Editor);
   }

   static async Task<(string Markdown, string? Clipboard)> Choose(string item, string into, bool inTheSource, string clipboard = "x")
   {
      var editor = Editor(into, inTheSource);
      await editor.Window.Clipboard.SetTextAsync(clipboard);
      await OpenMenuOf(editor).Choose(item);
      return (editor.Markdown, await editor.Window.Clipboard.TryGetTextAsync());
   }

   public class Opening : Using_the_menu
   {
      static bool IsOpenAfterARightClick(EditorInAWindow editor)
      {
         editor.Window.RightClick(new Point(60, 18));
         return new MenuOfAControl(editor.Editor).IsOpen;
      }

      [XFOnUIThread] public void is_done_with_the_right_button_in_the_rendered_view() => IsOpenAfterARightClick(Editor("one two‸")).Must().BeTrue();
      [XFOnUIThread] public void is_done_with_the_right_button_in_the_source_view() => IsOpenAfterARightClick(Editor("one two‸", inTheSource: true)).Must().BeTrue();
      [XFOnUIThread] public void is_done_with_the_menu_key_in_the_rendered_view() => OpenMenuOf(Editor("one‸")).IsOpen.Must().BeTrue();
      [XFOnUIThread] public void is_done_with_the_menu_key_in_the_source_view() => OpenMenuOf(Editor("one‸", inTheSource: true)).IsOpen.Must().BeTrue();

      [XFOnUIThread] public void opens_below_the_caret_when_it_is_opened_with_the_keyboard()
      {
         var editor = Editor("one\n\ntwo\n\n‸three");
         var place = OpenMenuOf(editor).Place;
         place.Must().Satisfy(it => Point.Distance(it, new Point(10, 10 + 2 * 25 + 17)) < 8);
      }

      [XFOnUIThread] public void opens_below_the_caret_when_it_is_opened_with_the_keyboard_in_the_source_view()
      {
         var editor = Editor("one\n\ntwo\n\n‸three", inTheSource: true);
         var place = OpenMenuOf(editor).Place;
         place.Must().Satisfy(it => Point.Distance(it, new Point(10, 10 + 5 * 19)) < 8);
      }

      [XFOnUIThread] public void opens_the_menu_of_the_application_instead_when_there_is_one()
      {
         var editor = Editor("one two‸");
         var menuOfTheApplication = new ContextMenu { ItemsSource = new[] { new MenuItem { Header = "Quote" } } };
         editor.Editor.ContextMenu = menuOfTheApplication;
         var ours = new MenuOfAControl(editor.Editor);
         editor.Window.RightClick(new Point(60, 18));
         new { Theirs = menuOfTheApplication.IsOpen, Ours = ours.IsOpen }.Must().Satisfy(it => it.Theirs && !it.Ours);
      }

      [XFOnUIThread] public void opens_the_flyout_of_the_application_instead_when_there_is_one()
      {
         var editor = Editor("one two‸");
         var ours = new MenuOfAControl(editor.Editor);
         var flyoutOfTheApplication = new MenuFlyout { ItemsSource = new[] { new MenuItem { Header = "Quote" } } };
         editor.Editor.ContextFlyout = flyoutOfTheApplication;
         editor.Window.RightClick(new Point(60, 18));
         new { Theirs = flyoutOfTheApplication.IsOpen, Ours = ours.IsOpen }.Must().Satisfy(it => it.Theirs && !it.Ours);
      }

      [XFOnUIThread] public void opens_none_when_the_application_wants_none()
      {
         var editor = Editor("one two‸");
         var ours = new MenuOfAControl(editor.Editor);
         editor.Editor.ContextFlyout = null;
         editor.Window.RightClick(new Point(60, 18));
         ours.IsOpen.Must().BeFalse();
      }
   }

   public class The_right_button : Using_the_menu
   {
      static string MarkdownAfterARightClick(string markdown, double x, bool inTheSource = false)
      {
         var editor = Editor(markdown, inTheSource);
         Visual view = inTheSource ? editor.Editor.GetVisualDescendants().OfType<TextEditor>().Single() : editor.RenderedView;
         editor.Window.RightClick(editor.Window.PointOf(view, new Point(10 + x, 18)));
         return editor.Markdown;
      }

      [XFOnUIThread] public void puts_the_caret_where_it_is_pressed() => MarkdownAfterARightClick("‸one two", x: 300).Must().Be("one two‸");
      [XFOnUIThread] public void puts_the_caret_where_it_is_pressed_outside_of_the_selection() => MarkdownAfterARightClick("⟦one⟧ two", x: 300).Must().Be("one two‸");
      [XFOnUIThread] public void leaves_the_selection_that_it_is_pressed_in() => MarkdownAfterARightClick("⟦one two⟧", x: 10).Must().Be("⟦one two⟧");
      [XFOnUIThread] public void puts_the_caret_where_it_is_pressed_in_the_source_view() => MarkdownAfterARightClick("one two three‸", x: 1, inTheSource: true).Must().Be("‸one two three");
      [XFOnUIThread] public void puts_the_caret_where_it_is_pressed_outside_of_the_selection_in_the_source_view() => MarkdownAfterARightClick("one two ⟦three⟧", x: 1, inTheSource: true).Must().Be("‸one two three");
      [XFOnUIThread] public void leaves_the_selection_that_it_is_pressed_in_in_the_source_view() => MarkdownAfterARightClick("⟦one two⟧", x: 10, inTheSource: true).Must().Be("⟦one two⟧");
   }

   public class The_items : Using_the_menu
   {
      [XFOnUIThread] public void tell_the_keys_that_do_the_same() =>
         OpenMenuOf(Editor("⟦one⟧")).Shown.Must().SequenceEqual(["Cut (Ctrl+X)", "Copy (Ctrl+C)", "Paste (Ctrl+V)", "Paste as text (Ctrl+Shift+V)", "Paste as quote (Ctrl+Shift+Q)", "Select all (Ctrl+A)"]);

      [XFOnUIThread] public void tell_the_keys_that_the_application_has_chosen()
      {
         var editor = Editor("⟦one⟧");
         editor.Editor.PasteAsQuoteGesture = new KeyGesture(Key.F2);
         editor.Editor.PasteTextGesture = null;
         OpenMenuOf(editor).Shown.Must().Satisfy(it => it.Contains("Paste as text") && it.Contains("Paste as quote (F2)"));
      }

      [XFOnUIThread] public void can_all_be_chosen_when_something_is_selected() => OpenMenuOf(Editor("⟦one⟧")).Choices.Must().HaveCount(6);
      [XFOnUIThread] public void can_not_cut_or_copy_when_nothing_is_selected() => OpenMenuOf(Editor("one‸")).Choices.Must().SequenceEqual(["Paste", "Paste as text", "Paste as quote", "Select all"]);
      [XFOnUIThread] public void can_only_paste_when_the_document_is_empty() => OpenMenuOf(Editor("‸")).Choices.Must().SequenceEqual(["Paste", "Paste as text", "Paste as quote"]);
   }

   public class In_the_rendered_view : Using_the_menu
   {
      [XFOnUIThread] public async Task Cut_cuts_what_is_selected() => (await Choose("Cut", "a **⟦b⟧** c", inTheSource: false)).Must().Satisfy(it => it.Clipboard == "**b**" && it.Markdown == "a ‸ c");
      [XFOnUIThread] public async Task Copy_copies_what_is_selected() => (await Choose("Copy", "a **⟦b⟧** c", inTheSource: false)).Must().Satisfy(it => it.Clipboard == "**b**" && it.Markdown == "a **⟦b⟧** c");
      [XFOnUIThread] public async Task Paste_pastes() => (await Choose("Paste", "a ⟦b⟧", inTheSource: false)).Markdown.Must().Be("a x‸");
      [XFOnUIThread] public async Task Paste_as_text_pastes() => (await Choose("Paste as text", "a ⟦b⟧", inTheSource: false)).Markdown.Must().Be("a x‸");
      [XFOnUIThread] public async Task Paste_as_quote_pastes_a_quote() => (await Choose("Paste as quote", "a‸", inTheSource: false)).Markdown.Must().Be("a\n\n> x\n\n‸");
      [XFOnUIThread] public async Task Select_all_selects_all() => (await Choose("Select all", "a‸\n\nb", inTheSource: false)).Markdown.Must().Be("⟦a\n\nb⟧");
   }

   public class In_the_source_view : Using_the_menu
   {
      [XFOnUIThread] public async Task Cut_cuts_what_is_selected() => (await Choose("Cut", "a **⟦b⟧** c", inTheSource: true)).Must().Satisfy(it => it.Clipboard == "b" && it.Markdown == "a **‸** c");
      [XFOnUIThread] public async Task Copy_copies_what_is_selected() => (await Choose("Copy", "a **⟦b⟧** c", inTheSource: true)).Must().Satisfy(it => it.Clipboard == "b" && it.Markdown == "a **⟦b⟧** c");
      [XFOnUIThread] public async Task Paste_pastes() => (await Choose("Paste", "a ⟦b⟧", inTheSource: true)).Markdown.Must().Be("a x‸");
      [XFOnUIThread] public async Task Paste_as_text_pastes() => (await Choose("Paste as text", "a ⟦b⟧", inTheSource: true)).Markdown.Must().Be("a x‸");
      [XFOnUIThread] public async Task Paste_as_quote_pastes_a_quote() => (await Choose("Paste as quote", "a‸", inTheSource: true)).Markdown.Must().Be("a\n\n> x\n\n‸");
      [XFOnUIThread] public async Task Select_all_selects_all() => (await Choose("Select all", "a‸\n\nb", inTheSource: true)).Markdown.Must().Be("⟦a\n\nb⟧");
   }
}
