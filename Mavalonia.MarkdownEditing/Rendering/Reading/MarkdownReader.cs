using System.Globalization;
using Markdig.Syntax;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;
using MarkdigTable = Markdig.Extensions.Tables.Table;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Reads a markdown text into the blocks it is rendered as.</summary>
class MarkdownReader
{
   readonly string _markdown;
   readonly List<RenderedBlock> _blocks = [];
   bool _isTheSourceOfATableUnknown;

   MarkdownReader(string markdown) => _markdown = markdown;

   internal static IReadOnlyList<RenderedBlock> Read(string markdown)
   {
      var reader = new MarkdownReader(markdown);
      reader.ReadBlocksIn(MarkdigParser.Parse(markdown), PlaceOfBlocks.OutsideAllContainers);

      return new EmptyParagraphs(markdown).AddedTo(reader.BlocksWhoseSourceIsKnown());
   }

   ///<summary>The blocks from the first to the last, or, when the source of one isn't known, all of the markdown as source.</summary>
   ///<remarks>
   /// Of rare markdown, such as a link that goes on into the next row of a table, Markdig says the wrong source.<br/>
   /// To edit that would change something else than what is shown.
   /// </remarks>
   IReadOnlyList<RenderedBlock> BlocksWhoseSourceIsKnown()
   {
      var blocks = _blocks.OrderBy(it => it.Text.Source.Start).ToList();
      return !_isTheSourceOfATableUnknown && IsSourceKnownOf(blocks) ? blocks : [AsSource(new SourceRange(0, _markdown.Length), BlockContainers.None)];
   }

   static bool IsSourceKnownOf(IReadOnlyList<RenderedBlock> blocks) =>
      blocks.All(it => it.Text.IsOfCharsInOrder) && blocks.Zip(blocks.Skip(1)).All(it => it.First.Text.Source.End <= it.Second.Text.Source.Start);

   void ReadBlocksIn(ContainerBlock blocks, PlaceOfBlocks place)
   {
      foreach(var block in blocks) Read(block, place);
   }

   void Read(Block block, PlaceOfBlocks place)
   {
      switch(block)
      {
         case QuoteBlock quote: ReadQuote(quote, place); break;
         case ListBlock list: ReadList(list, place); break;
         case MarkdigTable table: ReadTable(table, place.Containers); break;
         case HeadingBlock heading: ReadHeading(heading, place.Containers); break;
         case ParagraphBlock paragraph: ReadParagraph(paragraph, place.Containers); break;
         case FencedCodeBlock code: _blocks.Add(new CodeBlockReader(_markdown, place.Containers).Read(code)); break;
         case ThematicBreakBlock thematicBreak: _blocks.Add(new ThematicBreak(place.Containers, RangeOf(thematicBreak))); break;
         case LeafBlock leaf: _blocks.Add(AsSource(RangeOf(leaf), place.Containers)); break;
         case ContainerBlock container: ReadBlocksIn(container, place); break;
      }
   }

   static SourceRange RangeOf(MarkdownObject it) => new(it.Span.Start, it.Span.End + 1);

   SourceBlock AsSource(SourceRange range, BlockContainers containers) => new(containers, new TextReader(_markdown, containers).ReadAsSource(range));

   void ReadQuote(QuoteBlock quote, PlaceOfBlocks place)
   {
      var inTheQuote = place.In(new Quote(), _markdown.ColumnAt(EndOfTheQuoteMark()));

      if(quote.Count == 0)
         _blocks.Add(Paragraph.EmptyAt(_markdown.EndOfLineAt(Math.Min(quote.Span.End + 1, _markdown.Length)), inTheQuote.Containers));
      else
         ReadBlocksIn(quote, inTheQuote);

      int EndOfTheQuoteMark() => new Quote().EndOfItsPrefix(_markdown, quote.Span.Start) ?? quote.Span.Start;
   }

   void ReadList(ListBlock list, PlaceOfBlocks place)
   {
      var numberOfTheFirst = list.IsOrdered ? int.Parse(list.OrderedStart!, CultureInfo.InvariantCulture) : (int?)null;

      foreach(var (index, item) in list.Cast<ListItemBlock>().Index()) ReadItem(item, numberOfTheFirst + index, place);
   }

   void ReadItem(ListItemBlock item, int? number, PlaceOfBlocks place)
   {
      var reader = new ListItemReader(_markdown, item, number, place);

      if(item.Count == 0)
         _blocks.Add(reader.ReadEmpty());
      else
         ReadBlocksIn(item, reader.PlaceOfItsBlocks);
   }

   void ReadTable(MarkdigTable table, BlockContainers containers)
   {
      if(new TableReader(_markdown, containers, table).Read() is {} blocks)
         _blocks.AddRange(blocks);
      else
         _isTheSourceOfATableUnknown = true;
   }

   void ReadHeading(HeadingBlock heading, BlockContainers containers)
   {
      var endOfMarks = heading.Span.Start + heading.Level;

      if(_markdown.SkipSpaces(endOfMarks) == endOfMarks)
         _blocks.Add(new Paragraph(containers, new TextReader(_markdown, containers).ReadAsText(RangeOf(heading)))); // So far, it may be the start of anything, such as of #5.
      else
         _blocks.Add(new Heading(containers,
                                 new TextReader(_markdown, containers, ShiftOfTheText()).Read(heading.Inline) ?? RenderedText.EmptyAt(endOfMarks + 1),
                                 new SourceRange(heading.Span.Start, _markdown.EndOfLineAt(heading.Span.Start)),
                                 heading.Level));

      // Markdig doesn't count a tab that the text starts with.
      int ShiftOfTheText()
      {
         var startOfText = _markdown.SkipSpaces(endOfMarks);
         return _markdown[endOfMarks..startOfText].Contains('\t', StringComparison.Ordinal) && heading.Inline?.FirstChild is {} first ? startOfText - first.Span.Start : 0;
      }
   }

   ///<summary>Markdig leaves the tabs after the last cells of rows in a paragraph of its own. No other paragraph is only white space, unless that is written as entities.</summary>
   void ReadParagraph(ParagraphBlock paragraph, BlockContainers containers)
   {
      if(new TextReader(_markdown, containers).Read(paragraph.Inline) is {} text && !text.Chars.All(IsWrittenAsSpaceOrTab)) _blocks.Add(new Paragraph(containers, text));
   }

   bool IsWrittenAsSpaceOrTab(RenderedChar it) => it.Source.Length == 1 && _markdown[it.Source.Start] is ' ' or '\t';
}
