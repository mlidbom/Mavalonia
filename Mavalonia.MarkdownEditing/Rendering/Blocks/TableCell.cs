using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Rendering.Blocks;

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

   public Table Table { get; }
   public int Row { get; }
   public int Column { get; }
   public bool IsInTheHeader { get; }

   internal override bool EndsWithItsLastLine => false;

   internal override bool IsTheSameKindAs(RenderedBlock other) => other is TableCell cell && (cell.Row, cell.Column) == (Row, Column);
}
