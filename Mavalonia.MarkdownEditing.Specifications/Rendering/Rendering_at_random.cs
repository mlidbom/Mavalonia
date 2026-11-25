using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Specifications.Rendering;

///<summary>Renders markdown that nobody would write. It is the same each time the specification runs.</summary>
public class Rendering_at_random
{
   static readonly string[] Pieces = ["a", "b c", " ", "  ", "\n", "\n", "\n\n", "\r\n", "\t", "*", "**", "_", "`", "```", "~~", "[", "]", "(", ")", "](", "![", "<", ">", "&", "&amp;", "\\", "|", "|", "-", "---", "|-|", "#", "1.", "+", "=", ":", "!", "é", "👩‍👩‍👧"];

   static string MarkdownOf(int seed)
   {
      var random = new Random(seed);
      return string.Concat(Enumerable.Range(0, random.Next(1, 14)).Select(_ => Pieces[random.Next(Pieces.Length)]));
   }

   [XF] public void never_fails_and_renders_at_least_one_block() => Enumerable.Range(0, 40_000).Select(MarkdownOf).Where(it => RenderedMarkdown.Of(it).Blocks.Count == 0).Must().BeEmpty();
}
