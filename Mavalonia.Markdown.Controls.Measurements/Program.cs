using System.Diagnostics;
using Mavalonia.Markdown.Controls.Measurements;

MeasuredApplication.Start();
WarmUp();
foreach(var count in args.Length > 0 ? args.Select(int.Parse) : [100, 1000]) Measure(count);

static void WarmUp()
{
   for(var time = 0; time < 5; time++)
   {
      var chat = new Chat(Messages.Create(60));
      chat.Draw();
      chat.Close();
   }
}

static void Measure(int count)
{
   var messages = Messages.Create(count);
   var memoryBefore = GC.GetTotalMemory(forceFullCollection: true);

   Chat? chat = null;
   var shown = Time(() => (chat = new Chat(messages)).ScrollTo(fromTheEnd: 0));
   var memory = GC.GetTotalMemory(forceFullCollection: true) - memoryBefore;

   var nothingNew = Enumerable.Range(1, 30).Select(_ => Time(chat!.Draw)).Order().ToList();
   var scrolling = Enumerable.Range(1, 60).Select(it => Time(() => chat!.ScrollTo(fromTheEnd: it * 300))).Order().ToList();
   chat!.ScrollTo(fromTheEnd: 0);

   var whole = string.Join("\n\n", messages.Take(4));
   var writing = Enumerable.Range(1, whole.Length / 100).Select(it => Time(() =>
   {
      chat.TextOfTheLast = whole[..(it * 100)];
      chat.Draw();
   })).Order().ToList();

   var narrower = Time(() =>
   {
      chat.Width = 700;
      chat.Draw();
   });

   Console.WriteLine($"{count} messages, {messages.Sum(it => it.Length) / 1000}k characters");
   Console.WriteLine($"   shown in {shown:0} ms, {memory / 1e6:0.0} MB");
   Console.WriteLine($"   a frame with nothing new: median {nothingNew[15]:0.0} ms");
   Console.WriteLine($"   a frame while scrolling: median {scrolling[30]:0.0} ms, longest {scrolling[^1]:0.0} ms");
   Console.WriteLine($"   a frame while the last is written: median {writing[writing.Count / 2]:0.0} ms, longest {writing[^1]:0.0} ms");
   Console.WriteLine($"   made narrower in {narrower:0} ms");
   chat.Close();
}

static double Time(Action action)
{
   var watch = Stopwatch.StartNew();
   action();
   return watch.Elapsed.TotalMilliseconds;
}
