using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

public class Laying_out_what_is_read
{
   static readonly string LongText = string.Join(' ', Enumerable.Range(0, 60).Select(it => $"word{it}"));

   [XFOnUIThread] public void shows_the_same_long_text_at_each_of_the_widths_that_it_is_at() =>
      new ViewInAWindow($"{LongText}\n\n- {LongText}\n\n> {LongText}\n\n> - {LongText}\n\n- - {LongText}").BoundsOfEach("word0").Select(it => it.Width).Must().Satisfy(it => it.Count() == 5 && it.All(width => width > 20));

   [XFOnUIThread] public void shows_the_same_long_text_in_each_of_the_columns_of_a_table_that_it_is_in() =>
      new ViewInAWindow($"| a | b | c |\n|---|---|---|\n| {LongText} | {LongText} | {LongText} |\n| {LongText} {LongText} | a | {LongText} {LongText} {LongText} |").BoundsOfEach("word0").Select(it => it.Width).Must().Satisfy(it => it.Count() == 8 && it.All(width => width > 20));
}
