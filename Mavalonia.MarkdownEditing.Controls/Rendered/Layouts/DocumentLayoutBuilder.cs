using System.Globalization;
using Avalonia;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;

///<summary>Lays out rendered markdown from top to bottom.</summary>
class DocumentLayoutBuilder
{
   static readonly string[] Bullets = ["•", "◦", "▪"];

   readonly RenderedMarkdown _rendered;
   readonly MarkdownAppearance _appearance;
   readonly TextLayouts _texts;
   readonly Rect _area;
   readonly List<BlockLayout> _blocks = [];
   readonly List<ContainerDecoration> _decorations = [];

   ///<summary>Where the next block starts.</summary>
   double _top;

   RenderedBlock? _previous;
   double _bottomOfPrevious;

   DocumentLayoutBuilder(RenderedMarkdown rendered, MarkdownAppearance appearance, TextLayouts texts, double width, Thickness padding)
   {
      _rendered = rendered;
      _appearance = appearance;
      _texts = texts;
      _area = new Rect(0, 0, width, 0).Deflate(new Thickness(padding.Left, 0, padding.Right, 0));
      _top = padding.Top;
   }

   internal static DocumentLayout LayOut(RenderedMarkdown rendered, MarkdownAppearance appearance, TextLayouts texts, double width, Thickness padding)
   {
      var builder = new DocumentLayoutBuilder(rendered, appearance, texts, width, padding);
      builder.LayOutBlocks();
      return new DocumentLayout(rendered, builder._blocks, builder._decorations, new Size(width, builder._top + padding.Bottom));
   }

   double SpaceBetweenBlocks => _appearance.Scaled(8);
   double SpaceAboveAHeading => _appearance.Scaled(12);
   double SpaceBetweenListItems => _appearance.Scaled(3);
   double IndentationInAQuote => _appearance.Scaled(16);
   double IndentationInAListItem => _appearance.Scaled(24);
   double PaddingOfCode => _appearance.Scaled(8);
   double NarrowestText => _appearance.Scaled(40);

   void LayOutBlocks()
   {
      foreach(var block in _rendered.Blocks)
      {
         if(block is not TableCell cell) LayOut(block);
         else if(cell == cell.Table.FirstCell) LayOut(cell.Table);

         _previous = block;
      }
   }

   void LayOut(RenderedBlock block)
   {
      LeaveSpaceBefore(block);

      var left = LeftOf(block);
      var padding = block is CodeBlock ? PaddingOfCode : 0;
      var text = _texts.Of(block, Math.Max(NarrowestText, _area.Right - left - 2 * padding));
      var bounds = new Rect(left, _top, Math.Max(0, _area.Right - left), text.Height + 2 * padding);
      var originOfText = bounds.TopLeft + new Point(padding, padding);

      _blocks.Add(new BlockLayout(block, bounds, text, originOfText, _appearance));
      _top = bounds.Bottom;
      DecorateContainersOf(block, originOfText.Y);
   }

   void LayOut(Table table)
   {
      LeaveSpaceBefore(table.FirstCell);

      var topLeft = new Point(LeftOf(table.FirstCell), _top);
      var tableLayout = new TableLayout(table, _appearance, _texts, topLeft, _area.Right - topLeft.X);

      _blocks.AddRange(tableLayout.Cells);
      _top = tableLayout.Bottom;
      DecorateContainersOf(table.FirstCell, topLeft.Y);
   }

   void LeaveSpaceBefore(RenderedBlock block)
   {
      _bottomOfPrevious = _top;
      if(_previous is not null) _top += IsTheNextItemOfAList(block, _previous) ? SpaceBetweenListItems : block is Heading ? SpaceAboveAHeading : SpaceBetweenBlocks;
   }

   static bool IsTheNextItemOfAList(RenderedBlock block, RenderedBlock previous) => previous.Containers.OutermostFirst.OfType<ListItem>().Any() && block.Containers.Innermost is ListItem item && !previous.IsInside(item);

   double LeftOf(RenderedBlock block) => LeftInside(block.Containers.OutermostFirst);

   double LeftInside(IEnumerable<BlockContainer> containers) => _area.Left + containers.Sum(it => it is Quote ? IndentationInAQuote : IndentationInAListItem);

   void DecorateContainersOf(RenderedBlock block, double topOfText)
   {
      foreach(var (depth, container) in block.Containers.OutermostFirst.Index())
      {
         var around = block.Containers.OutermostFirst.Take(depth).ToList();
         var isTheFirstBlockInside = _previous?.IsInside(container) != true;

         if(container is Quote) AddQuoteBar(LeftInside(around), top: isTheFirstBlockInside ? topOfText : _bottomOfPrevious);
         else if(container is ListItem item && isTheFirstBlockInside) AddMarkerOf(item, around, topOfText);
      }
   }

   void AddQuoteBar(double left, double top) =>
      _decorations.Add(new ContainerDecoration.QuoteBar(new Rect(left + _appearance.Scaled(2), top, _appearance.Scaled(3), _top - top), _appearance.LineBrush));

   void AddMarkerOf(ListItem item, IReadOnlyList<BlockContainer> around, double topOfText) =>
      _decorations.Add(new ContainerDecoration.ListMarker(_texts.OfListMarker(MarkerOf(item, around)), new Point(LeftInside(around) + _appearance.Scaled(19), topOfText)));

   static string MarkerOf(ListItem item, IEnumerable<BlockContainer> around) =>
      item.Number is {} number ? number.ToString(CultureInfo.CurrentCulture) + "." : Bullets[around.OfType<ListItem>().Count() % Bullets.Length];
}
