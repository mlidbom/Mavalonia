using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.Markdown.Controls.Specifications.Laying_out_what_is_read;

public class A_list_item_with_a_number_of_many_digits
{
   [XFOnUIThread] public void has_room_for_the_number_before_its_text() => new ViewInAWindow("10000. a").BoundsOf("a").Left.Must().BeGreaterThan(10 + new ViewInAWindow("10000\\. b").BoundsOf("10000.").Width);
}
