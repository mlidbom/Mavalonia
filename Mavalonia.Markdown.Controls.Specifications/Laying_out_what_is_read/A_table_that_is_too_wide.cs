using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Controls.Specifications.Laying_out_what_is_read;

public class A_table_that_is_too_wide
{
   static readonly string LongText = string.Join(' ', Enumerable.Range(0, 60).Select(it => $"word{it}"));

   readonly ViewInAWindow _view = new($"| Name | Type | What it is for |\n|---|---|---|\n| Description | string | {LongText} {LongText} |\n| Remarks | string | {LongText} |");

   int CountOfLinesOf(string text) => _view.View.BoundsOf(new PlainTextRange(_view.View.PlainText.Value.IndexOf(text, StringComparison.Ordinal), text.Length)).Count;

   [XFOnUIThread] public void has_the_narrow_columns_as_wide_as_their_text() => new[] { "Name", "Type", "Description", "Remarks" }.Select(CountOfLinesOf).Must().SequenceEqual([1, 1, 1, 1]);
   [XFOnUIThread] public void has_its_wide_column_up_to_where_the_view_ends() => _view.BoundsOfEach("word").Max(it => it.Right).Must().BeGreaterThan(700);
}
