using JetBrains.Annotations;
using Mavalonia.Markdown.Rendering.Containers;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Rendering.Blocks;

///<summary>A cell of a <see cref="Blocks.Table"/>.</summary>
public class TableCell : RenderedBlock
{
   internal TableCell(BlockContainers containers, RenderedText text, Table table, int row, int column, bool isInTheHeader) : base(containers, text, table.Source)
   {
      Table = table;
      Row = row;
      Column = column;
      IsInTheHeader = isInTheHeader;
   }

   [PublicAPI]
   public Table Table { get; }
   public int Row { get; }
   public int Column { get; }
   public bool IsInTheHeader { get; }

   internal override bool EndsWithItsLastLine => false;

   internal override bool IsTheSameKindAs(RenderedBlock other) => other is TableCell cell && (cell.Row, cell.Column) == (Row, Column);
}
