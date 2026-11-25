namespace Mavalonia.MarkdownEditing.Rendering.Texts;

///<summary>How a character is formatted.</summary>
///<param name="Formats">Such as bold.</param>
///<param name="Link">The link whose label the character is in, if any.</param>
///<param name="IsShownAsSource">Whether the character belongs to markdown that isn't rendered but shown as it is written, such as inline HTML.</param>
public readonly record struct TextFormatting(TextFormats Formats, Link? Link = null, bool IsShownAsSource = false)
{
   public static readonly TextFormatting None = new(TextFormats.None);

   public bool Has(TextFormats format) => Formats.HasFlag(format);

   public TextFormatting With(TextFormats format) => this with { Formats = Formats | format };
   public TextFormatting Without(TextFormats format) => this with { Formats = Formats & ~format };
}
