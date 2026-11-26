using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.Markdown.Controls.Specifications.Reading_markdown;

public class The_menu
{
   static MenuOfAControl OpenMenuAt(string text, ViewInAWindow view)
   {
      view.Window.RightClick(view.CenterOf(text));
      return new MenuOfAControl(view.View);
   }

   [XFOnUIThread] public void opens_with_the_right_button() => OpenMenuAt("one", new ViewInAWindow("one")).IsOpen.Must().BeTrue();

   [XFOnUIThread] public void opens_with_the_menu_key()
   {
      var view = new ViewInAWindow("one two").Select("one", to: "one").Press(PhysicalKey.ContextMenu);
      new MenuOfAControl(view.View).IsOpen.Must().BeTrue();
   }

   [XFOnUIThread] public void opens_below_the_caret_when_it_is_opened_with_the_keyboard()
   {
      var view = new ViewInAWindow("one\n\ntwo\n\nthree").Select("two", to: "two").Press(PhysicalKey.ContextMenu);
      new { Menu = new MenuOfAControl(view.View).Place, Text = view.BoundsOf("two") }.Must().Satisfy(it => Point.Distance(it.Menu, it.Text.BottomRight) < 5);
   }

   [XFOnUIThread] public void opens_where_the_pointer_is()
   {
      var view = new ViewInAWindow("one\n\ntwo\n\nthree");
      new { Menu = OpenMenuAt("two", view).Place, Text = view.BoundsOf("two") }.Must().Satisfy(it => Point.Distance(it.Menu, it.Text.Center) < 5);
   }

   [XFOnUIThread] public void offers_to_copy_and_to_select_all() => OpenMenuAt("one", new ViewInAWindow("one two").Select("one", to: "two")).Shown.Must().SequenceEqual(["Copy (Ctrl+C)", "Select all (Ctrl+A)"]);
   [XFOnUIThread] public void can_not_copy_when_nothing_is_selected() => OpenMenuAt("one", new ViewInAWindow("one two")).Choices.Must().SequenceEqual(["Select all"]);
   [XFOnUIThread] public void can_choose_nothing_when_nothing_is_shown() => OpenedBesideTheText("").Choices.Must().BeEmpty();
   [XFOnUIThread] public void offers_to_copy_a_link_that_it_is_opened_at() => OpenMenuAt("link", new ViewInAWindow("a [link](https://b.c)")).Choices.Must().SequenceEqual(["Copy link", "Select all"]);
   [XFOnUIThread] public void offers_to_copy_the_link_to_an_image_that_it_is_opened_at() => OpenMenuAt("image", new ViewInAWindow("an ![image](https://b.c)")).Choices.Must().SequenceEqual(["Copy link", "Select all"]);

   [XFOnUIThread] public void offers_no_link_when_it_is_opened_with_the_keyboard()
   {
      var view = new ViewInAWindow("[link](https://b.c)").Select("link", to: "link");
      var menu = OpenMenuAt("link", view);
      menu.Close();

      view.View.Focus();
      view.Press(PhysicalKey.ContextMenu);
      menu.Shown.Must().SequenceEqual(["Copy (Ctrl+C)", "Select all (Ctrl+A)"]);
   }

   [XFOnUIThread] public async Task copies_what_is_selected()
   {
      var view = new ViewInAWindow("a **b** c").Select("b", to: "b");
      await OpenMenuAt("b", view).Choose("Copy");
      (await view.Window.Clipboard.TryGetTextAsync()).Must().Be("**b**");
   }

   [XFOnUIThread] public async Task copies_a_link()
   {
      var view = new ViewInAWindow("a [link](https://b.c)");
      await OpenMenuAt("link", view).Choose("Copy link");
      (await view.Window.Clipboard.TryGetTextAsync()).Must().Be("https://b.c");
   }

   [XFOnUIThread] public async Task selects_all()
   {
      var view = new ViewInAWindow("# a\n\nb");
      await OpenMenuAt("b", view).Choose("Select all");
      view.View.SelectedMarkdown.Must().Be("# a\n\nb");
   }

   [XFOnUIThread] public void leaves_the_selection_that_it_is_opened_in()
   {
      var view = new ViewInAWindow("one two three").Select("one", to: "two");
      OpenMenuAt("two", view);
      view.View.SelectedMarkdown.Must().Be("one two");
   }

   [XFOnUIThread] public void ends_the_selection_that_it_is_opened_outside_of()
   {
      var view = new ViewInAWindow("one two three").Select("one", to: "one");
      OpenMenuAt("three", view);
      view.View.IsAnythingSelected.Must().BeFalse();
   }

   [XFOnUIThread] public void does_not_click_the_link_that_it_is_opened_at()
   {
      var view = new ViewInAWindow("a [link](https://b.c)");
      OpenMenuAt("link", view);
      view.ClickedLinks.Must().BeEmpty();
   }

   [XFOnUIThread] public void of_the_application_is_opened_instead_when_there_is_one_and_can_have_the_link()
   {
      var view = new ViewInAWindow("a [link](https://b.c)");
      string? target = null;
      var menuOfTheApplication = new ContextMenu { ItemsSource = new[] { new MenuItem { Header = "Copy link" } } };
      view.View.ContextMenu = menuOfTheApplication;
      view.View.AddHandler(InputElement.ContextRequestedEvent, (_, request) => target = request.TryGetPosition(view.View, out var point) ? view.View.TargetOfTheLinkAt(point) : null, RoutingStrategies.Tunnel);

      var ours = OpenMenuAt("link", view);
      new { Theirs = menuOfTheApplication.IsOpen, Ours = ours.IsOpen, target }.Must().Satisfy(it => it.Theirs && !it.Ours && it.target == "https://b.c");
   }

   [XFOnUIThread] public void is_not_opened_when_the_application_wants_none()
   {
      var view = new ViewInAWindow("one");
      var ours = new MenuOfAControl(view.View);
      view.View.ContextFlyout = null;
      view.Window.RightClick(view.CenterOf("one"));
      ours.IsOpen.Must().BeFalse();
   }

   static MenuOfAControl OpenedBesideTheText(string markdown)
   {
      var view = new ViewInAWindow(markdown);
      view.Window.RightClick(view.Window.PointOf(view.View, new Point(5, 5)));
      return new MenuOfAControl(view.View);
   }
}
