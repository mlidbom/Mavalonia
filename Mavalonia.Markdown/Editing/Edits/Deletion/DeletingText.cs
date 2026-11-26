using Mavalonia.Markdown.Editing.Changes;
using Mavalonia.Markdown.Editing.Writing;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Editing.Edits.Deletion;

///<summary>Deletes text. The blocks that it starts and ends in become one.</summary>
class DeletingText
{
   readonly RenderedRange _deleted;

   internal DeletingText(RenderedRange deleted) => _deleted = deleted;

   internal SourceChange Change() => _deleted is { IsInOneBlock: true, Start.Block: CodeBlock block } ? DeleteCodeIn(block) : DeleteFormattedText();

   SourceChange DeleteCodeIn(CodeBlock block) => new CodeReplacement(_deleted.Rendered, block, new SourceRange(_deleted.Start.Offset, _deleted.End.Offset), code: "").Change();

   SourceChange DeleteFormattedText()
   {
      var replacement = new TextReplacement(_deleted, FormattedText.Empty);
      var plain = replacement.Plain();

      return replacement.ThatLooksRight(tryFirst: plain) ?? plain;
   }
}
