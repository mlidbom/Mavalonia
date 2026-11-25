namespace Mavalonia.MarkdownEditing.Controls.Measurements;

///<summary>Messages such as an AI writes. They are the same each time.</summary>
static class Messages
{
   static readonly string[] Words = "the editor shows what is selected as markdown and copies it with its markup when a person reads a long message that has been written by somebody else about code tables lists and other things which they want to quote".Split(' ');

   internal static IReadOnlyList<string> Create(int count)
   {
      var random = new Random(1);
      return [..Enumerable.Range(0, count).Select(_ => string.Join("\n\n", Enumerable.Range(0, random.Next(1, 9)).Select(_ => Block(random))))];
   }

   static string Block(Random random) => random.Next(10) switch
   {
      0 => $"## {Text(random, 4)}",
      1 or 2 => string.Join('\n', Enumerable.Range(0, random.Next(2, 6)).Select(_ => $"- {Text(random, 12)}")),
      3 => string.Join('\n', Enumerable.Range(1, 3).Select(it => $"{it}. **{Text(random, 2)}:** {Text(random, 8)}")),
      4 => $"```csharp\npublic static int {Word(random)}(string {Word(random)}) // {Text(random, 5)}\n{{\n   return \"{Word(random)}\".Length + {random.Next(100)};\n}}\n```",
      5 => $"| {Word(random)} | {Word(random)} |\n|---|---|\n| `{Word(random)}` | {Text(random, 4)} |\n| `{Word(random)}` | {Text(random, 9)} |",
      6 => $"> {Text(random, 14)}",
      _ => $"{Text(random, 20)}, **{Text(random, 2)}** and `{Word(random)}`, see [{Text(random, 2)}](https://example.org/{Word(random)}). {Text(random, 15)}."
   };

   static string Text(Random random, int words) => string.Join(' ', Enumerable.Range(0, words).Select(_ => Word(random)));
   static string Word(Random random) => Words[random.Next(Words.Length)];
}
