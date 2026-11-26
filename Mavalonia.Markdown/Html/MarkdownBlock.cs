using System.Globalization;
using System.Text.RegularExpressions;
using Mavalonia.Markdown.Editing.Writing;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Html;

///<summary>The lines of a block of markdown that is being written, which may have other blocks in it.</summary>
partial class MarkdownBlock
{
   readonly IReadOnlyList<string> _lines;
   readonly bool _isAList;

   MarkdownBlock(IReadOnlyList<string> lines, bool isCode = false, bool isAList = false)
   {
      _lines = lines;
      IsCode = isCode;
      _isAList = isAList;
   }

   internal bool IsCode { get; }

   internal static MarkdownBlock Paragraph(FormattedText text) => new(MarkdownOfText.InAParagraph(text).Split('\n'));

   internal static MarkdownBlock Heading(int level, FormattedText text) => new([MarkdownOfText.InAHeading(level, text)]);

   internal static MarkdownBlock ThematicBreak { get; } = new(["***"]); // Dashes after the dash of a list item would be a thematic break together with it.

   internal static MarkdownBlock Code(string code, string language)
   {
      var fence = FenceLongerThanAnyIn(code);
      return new MarkdownBlock([fence + language, ..code.ReplaceLineEndings("\n").Split('\n'), fence], isCode: true);
   }

   static string FenceLongerThanAnyIn(string code)
   {
      var longestRunOfBackticks = RunOfBackticks().Matches(code).Select(it => it.Length).DefaultIfEmpty(0).Max();
      return new string(CodeBlock.Fence[0], Math.Max(CodeBlock.Fence.Length, longestRunOfBackticks + 1));
   }

   internal static MarkdownBlock Table(IReadOnlyList<IReadOnlyList<FormattedText>> rows) => new(new MarkdownOfATable(rows).Lines);

   internal static MarkdownBlock Quote(IReadOnlyList<MarkdownBlock> blocks) => new([..LinesOf(blocks).Select(it => it.Length == 0 ? ">" : "> " + it)]);

   internal static MarkdownBlock BulletedList(IReadOnlyList<IReadOnlyList<MarkdownBlock>> items) => new([..items.SelectMany(it => LinesOfAnItem("- ", it))], isAList: true);

   internal static MarkdownBlock NumberedList(IReadOnlyList<IReadOnlyList<MarkdownBlock>> items, int firstNumber) =>
      new([..items.SelectMany((it, index) => LinesOfAnItem((firstNumber + index).ToString(CultureInfo.InvariantCulture) + ". ", it))], isAList: true);

   static IEnumerable<string> LinesOfAnItem(string marker, IReadOnlyList<MarkdownBlock> blocks) =>
      LinesInsideAnItem(blocks).Select((line, index) => index == 0 ? marker + line : line.Length == 0 ? "" : new string(' ', marker.Length) + line);

   ///<summary>A list inside an item is on the line after the item's text, when that is shown the same as with a blank line between them.</summary>
   static IReadOnlyList<string> LinesInsideAnItem(IReadOnlyList<MarkdownBlock> blocks)
   {
      var lines = LinesOf(blocks);
      if(!blocks.Skip(1).Any(it => it._isAList)) return lines;

      IReadOnlyList<string> withoutBlankLinesBeforeLists = [..blocks.SelectMany((block, index) => index == 0 || block._isAList ? block._lines : block._lines.Prepend(""))];
      return AreShownTheSame(lines, withoutBlankLinesBeforeLists) ? withoutBlankLinesBeforeLists : lines;
   }

   static bool AreShownTheSame(IReadOnlyList<string> some, IReadOnlyList<string> others)
   {
      var (blocks, otherBlocks) = (BlocksShownOf(some), BlocksShownOf(others));
      return blocks.Count == otherBlocks.Count && blocks.Zip(otherBlocks).All(it => it.First.LooksLike(it.Second));
   }

   static IReadOnlyList<RenderedBlock> BlocksShownOf(IReadOnlyList<string> lines) => RenderedMarkdown.Of(string.Join('\n', lines)).Blocks;

   ///<summary>The lines of all the blocks, with blank lines between them.</summary>
   internal static IReadOnlyList<string> LinesOf(IReadOnlyList<MarkdownBlock> blocks) => [..blocks.SelectMany((block, index) => index == 0 ? block._lines : block._lines.Prepend(""))];

   [GeneratedRegex("`+")]
   private static partial Regex RunOfBackticks();
}
