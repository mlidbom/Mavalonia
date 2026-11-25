using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

///<summary>Removes whole blocks.</summary>
class RemovingBlocks
{
   readonly RenderedMarkdown _rendered;

   internal RemovingBlocks(RenderedMarkdown rendered) => _rendered = rendered;

   ///<summary>Removes the blocks from <paramref name="first"/> to <paramref name="last"/>. What separates the last from the block after it then separates that from the block before the first.</summary>
   internal SourceChange Remove(RenderedBlock first, RenderedBlock last)
   {
      if(_rendered.BlockBefore(first) is {} previous) return SourceChange.Remove(_rendered, new SourceRange(previous.Source.End, last.Source.End));
      if(_rendered.BlockAfter(last) is {} next) return SourceChange.Remove(_rendered, new SourceRange(0, _rendered.Source.StartOfLineAt(next.Source.Start)));

      return SourceChange.Remove(_rendered, new SourceRange(0, _rendered.Source.Length));
   }
}
