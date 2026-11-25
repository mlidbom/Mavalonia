using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

///<summary>Starts a new line of code, or, from an empty line at the end of the code, a paragraph after it.</summary>
class StartingANewLineOfCode
{
   const string Fence = "```";

   readonly RenderedMarkdown _rendered;
   readonly RenderedPosition _position;
   readonly CodeBlock _code;

   internal StartingANewLineOfCode(RenderedMarkdown rendered, RenderedPosition position, CodeBlock code)
   {
      _rendered = rendered;
      _position = position;
      _code = code;
   }

   internal SourceChange Change() => IsAtAnEmptyLineAtTheEnd ? LeaveTheCode() : SourceChange.Insert(_rendered, _position.Offset, new Separator(_rendered, _code.Containers).NewLine);

   bool IsAtAnEmptyLineAtTheEnd => _position.IsAtTheEndOfItsBlock && _code.Text.Value.EndsWith('\n');

   SourceChange LeaveTheCode()
   {
      var endOfCodeWithoutTheEmptyLine = _code.Text.Characters[^1].Source.Start;
      return new EmptyParagraphAfter(_rendered, _code).Insert(EndOfTheBlock(), new SourceRange(endOfCodeWithoutTheEmptyLine, _code.Source.End));
   }

   string EndOfTheBlock()
   {
      var afterTheCode = _rendered.Source[_code.Text.Source.End.._code.Source.End];
      return string.IsNullOrWhiteSpace(afterTheCode) ? new Separator(_rendered, _code.Containers).NewLine + Fence : afterTheCode;
   }
}
