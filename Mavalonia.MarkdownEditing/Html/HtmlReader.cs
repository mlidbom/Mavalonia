using System.Globalization;
using AngleSharp.Dom;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Html;

///<summary>Reads HTML as blocks of markdown.</summary>
class HtmlReader
{
   readonly List<MarkdownBlock> _blocks = [];
   readonly TextCollector _text = new();

   HtmlReader() {}

   internal static IReadOnlyList<MarkdownBlock> ReadBlocksIn(IElement element) => ReadBlocksIn(element, TextFormatting.None);

   static IReadOnlyList<MarkdownBlock> ReadBlocksIn(IElement element, TextFormatting formattingAround)
   {
      var reader = new HtmlReader();
      reader.ReadWhatIsIn(element, element.Format(formattingAround));
      reader.EndParagraph();
      return reader._blocks;
   }

   void ReadWhatIsIn(INode node, TextFormatting formatting)
   {
      foreach(var child in node.ChildNodes)
         if(child is IText text)
            _text.Add(text.Data, formatting);
         else if(child is IElement { IsHidden: false } element)
            Read(element, element.Format(formatting));
   }

   void Read(IElement element, TextFormatting formatting)
   {
      if(element.LocalName == "br") BreakLine();
      else if(element.LocalName == "hr") Add(MarkdownBlock.ThematicBreak);
      else if(element.HeadingLevel is {} level) AddHeading(level, element, formatting);
      else if(element.IsCode) AddCode(element);
      else if(element.LocalName == "blockquote") AddQuote(ReadBlocksIn(element, formatting));
      else if(element.IsAList || element.LocalName == "li") AddList(element, formatting);
      else if(element.LocalName == "table") AddTable(element, formatting);
      else if(element.IsAParagraph) AddParagraphs(element, formatting);
      else if(element.IsALine) AddLines(element, formatting);
      else AddTextIn(element, formatting);
   }

   void AddTextIn(IElement element, TextFormatting formatting)
   {
      _text.Add(element.TextThatStandsForIt, formatting);
      ReadWhatIsIn(element, formatting);
   }

   void AddLines(IElement element, TextFormatting formatting)
   {
      _text.BreakLine();
      ReadWhatIsIn(element, formatting);
      _text.BreakLine();
   }

   void AddParagraphs(IElement element, TextFormatting formatting)
   {
      EndParagraph();
      ReadWhatIsIn(element, formatting);
      EndParagraph();
   }

   void BreakLine()
   {
      if(_text.IsOnABlankLine) EndParagraph();
      else _text.BreakLine();
   }

   void EndParagraph()
   {
      if(_text.Take() is { Length: > 0 } text) _blocks.Add(MarkdownBlock.Paragraph(text));
   }

   void Add(MarkdownBlock block)
   {
      EndParagraph();
      _blocks.Add(block);
   }

   void AddHeading(int level, IElement element, TextFormatting formatting)
   {
      var text = HtmlOnOneLine.TextIn(element, formatting).WithoutBold; // A heading is bold as it is.
      if(text.Length > 0) Add(MarkdownBlock.Heading(level, text));
   }

   void AddCode(IElement element)
   {
      var code = HtmlCode.TextIn(element);
      if(code.Length > 0) Add(MarkdownBlock.Code(code, element.LanguageOfCode));
   }

   void AddQuote(IReadOnlyList<MarkdownBlock> blocks)
   {
      if(blocks.Count > 0) Add(MarkdownBlock.Quote(blocks));
   }

   void AddList(IElement list, TextFormatting formatting)
   {
      var items = ItemsOf(list, formatting);
      if(items.Count > 0) Add(list.LocalName == "ol" ? MarkdownBlock.NumberedList(items, FirstNumberOf(list)) : MarkdownBlock.BulletedList(items));
   }

   static int FirstNumberOf(IElement list) => int.TryParse(list.GetAttribute("start"), CultureInfo.InvariantCulture, out var number) && number >= 0 ? number : 1;

   ///<summary>A list that is right inside the list, and not inside an item as it should be, belongs to the item before it.</summary>
   static IReadOnlyList<IReadOnlyList<MarkdownBlock>> ItemsOf(IElement list, TextFormatting formatting)
   {
      var items = new List<IReadOnlyList<MarkdownBlock>>();
      foreach(var element in list.LocalName == "li" ? [list] : list.Children.Where(it => !it.IsHidden))
      {
         var blocks = element.IsAList ? ReadBlocksInAList(element) : ReadBlocksIn(element, formatting);
         if(element.IsAList && items.Count > 0) items[^1] = [..items[^1], ..blocks];
         else if(blocks.Count > 0) items.Add(blocks);
      }

      return items;

      IReadOnlyList<MarkdownBlock> ReadBlocksInAList(IElement element)
      {
         var reader = new HtmlReader();
         reader.AddList(element, element.Format(formatting));
         return reader._blocks;
      }
   }

   void AddTable(IElement table, TextFormatting formatting)
   {
      var rows = HtmlTable.RowsOf(table, formatting);
      if(rows.Count > 0) Add(MarkdownBlock.Table(rows));
      else AddParagraphs(table, formatting);
   }
}
