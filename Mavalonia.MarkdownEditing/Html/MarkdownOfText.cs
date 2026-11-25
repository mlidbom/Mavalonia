using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Html;

///<summary>Writes text that isn't markdown as markdown that shows it. What looks like markup in it must not become that.</summary>
///<remarks>
/// Most of what could be markup isn't where it is, such as the asterisk of <c>2 * 3</c>, and looks better unescaped. So as little as possible is escaped, and what comes of it is verified by rendering it.
/// </remarks>
static class MarkdownOfText
{
   internal static string InAParagraph(FormattedText text) => FirstWayThatLooksRight(text, it => it, it => it is Paragraph);

   internal static string InAHeading(int level, FormattedText text) => FirstWayThatLooksRight(text, it => new string('#', level) + " " + it, it => it is Heading heading && heading.Level == level);

   internal static string InACell(FormattedText text)
   {
      const string delimiterRow = "\n|---|";
      return FirstWayThatLooksRight(text, it => $"| {it} |{delimiterRow}", it => it is TableCell)[2..^(2 + delimiterRow.Length)];
   }

   ///<summary>Some formatting has no markdown that shows it, such as that of the quoted word of <c>a"b"c</c>. Then the text is written without any.</summary>
   static string FirstWayThatLooksRight(FormattedText text, Func<string, string> inTheBlock, Func<RenderedBlock, bool> isTheBlock)
   {
      return WaysThatLookRight(text).FirstOrDefault() ?? WaysThatLookRight(text.WithoutFormatting).FirstOrDefault() ?? inTheBlock(text.WithoutFormatting.WithPunctuationEscaped.MarkdownWithoutFormatting);

      IEnumerable<string> WaysThatLookRight(FormattedText toWrite) => WaysToWrite(toWrite).Select(inTheBlock).Where(it => LooksRight(it, toWrite));

      bool LooksRight(string markdown, FormattedText written) => RenderedMarkdown.Of(markdown).Blocks is [{ Containers.Count: 0 } block] && isTheBlock(block) && written.LooksLike(block.Text);
   }

   static IEnumerable<string> WaysToWrite(FormattedText text) => WaysToEscape(text).SelectMany(InlineMarkdownWriter.WaysToWrite);

   static IEnumerable<FormattedText> WaysToEscape(FormattedText text)
   {
      yield return text;
      yield return text.WithTheStartOfItsLinesEscaped;
      yield return text.WithMarkupEscaped;
      yield return text.WithMarkupEscaped.WithTheStartOfItsLinesEscaped;
      yield return text.WithPunctuationEscaped;
   }
}
