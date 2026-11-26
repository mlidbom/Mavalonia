using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.Markdown.Controls.Specifications.Laying_out_what_is_read;

public class Text_that_is_wrapped
{
   static readonly string LongText = string.Join(' ', Enumerable.Range(0, 60).Select(it => $"word{it}"));

   [XFOnUIThread] public void is_shown_at_each_of_the_widths_that_it_is_at() =>
      new ViewInAWindow($"{LongText}\n\n- {LongText}\n\n> {LongText}\n\n> - {LongText}\n\n- - {LongText}").BoundsOfEach("word0").Select(it => it.Width).ToList().Must().Satisfy(it => it.Count == 5 && it.All(width => width > 20));

   [XFOnUIThread] public void is_shown_in_each_of_the_columns_of_a_table_that_it_is_in() =>
      new ViewInAWindow($"| a | b | c |\n|---|---|---|\n| {LongText} | {LongText} | {LongText} |\n| {LongText} {LongText} | a | {LongText} {LongText} {LongText} |").BoundsOfEach("word0").Select(it => it.Width).ToList().Must().Satisfy(it => it.Count == 8 && it.All(width => width > 20));
}
