using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits.NewBlocks;

///<summary>Starts a new line of code, or, from an empty line at the end of the code, a paragraph after it.</summary>
class StartingANewLineOfCode
{
   readonly Caret _caret;
   readonly CodeBlock _code;

   internal StartingANewLineOfCode(Caret caret, CodeBlock code)
   {
      _caret = caret;
      _code = code;
   }

   RenderedMarkdown Rendered => _caret.Rendered;

   internal SourceChange Change() => IsAtAnEmptyLineAtTheEnd ? LeaveTheCode() : SourceChange.Insert(Rendered, _caret.Position.Offset, LineBreaks.NewLine);

   LineBreaksInside LineBreaks => new(Rendered, _code.Containers);

   bool IsAtAnEmptyLineAtTheEnd => _caret.IsAtTheEndOfItsBlock && _code.Text.Value.EndsWith('\n');

   SourceChange LeaveTheCode()
   {
      var endOfCodeWithoutTheEmptyLine = _code.Text.Chars[^1].Source.Start;
      return new GoingToAnEmptyParagraphAfter(Rendered, _code).AddAfterReplacing(new SourceRange(endOfCodeWithoutTheEmptyLine, _code.Source.End), by: EndOfTheBlock());
   }

   string EndOfTheBlock()
   {
      var afterTheCode = Rendered.Source[_code.Text.Source.End.._code.Source.End];
      return string.IsNullOrWhiteSpace(afterTheCode) ? LineBreaks.NewLine + CodeBlock.Fence : afterTheCode;
   }
}
