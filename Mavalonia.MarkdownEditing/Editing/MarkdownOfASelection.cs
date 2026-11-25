using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Editing;

///<summary>The markdown of what is selected, to be used elsewhere: without the containers that all of it is inside.</summary>
class MarkdownOfASelection
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedPosition _from;
   readonly RenderedPosition _to;

   internal MarkdownOfASelection(RenderedMarkdown rendered, RenderedPosition from, RenderedPosition to)
   {
      _rendered = rendered;
      _from = from;
      _to = to;
   }

   internal string Markdown => _from.Block == _to.Block ? Text(_from.Block, _from.Index, _to.Index) : MarkdownOfSeveralBlocks();

   string Text(RenderedBlock block, int start, int end)
   {
      var text = FormattedText.Of(_rendered, block)[start..end];

      return block is CodeBlock ? new string([..text.Characters.Select(it => it.Value)]) : WithoutPrefixes(InlineMarkdownWriter.WaysToWrite(text).First(), block.Containers, isAtTheStartOfALine: false);
   }

   string MarkdownOfSeveralBlocks()
   {
      var shared = SharedContainers;
      var (textBefore, start) = StartOf(_from, shared);
      var (textAfter, end) = EndOf(_to);

      return WithoutPrefixes(textBefore + _rendered.Source[start..end] + textAfter, shared, isAtTheStartOfALine: false);
   }

   BlockContainers SharedContainers =>
      _rendered.Blocks.Skip(_rendered.NumberOf(_from.Block)).Take(_rendered.NumberOf(_to.Block) - _rendered.NumberOf(_from.Block) + 1).Select(it => it.Containers).Aggregate((all, it) => all.SharedWith(it));

   ///<summary>The markdown of the block that the selection starts in, as far as it isn't just its source, and the offset from which it is.</summary>
   (string Markdown, int Offset) StartOf(RenderedPosition position, BlockContainers shared)
   {
      var block = position.Block;
      var prefix = block.Containers.WithoutTheOutermost(shared.Count).PrefixOfALine(isTheFirstOf: it => _rendered.IsTheFirstBlockInside(it, block));

      return position.IsAtTheStartOfItsBlock || IsCopiedAsAWhole(block)
                ? (prefix, block.Source.Start)
                : (prefix + InlineMarkdownWriter.WaysToWrite(FormattedText.Of(_rendered, block)[position.Index..]).First(), block.Text.Source.End);
   }

   ///<summary>The markdown of the block that the selection ends in, as far as it isn't just its source, and the offset up to which it is.</summary>
   (string Markdown, int Offset) EndOf(RenderedPosition position)
   {
      var block = position.Block;

      return position.IsAtTheEndOfItsBlock || IsCopiedAsAWhole(block)
                ? ("", block.Source.End)
                : (InlineMarkdownWriter.WaysToWrite(FormattedText.Of(_rendered, block)[..position.Index]).First(), block.Text.Source.Start);
   }

   ///<summary>A part of a table isn't a table, nor is a part of a block of code one.</summary>
   static bool IsCopiedAsAWhole(RenderedBlock block) => block is TableCell or CodeBlock;

   static string WithoutPrefixes(string markdown, BlockContainers containers, bool isAtTheStartOfALine)
   {
      var lines = markdown.Split('\n');
      return string.Join('\n', lines.Select((line, number) => number == 0 && !isAtTheStartOfALine ? line : line[Math.Min(line.Length, containers.EndOfTheirPrefixes(line, 0))..]));
   }
}
