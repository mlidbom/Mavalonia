using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing.Writing;

partial class InlineMarkdownWriter
{
   ///<summary>Markup that gives characters a format, or makes them a link.</summary>
   ///<param name="Format">The format. None for a link.</param>
   ///<param name="Link">The link. Null for a format.</param>
   readonly record struct Markup(TextFormats Format, Link? Link)
   {
      internal static IEnumerable<Markup> Of(FormattedCharacter character) =>
         [
            ..FormatsWithDelimiters.Append(TextFormats.Code).Where(it => character.Formatting.Has(it)).Select(it => new Markup(it, null)),
            ..character.Formatting.Link is {} link ? [new Markup(TextFormats.None, link)] : Array.Empty<Markup>()
         ];

      internal bool IsCode => Format == TextFormats.Code;

      internal bool Formats(FormattedCharacter character) => Link is null ? character.Formatting.Has(Format) : character.Formatting.Link == Link;
   }
}
