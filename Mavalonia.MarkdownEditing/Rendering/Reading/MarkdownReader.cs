using System.Globalization;
using Markdig.Extensions.Alerts;
using Markdig.Helpers;
using Markdig.Syntax;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;
using MarkdigCodeBlock = Markdig.Syntax.CodeBlock;
using MarkdigTable = Markdig.Extensions.Tables.Table;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Reads a markdown text into the blocks it is rendered as.</summary>
class MarkdownReader
{
   readonly string _markdown;
   readonly MarkdownDialect _dialect;
   readonly List<RenderedBlock> _blocks = [];
   bool _isTheSourceOfATableUnknown;

   MarkdownReader(string markdown, MarkdownDialect dialect)
   {
      _markdown = markdown;
      _dialect = dialect;
   }

   internal static IReadOnlyList<RenderedBlock> Read(string markdown, MarkdownDialect dialect)
   {
      var markdownToShow = markdown.Replace('\0', CharHelper.ReplacementChar); // Markdig does that too, and then reads the replacement.
      var blocks = dialect == MarkdownDialect.PlainText ? ReadPlainText(markdownToShow) : new MarkdownReader(markdownToShow, dialect).ReadBlocks();

      if(dialect == MarkdownDialect.ForEditing) return new EmptyParagraphs(markdownToShow).AddedTo(blocks);
      return blocks.Count > 0 ? blocks : [Paragraph.EmptyAt(0, BlockContainers.None)];
   }

   static IReadOnlyList<RenderedBlock> ReadPlainText(string text)
   {
      return text.Length == 0 ? [] : [new Paragraph(BlockContainers.None, TextOfLines.Read(text, [..Lines()], TextFormatting.None))];

      IEnumerable<SourceRange> Lines()
      {
         for(int? startOfLine = 0; startOfLine is {} start; startOfLine = text.StartOfLineAfter(start)) yield return new SourceRange(start, text.EndOfLineAt(start));
      }
   }

   IReadOnlyList<RenderedBlock> ReadBlocks()
   {
      if(MarkdigParser.Parse(_markdown, _dialect) is not {} document) return [AllAsSource];

      ReadBlocksIn(document, PlaceOfBlocks.OutsideAllContainers);
      return BlocksWhoseSourceIsKnown();
   }

   bool IsForReading => _dialect == MarkdownDialect.ForReading;

   ///<summary>The blocks from the first to the last, or, when the source of one isn't known, all of the markdown as source.</summary>
   ///<remarks>
   /// Of rare markdown, such as a link that goes on into the next row of a table, Markdig says the wrong source.<br/>
   /// To edit that would change something else than what is shown.
   /// </remarks>
   IReadOnlyList<RenderedBlock> BlocksWhoseSourceIsKnown()
   {
      var blocks = _blocks.OrderBy(it => it.Text.Source.Start).ToList();
      return !_isTheSourceOfATableUnknown && IsSourceKnownOf(blocks) ? blocks : [AllAsSource];
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
         case HtmlBlock html when IsForReading && html.IsNothingButComments: break;
         case LinkReferenceDefinitionGroup when IsForReading: break;
         case AlertBlock alert: ReadAlert(alert, place); break;
         case QuoteBlock quote: ReadQuote(quote, place); break;
         case ListBlock list: ReadList(list, place); break;
         case MarkdigTable table: ReadTable(table, place.Containers); break;
         case HeadingBlock heading: ReadHeading(heading, place.Containers); break;
         case ParagraphBlock paragraph: ReadParagraph(paragraph, place.Containers); break;
         case FencedCodeBlock code: _blocks.Add(new CodeBlockReader(_markdown, place.Containers).Read(code)); break;
         case MarkdigCodeBlock code: _blocks.Add(new IndentedCodeReader(_markdown, place).Read(code)); break;
         case ThematicBreakBlock thematicBreak: _blocks.Add(new ThematicBreak(place.Containers, RangeOf(thematicBreak))); break;
         case LeafBlock leaf: _blocks.Add(AsSource(RangeOf(leaf), place.Containers)); break;
         case ContainerBlock container: ReadBlocksIn(container, place); break;
      }
   }

   static SourceRange RangeOf(MarkdownObject it) => new(it.Span.Start, it.Span.End + 1);

   SourceBlock AllAsSource => AsSource(new SourceRange(0, _markdown.Length), BlockContainers.None);

   SourceBlock AsSource(SourceRange range, BlockContainers containers) => new(containers, ReaderOfText(containers).ReadAsSource(range));

   TextReader ReaderOfText(BlockContainers containers, int shift = 0) => new(_markdown, containers, shift) { Dialect = _dialect };

   void ReadQuote(QuoteBlock quote, PlaceOfBlocks place)
   {
      var inTheQuote = place.In(new Quote(), _markdown.ColumnAt(EndOfTheQuoteMark()));

      if(quote.Count == 0)
         _blocks.Add(Paragraph.EmptyAt(_markdown.EndOfLineAt(Math.Min(quote.Span.End + 1, _markdown.Length)), inTheQuote.Containers));
      else
         ReadBlocksIn(quote, inTheQuote);

      int EndOfTheQuoteMark() => new Quote().EndOfItsPrefix(_markdown, quote.Span.Start) ?? quote.Span.Start;
   }

   void ReadAlert(AlertBlock alert, PlaceOfBlocks place)
   {
      var reader = new AlertReader(_markdown, alert);
      var inTheAlert = place.In(reader.Container, _markdown.ColumnAt(reader.EndOfTheQuoteMark));

      _blocks.Add(reader.ReadTitle(inTheAlert.Containers));
      ReadBlocksIn(alert, inTheAlert);
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
      if(new TableReader(_markdown, containers, table, () => ReaderOfText(containers)).Read() is {} blocks)
         _blocks.AddRange(blocks);
      else
         _isTheSourceOfATableUnknown = true;
   }

   void ReadHeading(HeadingBlock heading, BlockContainers containers)
   {
      if(heading.IsSetext) ReadUnderlinedHeading(heading, containers);
      else ReadMarkedHeading(heading, containers);
   }

   void ReadUnderlinedHeading(HeadingBlock heading, BlockContainers containers)
   {
      if(ReaderOfText(containers).Read(heading.Inline) is {} text) _blocks.Add(new Heading(containers, text, RangeOf(heading), heading.Level));
   }

   void ReadMarkedHeading(HeadingBlock heading, BlockContainers containers)
   {
      var endOfMarks = heading.Span.Start + heading.Level;

      if(_markdown.SkipSpaces(endOfMarks) == endOfMarks)
         _blocks.Add(new Paragraph(containers, ReaderOfText(containers).ReadAsText(RangeOf(heading)))); // So far, it may be the start of anything, such as of #5.
      else
         _blocks.Add(new Heading(containers,
                                 ReaderOfText(containers, ShiftOfTheText()).Read(heading.Inline) ?? RenderedText.EmptyAt(endOfMarks + 1),
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
      if(ReaderOfText(containers).Read(paragraph.Inline) is {} text && !text.Chars.All(IsWrittenAsSpaceOrTab)) _blocks.Add(new Paragraph(containers, text));
   }

   bool IsWrittenAsSpaceOrTab(RenderedChar it) => it.Source.Length == 1 && _markdown[it.Source.Start] is ' ' or '\t';
}
