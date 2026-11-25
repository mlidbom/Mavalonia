namespace Mavalonia.MarkdownEditing.Rendering.Blocks;

///<summary>A table. Its cells are the blocks: a table as such has no text.</summary>
public class Table
{
   readonly List<TableCell> _cells = [];

   internal Table(SourceRange source) => Source = source;

   ///<summary>The whole of the table in the markdown.</summary>
   internal SourceRange Source { get; }

   ///<summary>The cells, row by row.</summary>
   public IReadOnlyList<TableCell> Cells => _cells;

   public TableCell FirstCell => _cells[0];
   internal TableCell LastCell => _cells[^1];

   public int ColumnCount => _cells.Max(it => it.Column) + 1;

   internal void Add(TableCell cell) => _cells.Add(cell);

   ///<summary>The cell of the row after that of <paramref name="cell"/>, in the same column. Null when there is none.</summary>
   internal TableCell? CellBelow(TableCell cell) => _cells.FirstOrDefault(it => it.Row == cell.Row + 1 && it.Column == cell.Column);
}
