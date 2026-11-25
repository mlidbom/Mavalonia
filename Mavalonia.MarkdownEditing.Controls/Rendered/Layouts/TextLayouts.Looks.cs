using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;

partial class TextLayouts
{
   ///<summary>All that decides how a text is laid out.</summary>
   // ReSharper disable NotAccessedPositionalProperty.Local : they are what makes two equal.
   readonly record struct Looks(string Text, string Formatting, string KindOfBlock, double Width)
   {
      internal static Looks Of(RenderedBlock block, double width) => new(block.Text.Value, new string([..block.Text.Chars.Select(it => InOneChar(it.Formatting))]), KindOf(block), width);

      static char InOneChar(TextFormatting formatting) => (char)((int)formatting.Formats + (formatting.Link is null ? 0 : 16) + (formatting.IsShownAsSource ? 32 : 0));

      static string KindOf(RenderedBlock block) => block switch
      {
         Heading heading => $"heading {heading.Level}",
         TableCell { IsInTheHeader: true } => "header",
         CodeBlock => "code",
         _ => "text"
      };
   }
}
