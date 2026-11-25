using Markdig.Syntax;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;
using CodeBlock = Mavalonia.MarkdownEditing.Rendering.Blocks.CodeBlock;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Reads a block of code.</summary>
///<remarks>
/// Where each line of code starts is worked out here, since Markdig takes too many of the spaces away that code in a list item starts with.
/// </remarks>
class CodeBlockReader
{
   static readonly TextFormatting Formatting = new(TextFormats.Code);

   readonly string _markdown;
   readonly BlockContainers _containers;

   internal CodeBlockReader(string markdown, BlockContainers containers)
   {
      _markdown = markdown;
      _containers = containers;
   }

   internal CodeBlock Read(FencedCodeBlock code)
   {
      var lines = LinesOf(code).ToList();
      var text = lines.Count == 0 ? RenderedText.EmptyAt(_markdown.EndOfLineAt(code.Span.Start)) : TextOf(lines);
      var source = new SourceRange(code.Span.Start, Math.Clamp(code.Span.End + 1, text.Source.End, _markdown.Length)); // Markdig takes code that no fence ends to go on after the end of the document.

      return new CodeBlock(_containers, text, source, code.Info ?? "", hasNoLine: lines.Count == 0);
   }

   IEnumerable<SourceRange> LinesOf(FencedCodeBlock code)
   {
      var indentation = IndentationOf(code);
      var startOfLine = code.Span.Start;

      for(var number = 0; number < code.Lines.Count || IsTheEmptyLastLineThatMarkdigLeavesOut(); number++)
      {
         startOfLine = _markdown.StartOfLineAfter(startOfLine)!.Value;
         yield return new SourceRange(StartOfCode(startOfLine, indentation), _markdown.EndOfLineAt(startOfLine));
      }

      // The last line of the document, in code that no fence ends.
      bool IsTheEmptyLastLineThatMarkdigLeavesOut() => code.ClosingFencedCharCount == 0 && _containers.Count == 0 && _markdown.StartOfLineAfter(startOfLine) == _markdown.Length;
   }

   ///<summary>How many spaces the opening fence is indented by. Each line of code may be indented by as many, which aren't part of the code.</summary>
   int IndentationOf(FencedCodeBlock code)
   {
      var endOfPrefix = _containers.EndOfTheirPrefixes(_markdown, _markdown.StartOfLineAt(code.Span.Start));
      return _markdown.AsSpan(endOfPrefix..code.Span.Start).IsWhiteSpaceWithoutLineBreak() ? code.Span.Start - endOfPrefix : 0;
   }

   int StartOfCode(int startOfLine, int indentation)
   {
      var endOfLine = _markdown.EndOfLineAt(startOfLine);
      var endOfPrefix = _containers.EndOfTheirPrefixes(_markdown, startOfLine);
      var isBlankLineWithoutThePrefix = _containers.OfTheLineAt(_markdown, startOfLine).Count < _containers.Count;
      if(isBlankLineWithoutThePrefix) return endOfLine;

      return Math.Min(_markdown.SkipSpaces(endOfPrefix), Math.Min(endOfPrefix + indentation, endOfLine));
   }

   RenderedText TextOf(IReadOnlyList<SourceRange> lines)
   {
      var chars = lines.SelectMany((line, index) => index == 0 ? CharsOf(line) : CharsOf(line).Prepend(LineBreakBetween(lines[index - 1], line)));

      return new RenderedText([..chars], [], new SourceRange(lines[0].Start, lines[^1].End));
   }

   IEnumerable<RenderedChar> CharsOf(SourceRange line) =>
      Enumerable.Range(line.Start, line.Length).Select(it => new RenderedChar(_markdown[it], new SourceRange(it, it + 1), Formatting));

   static RenderedChar LineBreakBetween(SourceRange line, SourceRange next) => new('\n', new SourceRange(line.End, next.Start), Formatting);
}
