using Avalonia.Layout;
using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Controls.Specifications.Laying_out_what_is_read;

///<summary>
/// A <see cref="MarkdownView"/> that is aligned to a side rather than stretched, such as in the bubble of a message in a chat:
/// it's no wider than what it shows, as a TextBlock is, so that a short message's bubble is no wider than its text.
///</summary>
public class A_view_aligned_to_a_side
{
   const double Padding = 10;

   static ViewInAWindow AlignedToTheRight(string markdown)
   {
      var view = new ViewInAWindow(markdown) { View = { HorizontalAlignment = HorizontalAlignment.Right } };
      view.Window.RenderFrame();
      return view;
   }

   static ViewInAWindow Stretched(string markdown)
   {
      var view = new ViewInAWindow(markdown);
      view.Window.RenderFrame();
      return view;
   }

   static double WidthOf(ViewInAWindow view) => view.View.Bounds.Width;

   ///<summary>No less than <paramref name="width"/>, and more only as layout rounds it up to whole pixels.</summary>
   static Func<double, bool> ItIsAtMostAPixelMoreThan(double width) => it => it >= width && it <= width + 1;

   [XFOnUIThread] public void is_as_wide_as_a_short_paragraph_and_its_padding()
   {
      var view = AlignedToTheRight("a two b");
      WidthOf(view).Must().Satisfy(ItIsAtMostAPixelMoreThan(view.BoundsOf("a two b").Right + Padding));
   }

   [XFOnUIThread] public void ends_the_lines_of_a_paragraph_that_wraps_where_a_stretched_view_does()
   {
      var markdown = string.Concat(Enumerable.Repeat("Some words of a paragraph that wraps, ", 30));
      var everything = new PlainTextRange(0, Stretched(markdown).View.PlainText.Value.Length);
      AlignedToTheRight(markdown).View.BoundsOf(everything).Must().SequenceEqual(Stretched(markdown).View.BoundsOf(everything));
   }

   [XFOnUIThread] public void is_as_wide_as_the_longest_line_of_code_with_the_space_around_it()
   {
      var view = AlignedToTheRight("```\nshort\nthe longest line\n```");
      var spaceBeforeCode = view.BoundsOf("short").Left;
      WidthOf(view).Must().Satisfy(ItIsAtMostAPixelMoreThan(view.BoundsOf("the longest line").Right + spaceBeforeCode));
   }

   [XFOnUIThread] public void is_as_wide_as_a_narrow_table()
   {
      var view = AlignedToTheRight("| A | B |\n|---|---|\n| one | two |");
      WidthOf(view).Must().Satisfy(it => it > view.BoundsOf("two").Right + Padding && it < 200);
   }

   [XFOnUIThread] public void shows_a_narrow_table_as_a_stretched_view_does() =>
      AlignedToTheRight("| A | B |\n|---|---|\n| one | two |").BoundsOf("two").Must().Be(Stretched("| A | B |\n|---|---|\n| one | two |").BoundsOf("two"));

   [XFOnUIThread] public void of_an_editor_is_as_wide_as_it_is_offered()
   {
      var editor = new EditorInAWindow("a two b‸") { Editor = { HorizontalAlignment = HorizontalAlignment.Right } };
      editor.Window.RenderFrame();
      editor.RenderedView.Bounds.Width.Must().Be(800.0);
   }
}
