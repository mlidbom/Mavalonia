using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

///<summary>Deletes code.</summary>
class DeletingCode
{
   readonly RenderedMarkdown _rendered;
   readonly CodeBlock _code;

   internal DeletingCode(RenderedMarkdown rendered, CodeBlock code)
   {
      _rendered = rendered;
      _code = code;
   }

   internal SourceChange Between(int start, int end)
   {
      var range = new SourceRange(_code.Text.OffsetAt(start), _code.Text.OffsetAt(end));
      var startOfLine = _rendered.Source.StartOfLineAt(range.Start);
      var prefix = _code.Containers.PrefixOfTheirLines;

      // A blank line of code in a list item usually lacks the indentation that it must have once there is code on it.
      return IsCodeLeftAfter(range.End) && _rendered.Source.AsSpan(startOfLine..range.Start).IsWhiteSpaceWithoutLineBreak() && range.Start - startOfLine < prefix.Length
                ? SourceChange.Replace(_rendered, range with { Start = startOfLine }, prefix)
                : SourceChange.Remove(_rendered, range);
   }

   bool IsCodeLeftAfter(int offset) => offset < _rendered.Source.EndOfLineAt(offset);
}
