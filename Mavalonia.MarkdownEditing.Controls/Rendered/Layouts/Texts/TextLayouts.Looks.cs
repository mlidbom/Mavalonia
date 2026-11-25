using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts.Texts;

partial class TextLayouts
{
   ///<summary>All that decides how a text is laid out, but for the width.</summary>
   // ReSharper disable NotAccessedPositionalProperty.Local : they are what makes two equal.
   readonly record struct Looks(string Text, string Formatting, string KindOfBlock, string Images)
   {
      internal bool DependsOnTheWidth => Images.Length > 0;

      internal static Looks Of(RenderedBlock block, MarkdownAppearance appearance) =>
         new(block.Text.Value, new string([..block.Text.Chars.Select(it => InOneChar(it.Formatting))]), KindOf(block), ImagesIn(block, appearance));

      static char InOneChar(TextFormatting formatting) => (char)((int)formatting.Formats + (formatting.Link is null ? 0 : 16) + (formatting.IsShownAsSource ? 32 : 0) + (formatting.Image is null ? 0 : 64));

      ///<summary>Which of the images in the block are there to be shown.</summary>
      static string ImagesIn(RenderedBlock block, MarkdownAppearance appearance) =>
         string.Join(' ', block.Text.Chars.Select(it => it.Formatting.Image).OfType<InlineImage>().Distinct().Where(it => appearance.ImageOf(it) is not null).Select(it => it.Source));

      static string KindOf(RenderedBlock block) => block switch
      {
         Heading heading => $"heading {heading.Level}",
         TableCell { IsInTheHeader: true } => "header",
         CodeBlock => "code",
         AlertTitle title => title.Text.Value,
         _ => "text"
      };
   }
}
