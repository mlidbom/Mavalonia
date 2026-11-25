using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Editing;

///<summary>The markdown of what is selected, to be used elsewhere: without the containers that all of it is inside.</summary>
class MarkdownOfASelection
{
   readonly RenderedMarkdown _rendered;
   readonly RenderedRange _selected;

   internal MarkdownOfASelection(RenderedRange selected)
   {
      _rendered = selected.Rendered;
      _selected = selected;
   }

   internal string Markdown => _selected.IsInOneBlock ? MarkdownOfTextIn(_selected.Start.Block) : MarkdownOfSeveralBlocks();

   string MarkdownOfTextIn(RenderedBlock block)
   {
      var text = FormattedText.In(_selected);
      return block is CodeBlock ? text.Value : WithoutPrefixesOf(block.Containers, InlineMarkdownWriter.PreferredWayToWrite(text));
   }

   string MarkdownOfSeveralBlocks()
   {
      var shared = _selected.Blocks.Select(it => it.Containers).Aggregate((all, it) => all.SharedWith(it));
      var (start, startOfSource) = MarkdownOfTheStart();
      var (end, endOfSource) = MarkdownOfTheEnd();

      return WithoutPrefixesOf(shared, PrefixOfTheFirstLine() + start + _rendered.Source[startOfSource..endOfSource] + end);

      string PrefixOfTheFirstLine()
      {
         var block = _selected.Start.Block;
         return block.Containers.WithoutTheOutermost(shared.Count).PrefixOfALine(isTheFirstOf: it => _rendered.IsTheFirstBlockInside(it, block));
      }
   }

   ///<summary>The markdown of what is selected of the first block, as far as it isn't just its source, and the offset from which it is.</summary>
   (string Markdown, int StartOfSource) MarkdownOfTheStart()
   {
      var (block, index) = _selected.Start;

      return index == 0 || IsCopiedAsAWhole(block)
                ? ("", block.Source.Start)
                : (InlineMarkdownWriter.PreferredWayToWrite(FormattedText.Of(_rendered, block)[index..]), block.Text.Source.End);
   }

   ///<summary>The markdown of what is selected of the last block, as far as it isn't just its source, and the offset up to which it is.</summary>
   (string Markdown, int EndOfSource) MarkdownOfTheEnd()
   {
      var (block, index) = _selected.End;

      return _selected.End.IsAtTheEndOfItsBlock || IsCopiedAsAWhole(block)
                ? ("", block.Source.End)
                : (InlineMarkdownWriter.PreferredWayToWrite(FormattedText.Of(_rendered, block)[..index]), block.Text.Source.Start);
   }

   ///<summary>A part of a table isn't a table, nor is a part of a block of code one.</summary>
   static bool IsCopiedAsAWhole(RenderedBlock block) => block is TableCell or CodeBlock;

   ///<summary>The first line is without a prefix already, since no block's source starts with one.</summary>
   static string WithoutPrefixesOf(BlockContainers containers, string markdown) =>
      string.Join('\n', markdown.Split('\n').Select((line, number) => number == 0 ? line : line[containers.EndOfTheirPrefixes(line, 0)..]));
}
