using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.Reading_markdown;

public class A_link
{
   [XFOnUIThread] public void tells_that_it_is_clicked() => new ViewInAWindow("a [link](https://b.c) d").Click("link").ClickedLinks.Must().SequenceEqual(["https://b.c"]);
   [XFOnUIThread] public void that_is_a_bare_URL_tells_that_it_is_clicked() => new ViewInAWindow("a https://b.c d").Click("b.c").ClickedLinks.Must().SequenceEqual(["https://b.c"]);
   [XFOnUIThread] public void tells_nothing_when_the_text_beside_it_is_clicked() => new ViewInAWindow("[link](https://b.c) other").Click("other").ClickedLinks.Must().BeEmpty();
   [XFOnUIThread] public void tells_nothing_when_text_is_selected_from_it() => new ViewInAWindow("[link](https://b.c) other").Select("ink", to: "other").ClickedLinks.Must().BeEmpty();
   [XFOnUIThread] public void has_the_pointer_look_like_a_hand() => CursorOver("link").Must().Be("Hand");
   [XFOnUIThread] public void has_the_pointer_look_as_over_text_beside_it() => CursorOver("other").Must().Be("Ibeam");

   static string CursorOver(string text)
   {
      var view = new ViewInAWindow("[link](https://b.c) other");
      view.Window.MoveThePointerTo(view.CenterOf(text));
      return view.View.Cursor!.ToString();
   }
}
