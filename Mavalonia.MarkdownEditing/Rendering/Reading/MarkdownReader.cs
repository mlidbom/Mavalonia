using System.Globalization;
using Markdig;
using Markdig.Extensions.EmphasisExtras;
using Markdig.Parsers;
using Markdig.Syntax;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;
using MarkdigTable = Markdig.Extensions.Tables.Table;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Reads a markdown text into the blocks it is rendered as.</summary>
class MarkdownReader
{
   static readonly MarkdownPipeline Pipeline = CreatePipeline();

   readonly string _markdown;
   readonly List<RenderedBlock> _blocks = [];

   MarkdownReader(string markdown) => _markdown = markdown;

   internal static IReadOnlyList<RenderedBlock> Read(string markdown)
   {
      var reader = new MarkdownReader(markdown);
      reader.ReadBlocksIn(Markdown.Parse(markdown, Pipeline), Place.OutsideAllContainers);

      return new EmptyParagraphs(markdown).AddedTo([..reader._blocks.OrderBy(it => it.Text.Source.Start)]);
   }

   static MarkdownPipeline CreatePipeline()
   {
      var builder = new MarkdownPipelineBuilder().UsePreciseSourceLocation()
                                                 .UsePipeTables()
                                                 .UseEmphasisExtras(EmphasisExtraOptions.Strikethrough);

      TurnOffTheMarkupThatPeopleTypeWithoutMeaningTo();
      return builder.Build();

      void TurnOffTheMarkupThatPeopleTypeWithoutMeaningTo()
      {
         builder.BlockParsers.Find<ParagraphBlockParser>()!.ParseSetextHeadings = false; // Typing a dash to start a list under a line would turn that line into a heading.
         builder.BlockParsers.TryRemove<IndentedCodeBlockParser>(); // Typing spaces at the start of a line would turn it into code.
      }
   }

   void ReadBlocksIn(ContainerBlock blocks, Place place)
   {
      foreach(var block in blocks) Read(block, place);
   }

   void Read(Block block, Place place)
   {
      switch(block)
      {
         case QuoteBlock quote: ReadQuote(quote, place); break;
         case ListBlock list: ReadList(list, place); break;
         case MarkdigTable table: _blocks.AddRange(new TableReader(_markdown, place.Containers).Read(table)); break;
         case HeadingBlock heading: ReadHeading(heading, place); break;
         case ParagraphBlock paragraph: ReadParagraph(paragraph, place); break;
         case FencedCodeBlock code: _blocks.Add(new CodeBlockReader(_markdown, place.Containers).Read(code)); break;
         case ThematicBreakBlock thematicBreak: _blocks.Add(new ThematicBreak(place.Containers, RangeOf(thematicBreak))); break;
         case LeafBlock leaf: _blocks.Add(new SourceBlock(place.Containers, new TextReader(_markdown, place.Containers).ReadAsSource(RangeOf(leaf)))); break;
         case ContainerBlock container: ReadBlocksIn(container, place); break;
      }
   }

   static SourceRange RangeOf(MarkdownObject it) => new(it.Span.Start, it.Span.End + 1);

   void ReadQuote(QuoteBlock quote, Place place)
   {
      var inTheQuote = place.In(new Quote(), ColumnOf(EndOfTheQuoteMark()));

      if(quote.Count == 0)
         _blocks.Add(Paragraph.EmptyAt(_markdown.EndOfLineAt(Math.Min(quote.Span.End + 1, _markdown.Length)), inTheQuote.Containers));
      else
         ReadBlocksIn(quote, inTheQuote);

      int EndOfTheQuoteMark() => new Quote().EndOfItsPrefix(_markdown, quote.Span.Start) ?? quote.Span.Start;
   }

   void ReadList(ListBlock list, Place place)
   {
      var numberOfTheFirst = list.IsOrdered ? int.Parse(list.OrderedStart!, CultureInfo.InvariantCulture) : (int?)null;

      foreach(var (index, item) in list.Cast<ListItemBlock>().Index()) ReadItem(item, numberOfTheFirst + index, place);
   }

   void ReadItem(ListItemBlock item, int? number, Place place)
   {
      var marker = MarkerOf(item);
      var endOfMarker = item.Span.Start + marker.Length;

      if(item.Count == 0)
         ReadEmptyItem();
      else
         ReadBlocksIn(item, InTheItem(ColumnOfItsContent()));

      void ReadEmptyItem()
      {
         var endOfLine = _markdown.EndOfLineAt(endOfMarker);
         _blocks.Add(endOfLine == endOfMarker
                        ? new Paragraph(place.Containers, new TextReader(_markdown, place.Containers).ReadAsText(new SourceRange(item.Span.Start, endOfMarker))) // So far, it may be the start of anything, such as of -5.
                        : Paragraph.EmptyAt(endOfLine, InTheItem(ColumnAfterTheSpaceAfterTheMarker()).Containers));
      }

      int ColumnOfItsContent() => item[0].Span.Start <= _markdown.EndOfLineAt(endOfMarker) ? ColumnOf(item[0].Span.Start) : ColumnAfterTheSpaceAfterTheMarker();

      int ColumnAfterTheSpaceAfterTheMarker() => ColumnOf(endOfMarker) + 1;

      Place InTheItem(int column) => place.In(new ListItem(marker, number, column - ColumnThatItIsIndentedFrom()), column);

      // On a line that isn't the first one inside a container, text may start at another column than on the first.
      int ColumnThatItIsIndentedFrom()
      {
         var startOfLine = _markdown.StartOfLineAt(item.Span.Start);
         var isOnALineWithAllPrefixes = place.Containers.OfTheLineAt(_markdown, startOfLine).Count == place.Containers.Count;
         return isOnALineWithAllPrefixes ? place.Containers.EndOfTheirPrefixes(_markdown, startOfLine) - startOfLine : place.Column;
      }
   }

   string MarkerOf(ListItemBlock item)
   {
      var end = item.Span.Start;
      while(char.IsAsciiDigit(_markdown[end])) end++;
      return _markdown[item.Span.Start..(end + 1)];
   }

   void ReadHeading(HeadingBlock heading, Place place)
   {
      var endOfMarks = heading.Span.Start + heading.Level;

      if(_markdown.SkipSpaces(endOfMarks) == endOfMarks)
         _blocks.Add(new Paragraph(place.Containers, new TextReader(_markdown, place.Containers).ReadAsText(RangeOf(heading)))); // So far, it may be the start of anything, such as of #5.
      else
         _blocks.Add(new Heading(place.Containers,
                                 new TextReader(_markdown, place.Containers).Read(heading.Inline) ?? RenderedText.EmptyAt(endOfMarks + 1),
                                 new SourceRange(heading.Span.Start, _markdown.EndOfLineAt(heading.Span.Start)),
                                 heading.Level));
   }

   void ReadParagraph(ParagraphBlock paragraph, Place place)
   {
      if(new TextReader(_markdown, place.Containers).Read(paragraph.Inline) is {} text) _blocks.Add(new Paragraph(place.Containers, text));
   }

   int ColumnOf(int offset) => offset - _markdown.StartOfLineAt(offset);

   ///<summary>Where a block is.</summary>
   ///<param name="Containers">The containers it is inside.</param>
   ///<param name="Column">The column that text inside them starts at.</param>
   readonly record struct Place(BlockContainers Containers, int Column)
   {
      internal static readonly Place OutsideAllContainers = new(BlockContainers.None, 0);

      internal Place In(BlockContainer container, int column) => new(Containers.Inside(container), column);
   }
}
