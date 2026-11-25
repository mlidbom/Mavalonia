using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Editing.Changes;

///<summary>Moves a block into other containers, without changing which blocks it is between.</summary>
class BlockMove
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedBlock _block;
   readonly BlockContainers _containers;

   internal BlockMove(RenderedMarkdown rendered, RenderedBlock block, BlockContainers containers)
   {
      _rendered = rendered;
      _block = block;
      _containers = containers;
   }

   RenderedBlock? Previous => _rendered.BlockBefore(_block);
   RenderedBlock? Next => _rendered.BlockAfter(_block);
   bool StaysAListItem => _block.Containers.Innermost is ListItem && _containers.Innermost is ListItem;

   internal SourceChange Change()
   {
      var (start, textBefore) = Previous is null ? (_rendered.Source.StartOfLineAt(_block.Source.Start), "") : (Previous.Source.End, SeparatorFrom(Previous));
      var (end, textAfter) = Next is null || StaysAListItem ? (_block.Text.Source.End, "") : (_rendered.Source.StartOfLineAt(Next.Source.Start), SeparatorFrom(Next));

      return SourceChange.Replace(_rendered, new SourceRange(start, end), textBefore + _containers.PrefixOfALine(isTheFirstOf: IsEntered) + TextOfTheBlock() + textAfter);
   }

   bool IsEntered(BlockContainer container) => Previous?.IsInside(container) != true;

   ///<summary>Without a blank line between them, blocks could become one.</summary>
   string SeparatorFrom(RenderedBlock other) => StaysAListItem ? _rendered.Source.LineBreak : new LineBreaksInside(_rendered, _containers.SharedWith(other.Containers)).BlankLine + _rendered.Source.LineBreak;

   string TextOfTheBlock()
   {
      var start = _block.Source.Start;
      var text = "";
      foreach(var lineBreak in _block.Text.Chars.Where(it => it.IsLineBreak))
      {
         text += _rendered.Source[start.._rendered.Source.StartOfLineAfter(lineBreak.Source.Start)!.Value] + _containers.PrefixOfTheirLines;
         start = lineBreak.Source.End;
      }

      return text + _rendered.Source[start.._block.Text.Source.End];
   }
}
