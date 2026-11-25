using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.Laying_out_what_is_read;

public class A_view_that_is_not_shown
{
   [XFOnUIThread] public void says_where_its_text_is() => new MarkdownView { Markdown = "a" }.BoundsOf(new PlainTextRange(0, 1)).Must().HaveCount(1);
}
