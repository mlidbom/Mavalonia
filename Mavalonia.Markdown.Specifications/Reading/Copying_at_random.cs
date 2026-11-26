using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.Markdown.Editing;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Specifications.Reading;

///<summary>Copies out of documents that are the same each time the specification runs.</summary>
public class Copying_at_random
{
   const int CountOfDocuments = 6000;

   static readonly string[] Pieces = ["a", "b c", " ", "  ", "\n", "\n\n", "\r\n", "\t", "*", "**", "_", "`", "```", "~~", "[a](b)", "![a](b)", "<br>", "&amp;", "\\", "|", "|-|", "| a | b |\n|---|---|\n| 1 | 2 |\n", "---", "# ", "- ", "- [ ] ", "1. ", "> ", "> [!NOTE]\n> ", "    ", "\n====\n", "https://x.y", "é", "👩‍👩‍👧"];

   [XF] public void any_selection_in_markdown_that_nobody_would_write_never_fails()
   {
      for(var seed = 0; seed < CountOfDocuments; seed++)
      {
         var random = new Random(seed);
         var markdown = string.Concat(Enumerable.Range(0, random.Next(1, 14)).Select(_ => Pieces[random.Next(Pieces.Length)]));
         var read = new MarkdownToRead(markdown, MarkdownDialect.ForReading);
         read.Select(new TextSelection(random.Next(markdown.Length + 1), random.Next(markdown.Length + 1)));

         _ = read.SelectedMarkdown + read.SelectedHtml;
      }
   }

   [XF] public void all_gives_markdown_that_shows_the_same()
   {
      for(var seed = 0; seed < CountOfDocuments; seed++)
      {
         var markdown = string.Join("\n\n", Blocks(new Random(seed), depth: 0, atLeast: 2));
         var read = new MarkdownToRead(markdown, MarkdownDialect.ForReading);
         read.SelectAll();

         new { markdown, Copied = read.SelectedMarkdown }.Must().Satisfy(it => Rendered(it.Copied).SequenceEqual(Rendered(it.markdown)));
      }
   }

   [XF] public void from_the_start_of_a_block_to_the_end_of_another_gives_markdown_that_shows_the_same_text()
   {
      for(var seed = 0; seed < CountOfDocuments; seed++)
      {
         var random = new Random(seed);
         var markdown = string.Join("\n\n", Blocks(random, depth: 0, atLeast: 3));
         var read = new MarkdownToRead(markdown, MarkdownDialect.ForReading);
         var blocks = read.Rendered.Blocks;
         var first = random.Next(blocks.Count - 1);
         var last = random.Next(first + 1, blocks.Count);
         if(blocks[first] is TableCell || blocks[last] is TableCell) continue; // A table is copied as a whole.

         read.MoveTo(RenderedPosition.AtTheStartOf(blocks[first]));
         read.MoveTo(RenderedPosition.AtTheEndOf(blocks[last]), extendSelection: true);
         var selected = string.Join('\n', blocks.Skip(first).Take(last - first + 1).Select(it => it.Text.Value));
         new { markdown, Copied = read.SelectedMarkdown, selected }.Must().Satisfy(it => RenderedMarkdown.Of(it.Copied, MarkdownDialect.ForReading).PlainText.Value == it.selected);
      }
   }

   static IReadOnlyList<string> Rendered(string markdown) => [..RenderedMarkdown.Of(markdown, MarkdownDialect.ForReading).Blocks.Select(it => $"{it.GetType().Name} {it.Containers.OutermostFirst.Count} {it.Text.Value}")];

   static IReadOnlyList<string> Blocks(Random random, int depth, int atLeast = 1) => [..Enumerable.Range(0, random.Next(atLeast, atLeast + 3)).Select(_ => Block(random, depth))];

   static string Block(Random random, int depth) => random.Next(depth < 3 ? 13 : 5) switch
   {
      0 => new string('#', random.Next(1, 4)) + " " + Text(random),
      1 => "```cs\n" + Word(random) + "\n\n   " + Word(random) + "\n```",
      2 => $"| {Word(random)} | {Word(random)} |\n|---|---|\n| {Word(random)} | {Text(random)} |",
      5 => Indented("- ", random, depth) + "\n" + Indented("- ", random, depth),
      6 => Indented("- [x] " + Word(random) + "\n\n", random, depth),
      7 => Indented("7. ", random, depth),
      8 => Quoted(string.Join("\n\n", Blocks(random, depth + 1))),
      9 => Text(random) + "\n===",
      10 => "# " + Word(random) + "\n\n    " + Word(random) + "\n\n      " + Word(random),
      11 => "![d e](f.png) " + Text(random) + "<br>" + Text(random),
      12 when depth == 0 => Quoted("[!TIP]\n" + Text(random)),
      _ => Text(random) + "\n" + Text(random)
   };

   ///<summary>An item whose first line starts with <paramref name="start"/>, which may have other lines after it.</summary>
   static string Indented(string start, Random random, int depth)
   {
      var width = start.StartsWith("7.", StringComparison.Ordinal) ? 3 : 2;
      var lines = (start[width..] + string.Join("\n\n", Blocks(random, depth + 1))).Split('\n');
      return start[..width] + string.Join('\n', lines.Select((line, index) => index == 0 || line.Length == 0 ? line : new string(' ', width) + line));
   }

   static string Quoted(string markdown) => string.Join('\n', markdown.Split('\n').Select(it => it.Length == 0 ? ">" : "> " + it));

   static string Text(Random random) => string.Join(' ', Enumerable.Range(0, random.Next(1, 5)).Select(_ => Word(random)));

   static string Word(Random random) => random.Next(8) switch
   {
      0 => "**bold**",
      1 => "*it*",
      2 => "`c|d`".Replace("|", ""),
      3 => "[l](m)",
      4 => "https://x.y",
      5 => "~~s~~",
      _ => "word"
   };
}
