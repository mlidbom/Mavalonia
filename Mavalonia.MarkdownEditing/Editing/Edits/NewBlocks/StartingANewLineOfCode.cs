using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits.NewBlocks;

///<summary>Starts a new line of code, or, from an empty line at the end of the code, a paragraph after it.</summary>
class StartingANewLineOfCode
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedPosition _position;
   readonly CodeBlock _code;

   internal StartingANewLineOfCode(RenderedMarkdown rendered, RenderedPosition position, CodeBlock code)
   {
      _rendered = rendered;
      _position = position;
      _code = code;
   }

   internal SourceChange Change() => IsAtAnEmptyLineAtTheEnd ? LeaveTheCode() : SourceChange.Insert(_rendered, _position.Offset, LineBreaks.NewLine);

   LineBreaksInside LineBreaks => new(_rendered, _code.Containers);

   bool IsAtAnEmptyLineAtTheEnd => _position.IsAtTheEndOfItsBlock && _code.Text.Value.EndsWith('\n');

   SourceChange LeaveTheCode()
   {
      var endOfCodeWithoutTheEmptyLine = _code.Text.Characters[^1].Source.Start;
      return new GoingToAnEmptyParagraphAfter(_rendered, _code).AddAfterReplacing(new SourceRange(endOfCodeWithoutTheEmptyLine, _code.Source.End), by: EndOfTheBlock());
   }

   string EndOfTheBlock()
   {
      var afterTheCode = _rendered.Source[_code.Text.Source.End.._code.Source.End];
      return string.IsNullOrWhiteSpace(afterTheCode) ? LineBreaks.NewLine + CodeBlock.Fence : afterTheCode;
   }
}
