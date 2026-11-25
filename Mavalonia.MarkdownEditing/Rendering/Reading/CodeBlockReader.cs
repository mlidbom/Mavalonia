using Markdig.Syntax;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;
using CodeBlock = Mavalonia.MarkdownEditing.Rendering.Blocks.CodeBlock;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Reads a block of code.</summary>
class CodeBlockReader
{
   readonly string _markdown;
   readonly BlockContainers _containers;

   internal CodeBlockReader(string markdown, BlockContainers containers)
   {
      _markdown = markdown;
      _containers = containers;
   }

   static readonly TextFormatting Formatting = new(TextFormats.Code);

   internal CodeBlock Read(FencedCodeBlock code)
   {
      var lines = LinesOf(code);
      var text = lines.Count == 0 ? RenderedText.EmptyAt(_markdown.EndOfLineAt(code.Span.Start)) : TextOf(lines);
      var source = new SourceRange(code.Span.Start, Math.Max(code.Span.End + 1, text.Source.End));

      return new CodeBlock(_containers, text, source, code.Info ?? "", hasNoLine: lines.Count == 0);
   }

   static IReadOnlyList<SourceRange> LinesOf(FencedCodeBlock code) =>
      code.Lines.Count == 0 ? [] : [..code.Lines.Lines.Take(code.Lines.Count).Select(it => new SourceRange(it.Slice.Start, Math.Max(it.Slice.Start, it.Slice.End + 1)))];

   RenderedText TextOf(IReadOnlyList<SourceRange> lines)
   {
      var characters = lines.SelectMany((line, index) => index == 0 ? CharactersOf(line) : CharactersOf(line).Prepend(LineBreakBetween(lines[index - 1], line)));

      return new RenderedText([..characters], [], new SourceRange(lines[0].Start, lines[^1].End));
   }

   IEnumerable<RenderedCharacter> CharactersOf(SourceRange line) =>
      Enumerable.Range(line.Start, line.Length).Select(it => new RenderedCharacter(_markdown[it], new SourceRange(it, it + 1), Formatting));

   static RenderedCharacter LineBreakBetween(SourceRange line, SourceRange next) => new('\n', new SourceRange(line.End, next.Start), Formatting);
}
