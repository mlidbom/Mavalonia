using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Text that is shown just as it is written, line by line.</summary>
static class TextOfLines
{
   ///<summary>The text of <paramref name="lines"/>, of which there must be at least one. What is between two lines stands for the line break.</summary>
   internal static RenderedText Read(string markdown, IReadOnlyList<SourceRange> lines, TextFormatting formatting)
   {
      var chars = lines.SelectMany((line, index) => index == 0 ? CharsOf(line) : CharsOf(line).Prepend(LineBreakBetween(lines[index - 1], line)));
      return new RenderedText([..chars], [], new SourceRange(lines[0].Start, lines[^1].End));

      IEnumerable<RenderedChar> CharsOf(SourceRange line) => Enumerable.Range(line.Start, line.Length).Select(it => new RenderedChar(markdown[it], new SourceRange(it, it + 1), formatting));
      RenderedChar LineBreakBetween(SourceRange line, SourceRange next) => new('\n', new SourceRange(line.End, next.Start), formatting);
   }
}
