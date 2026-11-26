using Avalonia.Media;
using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Controls.Specifications.Reading_markdown;

///<summary>
/// Text that the application shows as a link where the markdown has none (<see cref="MarkdownView.Links"/>), such as the name of something in its code.
/// It looks as the markdown's links do (Magnus, 2026-11-26: "The styling that the old symbollinker java code had was fine, go with that"), in code too
/// ("Why would code not link symbols?").
///</summary>
public class Text_shown_as_a_link
{
   const string Code = "```cs\nreturn some;\n```";

   static ViewInAWindow ViewWithALink(string markdown, string linked, string target = "cloyd://linked")
   {
      var view = new ViewInAWindow(markdown);
      view.View.Links = [new TextLink(view.RangeOf(linked), target)];
      return view;
   }

   static string FrameOf(ViewInAWindow view) => view.Window.RenderFrame().Fingerprint;

   [XFOnUIThread] public void looks_as_a_link_of_the_markdown_does() => FrameOf(ViewWithALink("a two b", "two")).Must().Be(FrameOf(new ViewInAWindow("a [two](x) b")));
   [XFOnUIThread] public void in_code_of_a_paragraph_looks_as_a_link_of_the_markdown_does() => FrameOf(ViewWithALink("a `TheHub` b", "TheHub")).Must().Be(FrameOf(new ViewInAWindow("a [`TheHub`](x) b")));
   [XFOnUIThread] public void in_bold_looks_as_a_link_of_the_markdown_does() => FrameOf(ViewWithALink("a **two** b", "two")).Must().Be(FrameOf(new ViewInAWindow("a **[two](x)** b")));
   [XFOnUIThread] public void where_a_link_of_the_markdown_is_looks_as_that_link_does() => FrameOf(ViewWithALink("a [two](x) b", "two")).Must().Be(FrameOf(new ViewInAWindow("a [two](x) b")));

   [XFOnUIThread] public void that_is_no_more_looks_as_before()
   {
      var view = new ViewInAWindow("a two b");
      var before = FrameOf(view);
      view.View.Links = [new TextLink(view.RangeOf("two"), "x")];
      view.Window.RenderFrame();
      view.View.Links = [];
      FrameOf(view).Must().Be(before);
   }

   [XFOnUIThread] public void none_as_a_binding_may_give_looks_as_before()
   {
      var view = new ViewInAWindow("a two b");
      var before = FrameOf(view);
      view.View.Links = null!;
      FrameOf(view).Must().Be(before);
   }

   [XFOnUIThread] public void beyond_the_text_looks_as_before()
   {
      var view = new ViewInAWindow("a two b");
      var before = FrameOf(view);
      view.View.Links = [new TextLink(new PlainTextRange(1000, 5), "x"), new TextLink(new PlainTextRange(-10, 5), "y")];
      FrameOf(view).Must().Be(before);
   }

   [XFOnUIThread] public void changes_no_sizes_nor_where_lines_end()
   {
      var view = new ViewInAWindow(string.Concat(Enumerable.Repeat("TheHub.Start(), and some words between, ", 12))) { View = { Width = 300 } };
      var everything = new PlainTextRange(0, view.View.PlainText.Value.Length);
      view.Window.RenderFrame();
      var before = (view.View.DesiredSize, Lines: view.View.BoundsOf(everything));
      view.View.Links = [..Enumerable.Range(0, view.View.PlainText.Value.Length).Where(it => view.View.PlainText.Value.AsSpan(it).StartsWith("TheHub.Start")).Select(it => new TextLink(new PlainTextRange(it, "TheHub.Start".Length), "x"))];
      view.Window.RenderFrame();
      (view.View.DesiredSize, Lines: view.View.BoundsOf(everything)).Must().Satisfy(it => it.DesiredSize == before.DesiredSize && it.Lines.SequenceEqual(before.Lines));
   }

   [XFOnUIThread] public void tells_that_it_is_clicked() => ViewWithALink("a two b", "two", "cloyd://two").Click("two").ClickedLinks.Must().SequenceEqual(["cloyd://two"]);
   [XFOnUIThread] public void tells_nothing_when_the_text_beside_it_is_clicked() => ViewWithALink("a two b", "two").Click("b").ClickedLinks.Must().BeEmpty();
   [XFOnUIThread] public void in_code_tells_that_it_is_clicked() => ViewWithALink(Code, "some", "cloyd://some").Click("some").ClickedLinks.Must().SequenceEqual(["cloyd://some"]);
   [XFOnUIThread] public void where_a_link_of_the_markdown_is_tells_that_that_is_clicked() => ViewWithALink("a [two](x) b", "two", "y").Click("two").ClickedLinks.Must().SequenceEqual(["x"]);

   [XFOnUIThread] public void where_another_overlaps_it_tells_that_the_last_is_clicked()
   {
      var view = new ViewInAWindow("a two b");
      view.View.Links = [new TextLink(view.RangeOf("two"), "first"), new TextLink(view.RangeOf("tw"), "last")];
      view.Click("two").ClickedLinks.Must().SequenceEqual(["last"]);
   }

   [XFOnUIThread] public void tells_where_it_is_clicked()
   {
      var view = ViewWithALink("a two b", "two").Click("two");
      (view.Clicks.Single().GetPosition(view.View) - view.BoundsOf("two").Center).Must().Satisfy(it => Math.Abs(it.X) < 0.5 && Math.Abs(it.Y) < 0.5);
   }

   [XFOnUIThread] public void has_the_pointer_look_like_a_hand()
   {
      var view = ViewWithALink("a two b", "two");
      view.Window.MoveThePointerTo(view.CenterOf("two"));
      view.View.Cursor!.ToString().Must().Be("Hand");
   }

   [XFOnUIThread] public void is_where_the_menu_copies_a_link_from()
   {
      var view = ViewWithALink("a two b", "two", "cloyd://two");
      view.View.TargetOfTheLinkAt(view.BoundsOf("two").Center).Must().Be("cloyd://two");
   }

   [XFOnUIThread] public void is_copied_as_the_markdown_it_is()
   {
      var view = ViewWithALink("a **two** b", "two");
      view.View.SelectAll();
      view.View.SelectedMarkdown.Must().Be("a **two** b");
   }

   [XFOnUIThread] public async Task in_code_is_drawn_in_the_colour_of_links_over_the_colours_of_code_and_underlined()
   {
      var view = new ViewInAWindow(Code);
      var withoutColours = new ViewInAWindow(Code) { View = { ColoursCode = false } }.Window.RenderFrame().Fingerprint;
      for(var time = 0; time < 400 && FrameOf(view) == withoutColours; time++) await Task.Delay(10);

      var before = PixelsInTheColourOfLinks();
      view.View.Links = [new TextLink(view.RangeOf("return"), "x")];
      var linked = PixelsInTheColourOfLinks();
      view.Window.RenderFrame().SaveForInspectionAs("Code with a link");

      new { IsColoured = FrameOf(view) != withoutColours, Before = before.Max(), MostOnARow = linked.Max(), view.PixelsOf("return").Width }
        .Must().Satisfy(it => it.IsColoured && it.Before == 0 && it.MostOnARow >= it.Width * 0.9);

      // How many pixels on each row of where return is are drawn in the colour of links: those that change with it.
      IReadOnlyList<int> PixelsInTheColourOfLinks()
      {
         view.View.LinkForeground = Brushes.Orange;
         var orange = view.Window.RenderFrame();
         view.View.LinkForeground = Brushes.Blue;
         return orange.PixelsThatDifferFrom(view.Window.RenderFrame(), view.PixelsOf("return"));
      }
   }
}
