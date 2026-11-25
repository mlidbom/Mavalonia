using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Specifications.Rendering;

///<summary>Describes what is rendered, so that specifications can say it in strings.</summary>
static class RenderedMarkdownDescriptionEX
{
   extension(RenderedMarkdown @this)
   {
      ///<summary>Each block as its containers, its kind and its text, such as <c>quote > • > paragraph "text"</c>.</summary>
      internal IReadOnlyList<string> Described => [..@this.Blocks.Select(it => it.Described)];
   }

   extension(RenderedBlock @this)
   {
      string Described => string.Concat(@this.Containers.OutermostFirst.Select(it => it.Described + " > ")) + $"{@this.Kind} \"{@this.Text.Value}\"";

      string Kind => @this switch
      {
         Paragraph => "paragraph",
         Heading heading => $"heading {heading.Level}",
         CodeBlock code => $"code {code.Language}".TrimEnd(),
         ThematicBreak => "break",
         SourceBlock => "source",
         TableCell cell => $"{(cell.IsInTheHeader ? "header" : "cell")} {cell.Row},{cell.Column}",
         _ => throw new ArgumentOutOfRangeException(nameof(@this), @this.GetType().Name)
      };
   }

   extension(BlockContainer @this)
   {
      string Described => @this switch
      {
         Quote => "quote",
         ListItem { Number: {} number } => $"{number}.",
         ListItem => "•",
         _ => throw new ArgumentOutOfRangeException(nameof(@this), @this.GetType().Name)
      };
   }

   extension(RenderedText @this)
   {
      ///<summary>The text with each stretch that is formatted in brackets, such as <c>a [Bold: b] c</c>.</summary>
      internal string DescribedWithFormatting => string.Concat(@this.StretchesOfTheSameFormatting.Select(it => it.DescribedIn(@this)));
   }

   extension(StretchOfText @this)
   {
      string DescribedIn(RenderedText text)
      {
         var characters = text.Value.Substring(@this.Start, @this.Length);
         return @this.Formatting == TextFormatting.None ? characters : $"[{@this.Formatting.Described}: {characters}]";
      }
   }

   extension(TextFormatting @this)
   {
      string Described => string.Join(", ", new[] { @this.Formats == TextFormats.None ? null : @this.Formats.ToString(), @this.Link is {} link ? $"link to {link.Target}" : null, @this.IsShownAsSource ? "source" : null }.OfType<string>());
   }
}
