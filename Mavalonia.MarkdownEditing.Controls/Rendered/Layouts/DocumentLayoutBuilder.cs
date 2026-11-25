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
   double _top;

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

   void LayOutBlocks()
   {
      RenderedBlock? previous = null;
      foreach(var block in _rendered.Blocks)
      {
         if(block is TableCell { Row: 0, Column: 0 } firstCell) LayOut(firstCell.Table, previous);
         else if(block is not TableCell) LayOut(block, previous);

         previous = block;
      }
   }

   void LayOut(RenderedBlock block, RenderedBlock? previous)
   {
      var bottomOfPrevious = LeaveSpaceBetween(previous, block);
      var padding = _appearance.Scaled(block is CodeBlock ? 8 : 0);
      var left = _area.Left + IndentationInside(block.Containers.OutermostFirst);
      var text = _texts.Of(block, Math.Max(_appearance.Scaled(40), _area.Right - left - 2 * padding));
      var bounds = new Rect(left, _top, Math.Max(0, _area.Right - left), text.Height + 2 * padding);

      _blocks.Add(new BlockLayout(block, bounds, text, bounds.TopLeft + new Point(padding, padding), _appearance));
      _top = bounds.Bottom;
      Decorate(block, previous, bottomOfPrevious, topOfText: bounds.Top + padding);
   }

   void LayOut(Table table, RenderedBlock? previous)
   {
      var bottomOfPrevious = LeaveSpaceBetween(previous, table.FirstCell);
      var left = _area.Left + IndentationInside(table.FirstCell.Containers.OutermostFirst);
      var top = _top;
      var tableLayout = new TableLayout(table, _appearance, _texts, new Point(left, top), _area.Right - left);

      _blocks.AddRange(tableLayout.Cells);
      _top = tableLayout.Bottom;
      Decorate(table.FirstCell, previous, bottomOfPrevious, topOfText: top);
   }

   ///<summary>Returns where the previous block ends.</summary>
   double LeaveSpaceBetween(RenderedBlock? previous, RenderedBlock block)
   {
      var bottomOfPrevious = _top;
      if(previous is not null) _top += _appearance.Scaled(AreItemsOfTheSameList(previous, block) ? 3 : block is Heading ? 12 : 8);
      return bottomOfPrevious;
   }

   static bool AreItemsOfTheSameList(RenderedBlock previous, RenderedBlock block) => previous.Containers.OutermostFirst.OfType<ListItem>().Any() && block.Containers.Innermost is ListItem item && !previous.IsInside(item);

   double IndentationInside(IEnumerable<BlockContainer> containers) => containers.Sum(it => _appearance.Scaled(it is Quote ? 16 : 24));

   void Decorate(RenderedBlock block, RenderedBlock? previous, double bottomOfPrevious, double topOfText)
   {
      foreach(var (depth, container) in block.Containers.OutermostFirst.Index())
      {
         var left = _area.Left + IndentationInside(block.Containers.OutermostFirst.Take(depth));
         var isFirst = previous?.IsInside(container) != true;

         switch(container)
         {
            case Quote:
               var top = isFirst ? topOfText : bottomOfPrevious;
               _decorations.Add(new ContainerDecoration.QuoteBar(new Rect(left + _appearance.Scaled(2), top, _appearance.Scaled(3), _top - top), _appearance.LineBrush));
               break;
            case ListItem item when isFirst:
               _decorations.Add(new ContainerDecoration.ListMarker(_texts.OfListMarker(MarkerOf(item, block.Containers.OutermostFirst.Take(depth))), new Point(left + _appearance.Scaled(19), topOfText)));
               break;
         }
      }
   }

   static string MarkerOf(ListItem item, IEnumerable<BlockContainer> around) =>
      item.Number is {} number ? number.ToString(CultureInfo.CurrentCulture) + "." : Bullets[around.OfType<ListItem>().Count() % Bullets.Length];
}
