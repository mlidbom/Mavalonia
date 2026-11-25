using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Editing;

///<summary>A markdown text that a person reads, and selects in to copy out of it.</summary>
public sealed class MarkdownToRead : SelectableMarkdown
{
   RenderedMarkdown _rendered;

   public MarkdownToRead(string markdown, MarkdownDialect dialect) => _rendered = RenderedMarkdown.Of(markdown, dialect);

   public override RenderedMarkdown Rendered => _rendered;

   ///<summary>Shows <paramref name="markdown"/> instead. What is selected stays so when the text up to its end is the same, as when the text is still being written.</summary>
   public void Show(string markdown, MarkdownDialect dialect)
   {
      var isTheSameUpToTheEndOfTheSelection = dialect == _rendered.Dialect && markdown.AsSpan().CommonPrefixLength(_rendered.Source) >= Selection.End;

      _rendered = RenderedMarkdown.Of(markdown, dialect);
      Selection = isTheSameUpToTheEndOfTheSelection ? WhereItIsShown(Selection) : TextSelection.CaretAt(Rendered.Start.Offset);
   }

   ///<summary>Selects nothing.</summary>
   public void SelectNothing() => MoveTo(CaretPosition);
}
