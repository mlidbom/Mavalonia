using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Editing.Edits.Nesting;

///<summary>Takes the block that the caret is in out of the innermost container it is inside.</summary>
class LeavingAContainer
{
   readonly Caret _caret;

   internal LeavingAContainer(Caret caret) => _caret = caret;

   internal SourceChange Change() =>
      new BlockMove(_caret.Rendered, _caret.Block, _caret.Block.Containers.Innermost is ListItem item ? ContainersAfterLeaving(item) : ContainersAround)
        .Change().WithTheCaretAt(_caret.Rendered.NumbersOf(_caret.Position));

   ///<summary>The first block takes the item out of its list. Any other leaves the item behind, and becomes the next item of the list.</summary>
   BlockContainers ContainersAfterLeaving(ListItem item) => _caret.Rendered.IsTheFirstBlockInside(item, _caret.Block) ? ContainersAfterLeavingTheList : ContainersAround.Inside(item.NewItemAfter());

   ///<summary>Out of a list inside an item of another list, the item becomes one of the other list.</summary>
   BlockContainers ContainersAfterLeavingTheList => ContainersAround.Innermost is ListItem itemAroundTheList ? ContainersAround.WithoutTheInnermost.Inside(itemAroundTheList.NewItemAfter()) : ContainersAround;

   BlockContainers ContainersAround => _caret.Block.Containers.WithoutTheInnermost;
}
