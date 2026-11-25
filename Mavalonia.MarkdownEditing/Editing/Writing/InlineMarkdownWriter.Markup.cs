using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing.Writing;

partial class InlineMarkdownWriter
{
   ///<summary>Markup that gives chars a format, or makes them a link.</summary>
   ///<param name="Format">The format. None for a link.</param>
   ///<param name="Link">The link. Null for a format.</param>
   readonly record struct Markup(TextFormats Format, Link? Link)
   {
      internal static IEnumerable<Markup> Of(FormattedChar formattedChar) =>
         [
            ..FormatsWithDelimiters.Append(TextFormats.Code).Where(it => formattedChar.Formatting.Has(it)).Select(it => new Markup(it, null)),
            ..formattedChar.Formatting.Link is {} link ? [new Markup(TextFormats.None, link)] : Array.Empty<Markup>()
         ];

      internal bool IsCode => Format == TextFormats.Code;

      internal bool Formats(FormattedChar formattedChar) => Link is null ? formattedChar.Formatting.Has(Format) : formattedChar.Formatting.Link == Link;
   }
}
