using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits.Tables;

///<summary>Adds an empty row at the end of a table, and goes to its first cell.</summary>
class AddingARow
{
   readonly RenderedMarkdown _rendered;
   readonly Table _table;

   internal AddingARow(RenderedMarkdown rendered, Table table)
   {
      _rendered = rendered;
      _table = table;
   }

   internal SourceChange Change()
   {
      var row = "|" + string.Concat(Enumerable.Repeat("  |", _table.ColumnCount));

      return SourceChange.Insert(_rendered, _table.Source.End, new LineBreaksInside(_rendered, _table.LastCell.Containers).NewLine + row)
                         .WithTheCaretAt(PositionByNumbers.AtTheStartOf(_rendered.NumberOf(_table.LastCell) + 1));
   }
}
