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

   internal string Markdown => WithDefinitionsOfLinks(_selected.IsInOneBlock ? MarkdownOfTextIn(_selected.Start.Block) : MarkdownOfSeveralBlocks());

   ///<summary>A link whose definition is elsewhere in the text would not be a link without it.</summary>
   string WithDefinitionsOfLinks(string markdown)
   {
      var definitions = _selected.PartInEachBlock.SelectMany(it => it.Start.Block.Text.Chars.Take(it.Start.Index..it.End.Index))
                                 .Select(it => it.Formatting.Link?.Definition)
                                 .OfType<string>()
                                 .Distinct()
                                 .Where(it => !markdown.Contains(it, StringComparison.Ordinal))
                                 .ToList();

      return definitions.Count == 0 ? markdown : $"{markdown}\n\n{string.Join('\n', definitions)}";
   }

   string MarkdownOfTextIn(RenderedBlock block)
   {
      var text = FormattedText.In(_selected);
      if(block is CodeBlock or AlertTitle) return text.Value;

      var leftOut = ContainersToLeaveOut();
      return WithoutPrefixesOf(leftOut, PrefixOfTheFirstLineWithout(leftOut) + InlineMarkdownWriter.PreferredWayToWrite(text));
   }

   string MarkdownOfSeveralBlocks()
   {
      var leftOut = ContainersToLeaveOut();
      var (start, startOfSource) = MarkdownOfTheStart();
      var (end, endOfSource) = MarkdownOfTheEnd();

      return WithoutPrefixesOf(leftOut, PrefixOfTheFirstLineWithout(leftOut) + start + _rendered.Source[startOfSource..endOfSource] + end);
   }

   ///<summary>What is copied is the start of all the containers it starts in, also when it starts in the middle of a list item.</summary>
   string PrefixOfTheFirstLineWithout(BlockContainers leftOut) => _selected.Start.Block.Containers.WithoutTheOutermost(leftOut.Count).PrefixOfALine(isTheFirstOf: _ => true);

   ///<summary>Those that all of the blocks are inside. But the title of an alert isn't one outside the alert, nor is the box of a task one outside its list.</summary>
   BlockContainers ContainersToLeaveOut()
   {
      var shared = _selected.Blocks.Select(it => it.Containers).Aggregate((all, it) => all.SharedWith(it));
      return _selected.Start switch
      {
         { Index: 0, Block: AlertTitle title } => shared.Around(title.Alert),
         { Index: 0, Block: { Containers.Innermost: ListItem item, Text: var text } } when text.StartsWithTheBoxOfATask => shared.Around(item),
         _ => shared
      };
   }

   ///<summary>The markdown of what is selected of the first block, as far as it isn't just its source, and the offset from which it is.</summary>
   (string Markdown, int StartOfSource) MarkdownOfTheStart()
   {
      var (block, index) = _selected.Start;
      if(IsNothingSelectedOfTheFirst) return ("", block.Source.End);

      return index == 0 || IsCopiedAsAWhole(block)
                ? ("", block.Source.Start)
                : (InlineMarkdownWriter.PreferredWayToWrite(FormattedText.Of(_rendered, block)[index..]), block.Text.Source.End);
   }

   ///<summary>The markdown of what is selected of the last block, as far as it isn't just its source, and the offset up to which it is.</summary>
   (string Markdown, int EndOfSource) MarkdownOfTheEnd()
   {
      var (block, index) = _selected.End;
      if(IsNothingSelectedOfTheLast) return ("", Math.Max(_rendered.Source.StartOfLineAt(block.Source.Start), _rendered.BlockBefore(block)!.Source.End)); // The block before may end with the line break.

      return _selected.End.IsAtTheEndOfItsBlock || IsCopiedAsAWhole(block)
                ? ("", block.Source.End)
                : (InlineMarkdownWriter.PreferredWayToWrite(FormattedText.Of(_rendered, block)[..index]), block.Text.Source.Start);
   }

   bool IsNothingSelectedOfTheFirst => _selected.Start is { IsAtTheEndOfItsBlock: true, Block: { Text.IsEmpty: false } block } && (block is not TableCell cell || cell == cell.Table.LastCell);
   bool IsNothingSelectedOfTheLast => _selected.End is { Index: 0, Block: { Text.IsEmpty: false } block } && (block is not TableCell cell || cell == cell.Table.FirstCell);

   ///<summary>A part of a table isn't a table, nor is a part of a block of code one.</summary>
   static bool IsCopiedAsAWhole(RenderedBlock block) => block is TableCell or CodeBlock;

   ///<summary>The first line is without a prefix already, since no block's source starts with one.</summary>
   static string WithoutPrefixesOf(BlockContainers containers, string markdown) =>
      string.Join('\n', markdown.Split('\n').Select((line, number) => number == 0 ? line : line[containers.EndOfTheirPrefixes(line, 0)..]));
}
