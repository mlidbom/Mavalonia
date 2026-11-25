using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.Laying_out_what_is_read;

public class Code_in_colours
{
   static readonly string LongLines = string.Join('\n', Enumerable.Range(0, 6).Select(it => $"if(some.Value{it}(x=>x.Other.Member(\"a string {it} with words\",42)).ToString().Length>0&&other!=null){{return(some+other-{it}*3)/some.Thing[{it}].Else?.Method<int,string>(a:1,b:2);}} // comment number {it} goes on"));

   [XFOnUIThread] public async Task is_as_high_as_without_them_where_its_lines_are_wrapped()
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

   [XFOnUIThread] public async Task keeps_them_where_there_are_many_hundreds_of_blocks_of_it()
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

   [XFOnUIThread] public async Task gets_them_without_waiting_for_those_of_other_code_as_it_was_while_it_was_written()
   {
      var written = new MarkdownView { FontSize = 60 };
      var window = new HeadlessWindow(written);
      var code = "";
      for(var update = 0; update < 40; update++)
      {
         code += LinesOfUpdate(update);
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

   static string LinesOfUpdate(int update) => string.Concat(Enumerable.Range(0, 10).Select(it => $"   if(some.Value{it}(it => it.Other, \"a string\") > 3) return {update}; // comment\n"));

   static StackPanel PanelOf(IEnumerable<Control> children)
   {
      var panel = new StackPanel();
      panel.Children.AddRange(children);
      return panel;
   }
}
