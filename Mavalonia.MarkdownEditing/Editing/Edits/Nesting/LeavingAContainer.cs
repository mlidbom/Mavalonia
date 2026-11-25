using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Editing.Edits.Nesting;

///<summary>Takes a block out of the innermost container it is inside.</summary>
class LeavingAContainer
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedPosition _position;

   internal LeavingAContainer(RenderedMarkdown rendered, RenderedPosition position)
   {
      _rendered = rendered;
      _position = position;
   }

   internal SourceChange Change() =>
      new BlockMove(_rendered, _position.Block, _position.Block.Containers.Innermost is ListItem item ? ContainersAfterLeaving(item) : ContainersAround)
        .Change().WithTheCaretAt(_rendered.NumbersOf(_position));

   ///<summary>The first block takes the item out of its list. Any other leaves the item behind, and becomes the next item of the list.</summary>
   BlockContainers ContainersAfterLeaving(ListItem item) => _rendered.IsTheFirstBlockInside(item, _position.Block) ? ContainersAfterLeavingTheList : ContainersAround.Inside(item.NewItemAfter());

   ///<summary>Out of a list inside an item of another list, the item becomes one of the other list.</summary>
   BlockContainers ContainersAfterLeavingTheList => ContainersAround.Innermost is ListItem itemAroundTheList ? ContainersAround.WithoutTheInnermost.Inside(itemAroundTheList.NewItemAfter()) : ContainersAround;

   BlockContainers ContainersAround => _position.Block.Containers.WithoutTheInnermost;
}
