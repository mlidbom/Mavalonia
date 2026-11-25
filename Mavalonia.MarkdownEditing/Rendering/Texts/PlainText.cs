using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Rendering.Texts;

///<summary>All the text that is shown, in one, such as to search in: that of each block on lines of its own.</summary>
public sealed class PlainText
{
   readonly IReadOnlyList<RenderedBlock> _blocks;
   readonly IReadOnlyList<int> _starts;

   internal PlainText(IReadOnlyList<RenderedBlock> blocks)
   {
      _blocks = blocks;
      _starts = [..StartsOf(blocks)];
      Value = string.Join('\n', blocks.Select(it => it.Text.Value));
   }

   static IEnumerable<int> StartsOf(IEnumerable<RenderedBlock> blocks)
   {
      var start = 0;
      foreach(var block in blocks)
      {
         yield return start;
         start += block.Text.Length + 1;
      }
   }

   public string Value { get; }

   ///<summary>The parts of <paramref name="range"/> that are in each block, as far as there is any text there.</summary>
   public IEnumerable<PartOfABlock> PartsOf(PlainTextRange range) =>
      _blocks.Zip(_starts, (block, start) => new PartOfABlock(block, Math.Max(0, range.Start - start), Math.Min(block.Text.Length, range.End - start)))
             .Where(it => it.Start < it.End);
}
