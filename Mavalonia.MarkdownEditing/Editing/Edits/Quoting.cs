using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Edits.NewBlocks;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

///<summary>Adds a quote where the caret is, and puts the caret in a paragraph after it, to write there what there is to say about the quote.</summary>
class Quoting
{
   readonly Caret _caret;

   internal Quoting(Caret caret) => _caret = caret;

   ///<summary>Whether a quote can be added where the caret is. Where it can't, such as in code or a table, it goes after that.</summary>
   internal bool IsPossible => _caret.Block is Paragraph or Heading;

   ///<summary>Goes to where a quote can be added.</summary>
   internal SourceChange GoToWhereItIsPossible() => new GoingToAnEmptyParagraphAfter(_caret.Rendered, _caret.Block is TableCell cell ? cell.Table.LastCell : _caret.Block).Change();

   internal SourceChange Of(string markdown) => new InsertingBlocks(_caret, string.Join('\n', LinesOfAQuoteOf(markdown))).WithTheCaretAfterThem();

   static IEnumerable<string> LinesOfAQuoteOf(string markdown) => markdown.Trim('\r', '\n').ReplaceLineEndings("\n").Split('\n').Select(it => it.Length == 0 ? ">" : "> " + it);
}
