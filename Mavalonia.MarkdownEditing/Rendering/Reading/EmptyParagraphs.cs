using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>The paragraphs that a person is about to write: the blank lines of a markdown text that aren't needed to separate its blocks.</summary>
///<remarks>
/// Two paragraphs need one blank line between them. With a blank line more on each side of it, a blank line can be written on<br/>
/// without becoming part of the paragraph above or below: so every other of the blank lines is an empty paragraph.
///</remarks>
class EmptyParagraphs
{
   readonly string _markdown;

   internal EmptyParagraphs(string markdown) => _markdown = markdown;

   internal IReadOnlyList<RenderedBlock> AddedTo(IReadOnlyList<RenderedBlock> blocks)
   {
      if(blocks.Count == 0) return [..EveryOther(LinesFrom(0), first: 0).Select(it => EmptyAt(it, BlockContainers.None))];

      List<RenderedBlock> all = [..Before(blocks[0])];
      foreach(var (block, next) in blocks.Zip(blocks.Skip(1)))
      {
         all.Add(block);
         all.AddRange(Between(block, next));
      }

      all.AddRange(LastBlockAndWhatIsAfterIt(blocks[^1]));
      return all;
   }

   IEnumerable<RenderedBlock> Before(RenderedBlock first)
   {
      var blankLines = LinesFrom(0).TakeWhile(it => it < _markdown.StartOfLineAt(first.Source.Start)).ToList();
      return EveryOther(blankLines, first: 0).Take(blankLines.Count / 2).Select(it => EmptyAt(it, first.Containers));
   }

   IEnumerable<RenderedBlock> Between(RenderedBlock block, RenderedBlock next)
   {
      var blankLines = LinesAfter(block).TakeWhile(it => it < _markdown.StartOfLineAt(next.Source.Start)).ToList();
      return EveryOther(blankLines, first: 1).Take(Math.Max(0, blankLines.Count - 1) / 2).Select(it => EmptyAt(it, block.Containers, next.Containers));
   }

   IEnumerable<RenderedBlock> LastBlockAndWhatIsAfterIt(RenderedBlock last)
   {
      var blankLines = LinesAfter(last).ToList();

      if(blankLines.Count != 1) return EveryOther(blankLines, first: 1).Select(it => EmptyAt(it, last.Containers)).Prepend(last);
      if(last is Paragraph paragraph) return [WithALineBreakAtItsEnd(paragraph)];
      return last.EndsWithItsLastLine ? [last, EmptyAt(blankLines[0], last.Containers)] : [last];
   }

   ///<summary>What is typed on the line after a paragraph continues the paragraph.</summary>
   Paragraph WithALineBreakAtItsEnd(Paragraph paragraph)
   {
      var text = paragraph.Text;
      var lineBreak = new RenderedChar('\n', new SourceRange(text.Source.End, _markdown.Length), TextFormatting.None);

      return new Paragraph(paragraph.Containers, new RenderedText([..text.Chars, lineBreak], text.Markups, text.Source with { End = _markdown.Length }));
   }

   ///<summary>An empty paragraph on the line that starts at <paramref name="startOfLine"/>. It is inside those containers of the blocks around it whose prefixes are on the line.</summary>
   Paragraph EmptyAt(int startOfLine, params IReadOnlyList<BlockContainers> containersOfTheBlocksAroundIt) =>
      Paragraph.EmptyAt(_markdown.EndOfLineAt(startOfLine), containersOfTheBlocksAroundIt.Select(it => it.OfTheLineAt(_markdown, startOfLine)).MaxBy(it => it.Count)!);

   static IEnumerable<int> EveryOther(IEnumerable<int> lines, int first) => lines.Where((_, index) => index >= first && (index - first) % 2 == 0);

   IEnumerable<int> LinesAfter(RenderedBlock block) => _markdown.StartOfLineAfter(block.Source.End) is {} startOfLine ? LinesFrom(startOfLine) : [];

   ///<summary>The offsets that the lines start at, from the line that starts at <paramref name="startOfFirst"/>.</summary>
   IEnumerable<int> LinesFrom(int startOfFirst)
   {
      for(int? startOfLine = startOfFirst; startOfLine is {} start; startOfLine = _markdown.StartOfLineAfter(start)) yield return start;
   }
}
