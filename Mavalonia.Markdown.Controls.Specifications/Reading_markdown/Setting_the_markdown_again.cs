using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.Markdown.Rendering;

namespace Mavalonia.Markdown.Controls.Specifications.Reading_markdown;

public class Setting_the_markdown_again
{
   [XFOnUIThread] public void to_null_as_a_binding_may_shows_nothing()
   {
      var view = new ViewInAWindow("one") { View = { Markdown = null! } };
      view.View.PlainText.Value.Must().Be("");
   }

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
      var view = new ViewInAWindow("**a**") { View = { Dialect = MarkdownDialect.PlainText } };
      view.View.PlainText.Value.Must().Be("**a**");
   }
}
