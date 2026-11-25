using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

public class Laying_out_what_is_read
{
   static readonly string LongText = string.Join(' ', Enumerable.Range(0, 60).Select(it => $"word{it}"));

   [XFOnUIThread] public void shows_the_same_long_text_at_each_of_the_widths_that_it_is_at() =>
      new ViewInAWindow($"{LongText}\n\n- {LongText}\n\n> {LongText}\n\n> - {LongText}\n\n- - {LongText}").BoundsOfEach("word0").Select(it => it.Width).Must().Satisfy(it => it.Count() == 5 && it.All(width => width > 20));

   [XFOnUIThread] public void shows_the_same_long_text_in_each_of_the_columns_of_a_table_that_it_is_in() =>
      new ViewInAWindow($"| a | b | c |\n|---|---|---|\n| {LongText} | {LongText} | {LongText} |\n| {LongText} {LongText} | a | {LongText} {LongText} {LongText} |").BoundsOfEach("word0").Select(it => it.Width).Must().Satisfy(it => it.Count() == 8 && it.All(width => width > 20));

   [XFOnUIThread] public void says_where_text_is_also_before_it_is_shown() => new MarkdownView { Markdown = "a" }.BoundsOf(new PlainTextRange(0, 1)).Must().HaveCount(1);

   public class A_table_that_is_too_wide : Laying_out_what_is_read
   {
      readonly ViewInAWindow _view = new($"| Name | Type | What it is for |\n|---|---|---|\n| Description | string | {LongText} {LongText} |\n| Remarks | string | {LongText} |");

      int CountOfLinesOf(string text) => _view.View.BoundsOf(new PlainTextRange(_view.View.PlainText.Value.IndexOf(text, StringComparison.Ordinal), text.Length)).Count;

      [XFOnUIThread] public void has_the_narrow_columns_as_wide_as_their_text() => new[] { "Name", "Type", "Description", "Remarks" }.Select(CountOfLinesOf).Must().SequenceEqual([1, 1, 1, 1]);
      [XFOnUIThread] public void has_its_wide_column_up_to_where_the_view_ends() => _view.BoundsOfEach("word").Max(it => it.Right).Must().BeGreaterThan(700);
   }

   public class A_list_item_with_a_number_of_many_digits : Laying_out_what_is_read
   {
      [XFOnUIThread] public void has_room_for_the_number_before_its_text() => new ViewInAWindow("10000. a").BoundsOf("a").Left.Must().BeGreaterThan(10 + new ViewInAWindow("10000\\. b").BoundsOf("10000.").Width);
   }

   public class Code : Laying_out_what_is_read
   {
      static readonly string LongLines = string.Join('\n', Enumerable.Range(0, 6).Select(it => $"if(some.Value{it}(x=>x.Other.Member(\"a string {it} with words\",42)).ToString().Length>0&&other!=null){{return(some+other-{it}*3)/some.Thing[{it}].Else?.Method<int,string>(a:1,b:2);}} // comment number {it} goes on"));

      [XFOnUIThread] public async Task is_as_high_in_its_colours_as_without_them_where_its_lines_are_wrapped()
      {
         var views = Enumerable.Range(0, 24).Select(it => new MarkdownView { Markdown = $"```cs\n{LongLines}\n```", Width = 150 + it * 23 }).ToList();
         var window = new HeadlessWindow(PanelOf(views));
         var heightsWithoutColours = views.Select(it => it.DesiredSize.Height).ToList();
         var withoutColours = window.RenderFrame().Fingerprint;
         for(var time = 0; time < 400 && window.RenderFrame().Fingerprint == withoutColours; time++) await Task.Delay(10);
         await Task.Delay(200);

         foreach(var view in views) view.InvalidateMeasure();
         window.RenderFrame();
         views.Select(it => it.DesiredSize.Height).Must().SequenceEqual(heightsWithoutColours);
      }

      [XFOnUIThread] public async Task keeps_its_colours_where_there_are_many_hundreds_of_blocks_of_it()
      {
         var view = new ViewInAWindow(string.Concat(Enumerable.Range(0, 450).Select(it => $"```cs\nint a{it};\n```\n\n")));
         await Task.Delay(1500);
         view.Window.RenderFrame();

         var frames = new List<string>();
         for(var time = 0; time < 25; time++)
         {
            frames.Add(view.Window.RenderFrame().Fingerprint);
            await Task.Delay(20);
         }

         frames.Distinct().Must().HaveCount(1);
      }

      [XFOnUIThread] public async Task gets_its_colours_without_waiting_for_those_of_code_that_was_there_while_it_was_written()
      {
         var written = new MarkdownView { FontSize = 60 };
         var window = new HeadlessWindow(written);
         var code = "";
         for(var update = 0; update < 40; update++)
         {
            code += string.Concat(Enumerable.Range(0, 10).Select(it => $"   if(some.Value{it}(it => it.Other, \"a string\") > 3) return {update}; // comment\n"));
            written.Markdown = $"```cs\n{code}```";
            window.MoveThePointerTo(new Point(700, 500 + update % 2));
            await Task.Delay(1);
         }

         var other = new ViewInAWindow("```cs\nint other;\n```");
         var withoutColours = other.Window.RenderFrame().Fingerprint;
         var watch = Stopwatch.StartNew();
         while(other.Window.RenderFrame().Fingerprint == withoutColours && watch.ElapsedMilliseconds < 10_000) await Task.Delay(10);
         watch.ElapsedMilliseconds.Must().BeLessThan(2500);
      }

      static StackPanel PanelOf(IEnumerable<Control> children)
      {
         var panel = new StackPanel();
         panel.Children.AddRange(children);
         return panel;
      }
   }
}
