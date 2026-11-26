using Mavalonia.Markdown.Editing.Changes;
using Mavalonia.Markdown.Editing.Edits.NewBlocks;
using Mavalonia.Markdown.Editing.Writing;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Editing.Edits;

///<summary>Pastes markdown where the caret is.</summary>
class Pasting
{
   readonly Caret _caret;

   internal Pasting(Caret caret) => _caret = caret;

   RenderedMarkdown Rendered => _caret.Rendered;
   RenderedBlock Block => _caret.Block;
   LineBreaksInside LineBreaks => new(Rendered, Block.Containers);

   internal SourceChange Of(string markdown) => Block switch
   {
      CodeBlock => new Typing(_caret, _caret.Formatting).Of(InTheLinesOfTheBlock(markdown)),
      TableCell => new Typing(_caret, _caret.Formatting).Of(OnOneLine(markdown)),
      _ when IsTextOfOneParagraph(markdown) => SourceChange.Insert(Rendered, _caret.Offset, InTheLinesOfTheBlock(markdown)),
      _ => new InsertingBlocks(_caret, markdown).WithTheCaretAtTheirEnd()
   };

   static bool IsTextOfOneParagraph(string markdown) => RenderedMarkdown.Of(markdown).Blocks is [Paragraph { Containers.Count: 0 }];

   static string OnOneLine(string markdown) => string.Join(' ', markdown.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries));

   ///<summary>With the prefix that each line must have to be in the block.</summary>
   string InTheLinesOfTheBlock(string markdown) => LineBreaks.Between(markdown.ReplaceLineEndings("\n").Split('\n'));
}
