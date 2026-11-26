using Mavalonia.Markdown.Editing.Changes;
using Mavalonia.Markdown.Rendering;

namespace Mavalonia.Markdown.Editing.Edits.Deletion;

///<summary>Removes whole blocks. What separates the last of them from the block after it then separates that from the block before the first.</summary>
class RemovingBlocks
{
   readonly RenderedRange _removed;

   internal RemovingBlocks(RenderedRange removed) => _removed = removed;

   internal SourceChange Change() => SourceChange.Remove(_removed.Rendered, RemovedSource());

   SourceRange RemovedSource()
   {
      var rendered = _removed.Rendered;
      if(rendered.BlockBefore(_removed.Start.Block) is {} previous) return new SourceRange(previous.Source.End, _removed.End.Block.Source.End);
      if(rendered.BlockAfter(_removed.End.Block) is {} next) return new SourceRange(0, rendered.Source.StartOfLineAt(next.Source.Start));

      return new SourceRange(0, rendered.Source.Length);
   }
}
