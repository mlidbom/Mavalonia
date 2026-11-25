using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

///<summary>Formats the selected text, or, when all of it has the format already, takes the format away.</summary>
class TogglingAFormat
{
   readonly RenderedMarkdown _rendered;
   readonly TextFormats _format;
   readonly IReadOnlyList<SelectedText> _texts;

   internal TogglingAFormat(RenderedMarkdown rendered, RenderedPosition from, RenderedPosition to, TextFormats format)
   {
      _rendered = rendered;
      _format = format;

      var (first, last) = (rendered.NumberOf(from.Block), rendered.NumberOf(to.Block));
      _texts =
      [
         ..Enumerable.Range(first, last - first + 1)
                     .Select(it => new SelectedText(it, Start: it == first ? from.Index : 0, End: it == last ? to.Index : rendered.Blocks[it].Text.Length))
                     .Where(it => it.Start < it.End && rendered.Blocks[it.BlockNumber] is Paragraph or Heading or TableCell)
      ];
   }

   ///<summary>The changes to make, one after the other: each is made to what is rendered after the one before it.</summary>
   internal IEnumerable<Func<RenderedMarkdown, SourceChange?>> Changes()
   {
      var allHaveIt = _texts.SelectMany(it => CharactersOf(it, _rendered).Characters).All(it => it.IsWhiteSpace || it.Formatting.Has(_format));

      return _texts.Select<SelectedText, Func<RenderedMarkdown, SourceChange?>>(text => rendered => Change(rendered, text, allHaveIt ? it => it.Without(_format) : it => it.With(_format)));
   }

   static SourceChange? Change(RenderedMarkdown rendered, SelectedText text, Func<FormattedCharacter, FormattedCharacter> change)
   {
      var block = rendered.Blocks[text.BlockNumber];
      return new TextReplacement(rendered, new RenderedPosition(block, text.Start), new RenderedPosition(block, text.End), CharactersOf(text, rendered).Select(change)).ThatLooksRight();
   }

   static FormattedText CharactersOf(SelectedText text, RenderedMarkdown rendered) => FormattedText.Of(rendered, rendered.Blocks[text.BlockNumber])[text.Start..text.End];

   readonly record struct SelectedText(int BlockNumber, int Start, int End);
}
