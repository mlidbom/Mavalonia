using System.Text.RegularExpressions;
using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits.Tables;

///<summary>Makes a table, with an empty row last, of a paragraph that is written as a table: as its header, such as <c>| a | b |</c>, or as all of its lines.</summary>
partial class MakingATable
{
   readonly Caret _caret;
   readonly IReadOnlyList<IReadOnlyList<FormattedText>> _rows;

   MakingATable(Caret caret, IReadOnlyList<IReadOnlyList<FormattedText>> rows)
   {
      _caret = caret;
      _rows = rows;
   }

   ///<summary>Null when the caret isn't at the end of a paragraph that is written as a table.</summary>
   internal static MakingATable? Of(Caret caret)
   {
      if(caret is not { Block: Paragraph, IsAtTheEndOfItsBlock: true }) return null;

      var lines = FormattedText.Of(caret.Rendered, caret.Block).AsIfTyped.SplitAt(it => it.Value == '\n');
      if(lines.Count > 1 && !LineUnderAHeader().IsMatch(lines[1].Value)) return null;

      IReadOnlyList<IReadOnlyList<FormattedText>> rows = [..lines.Take(1).Concat(lines.Skip(2)).Select(CellsOf)];
      return rows.All(it => it.Count > 1) ? new MakingATable(caret, rows) : null;
   }

   ///<summary>None when the line isn't written as a row.</summary>
   static IReadOnlyList<FormattedText> CellsOf(FormattedText line)
   {
      var parts = line.Trimmed.SplitAt(it => it is { Value: '|', IsCode: false });
      var isARow = parts.Count > 2 && parts[0].Length == 0 && parts[^1].Length == 0;

      return isARow ? [..parts.Skip(1).SkipLast(1).Select(it => it.Trimmed)] : [];
   }

   internal SourceChange Change()
   {
      var rendered = _caret.Rendered;
      var lineBreaks = new LineBreaksInside(rendered, _caret.Block.Containers);
      var lines = new MarkdownOfATable([.._rows, [FormattedText.Empty]]).Lines;
      var separatorFromNext = rendered.IsFollowedByABlockOnTheNextLine(_caret.Block) ? lineBreaks.BlankLine : "";

      return SourceChange.Replace(rendered, _caret.Block.Source, lineBreaks.Between(lines) + separatorFromNext)
                         .WithTheCaretAt(PositionByNumbers.AtTheStartOf(rendered.NumberOf(_caret.Block) + _rows.Count * _rows.Max(it => it.Count)));
   }

   [GeneratedRegex(@"^\s*\|(\s*:?-+:?\s*\|)+\s*$")]
   private static partial Regex LineUnderAHeader();
}
