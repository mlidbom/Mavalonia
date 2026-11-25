using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Specifications.Rendering;

///<summary>Renders markdown that nobody would write. It is the same each time the specification runs.</summary>
public class Rendering_at_random
{
   static readonly string[] Pieces = ["a", "b c", " ", "  ", "\n", "\n", "\n\n", "\r\n", "\t", "*", "**", "_", "`", "```", "~~", "[", "]", "(", ")", "](", "![", "<", ">", "&", "&amp;", "\\", "|", "|", "-", "---", "|-|", "#", "1.", "+", "=", ":", "!", "![a](b)", "[ ]", "> [!NOTE]", "    ", "https://x.y", "<br>", "<!--", "-->", "é", "👩‍👩‍👧", "\0"];

   static string MarkdownOf(int seed)
   {
      var random = new Random(seed);
      return string.Concat(Enumerable.Range(0, random.Next(1, 14)).Select(_ => Pieces[random.Next(Pieces.Length)]));
   }

   [XF] public void never_fails_and_renders_at_least_one_block() => Enumerable.Range(0, 40_000).Select(MarkdownOf).Where(it => RenderedMarkdown.Of(it).Blocks.Count == 0).Must().BeEmpty();

   [XF] public void for_reading_never_fails_and_renders_at_least_one_block_in_the_order_of_its_source() =>
      Enumerable.Range(0, 60_000).Select(MarkdownOf).Where(it => !IsInOrder(RenderedMarkdown.Of(it, MarkdownDialect.ForReading))).Must().BeEmpty();

   static bool IsInOrder(RenderedMarkdown rendered) => rendered.Blocks.Count > 0 && rendered.Blocks.Zip(rendered.Blocks.Skip(1)).All(it => it.First.Text.Source.End <= it.Second.Text.Source.Start);
}
