using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Editing.Edits.Nesting;

///<summary>Takes a block out of the innermost container it is inside.<br/>
/// Out of an item of a list inside another list, it becomes an item of the other list. A block that isn't the first in its list item becomes the next item of the same list.</summary>
class LeavingAContainer
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedPosition _position;
   readonly BlockContainers _containers;

   internal LeavingAContainer(RenderedMarkdown rendered, RenderedPosition position)
   {
      _rendered = rendered;
      _position = position;
      _containers = ContainersAfterLeaving();
   }

   RenderedBlock Block => _position.Block;
   RenderedBlock? Previous => _rendered.BlockBefore(Block);
   RenderedBlock? Next => _rendered.BlockAfter(Block);
   bool BecomesAListItem => Block.Containers.Innermost is ListItem && _containers.Innermost is ListItem;

   internal SourceChange Change()
   {
      var (start, textBefore) = Previous is null ? (_rendered.Source.StartOfLineAt(Block.Source.Start), "") : (Previous.Source.End, SeparatorFrom(Previous));
      var text = textBefore + _containers.PrefixOfALine(isTheFirstOf: IsEntered) + TextOfTheBlock();

      var (end, textAfter) = Next is null || BecomesAListItem ? (Block.Text.Source.End, "") : (_rendered.Source.StartOfLineAt(Next.Source.Start), SeparatorFrom(Next));

      return SourceChange.Replace(_rendered, new SourceRange(start, end), text + textAfter).WithTheCaretAt(_rendered.NumbersOf(_position));
   }

   BlockContainers ContainersAfterLeaving()
   {
      var without = Block.Containers.WithoutTheInnermost;
      if(Block.Containers.Innermost is not ListItem item) return without;

      if(!_rendered.IsTheFirstBlockInside(item, Block)) return without.Inside(item.NewItemAfter());
      return without.Innermost is ListItem itemOfTheOuterList ? without.WithoutTheInnermost.Inside(itemOfTheOuterList.NewItemAfter()) : without;
   }

   ///<summary>Without a blank line between them, blocks could become one.</summary>
   string SeparatorFrom(RenderedBlock other) => BecomesAListItem ? _rendered.Source.LineBreak : new LineBreaksInside(_rendered, _containers.SharedWith(other.Containers)).BlankLine + _rendered.Source.LineBreak;

   bool IsEntered(BlockContainer container) => Previous?.IsInside(container) != true;

   string TextOfTheBlock()
   {
      var start = Block.Source.Start;
      var text = "";
      foreach(var lineBreak in Block.Text.Characters.Where(it => it.IsLineBreak))
      {
         text += _rendered.Source[start.._rendered.Source.StartOfLineAfter(lineBreak.Source.Start)!.Value] + _containers.PrefixOfTheirLines;
         start = lineBreak.Source.End;
      }

      return text + _rendered.Source[start..Block.Text.Source.End];
   }
}
