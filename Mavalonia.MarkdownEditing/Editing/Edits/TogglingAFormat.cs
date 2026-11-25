using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Writing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

///<summary>Formats the selected text, or, when all of it has the format already, takes the format away.</summary>
class TogglingAFormat
{
   readonly TextFormats _format;
   readonly IReadOnlyList<RenderedRange> _texts;
   readonly bool _allHaveTheFormat;

   internal TogglingAFormat(RenderedRange selected, TextFormats format)
   {
      _format = format;
      _texts = [..selected.PartInEachBlock.Where(it => !it.IsEmpty && it.Start.Block is Paragraph or Heading or TableCell)];
      _allHaveTheFormat = _texts.SelectMany(it => FormattedText.In(it).Characters).All(it => it.IsWhiteSpace || it.Formatting.Has(format));
   }

   ///<summary>The changes to make, one after the other: each is made to what is rendered after the one before it. Null where no change looks right.</summary>
   internal IEnumerable<Func<RenderedMarkdown, SourceChange?>> Changes() =>
      _texts.Select(it => it.ByNumbers).Select<RangeByNumbers, Func<RenderedMarkdown, SourceChange?>>(text => rendered => Toggle(text.In(rendered)));

   SourceChange? Toggle(RenderedRange text) => new TextReplacement(text, FormattedText.In(text).Select(it => _allHaveTheFormat ? it.Without(_format) : it.With(_format))).ThatLooksRight();
}
