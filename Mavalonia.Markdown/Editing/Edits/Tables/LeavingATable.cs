using Mavalonia.Markdown.Editing.Changes;
using Mavalonia.Markdown.Editing.Edits.NewBlocks;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Editing.Edits.Tables;

///<summary>Goes to a paragraph after a table. When its last row is empty, that goes away: it was added to be written in.</summary>
class LeavingATable
{
   readonly RenderedMarkdown _rendered;
   readonly Table _table;

   internal LeavingATable(RenderedMarkdown rendered, Table table)
   {
      _rendered = rendered;
      _table = table;
   }

   internal SourceChange Change()
   {
      var toAParagraph = new GoingToAnEmptyParagraphAfter(_rendered, _table.LastCell);
      return IsTheLastRowEmpty ? toAParagraph.AddAfterReplacing(SourceOfTheLastRow, by: "") : toAParagraph.Change();
   }

   IReadOnlyList<TableCell> LastRow => [.._table.Cells.Where(it => it.Row == _table.LastCell.Row)];

   bool IsTheLastRowEmpty => LastRow.All(it => it is { Text.IsEmpty: true, IsInTheHeader: false });

   ///<summary>With the line break before it.</summary>
   SourceRange SourceOfTheLastRow => new(_rendered.Source.EndOfLineBefore(LastRow[0].Text.Source.Start), _table.Source.End);
}
