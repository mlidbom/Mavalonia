namespace Mavalonia.MarkdownEditing.Rendering.Texts;

///<summary>How a char is formatted.</summary>
///<param name="Formats">Such as bold.</param>
///<param name="Link">The link whose label the char is in, if any.</param>
///<param name="Image">The image that the char describes, if any.</param>
///<param name="IsShownAsSource">Whether the char belongs to markdown that isn't rendered but shown as it is written, such as inline HTML.</param>
public readonly record struct TextFormatting(TextFormats Formats, Link? Link = null, bool IsShownAsSource = false, InlineImage? Image = null)
{
   public static readonly TextFormatting None = new(TextFormats.None);

   ///<summary>Whether text formatted so looks the same as text formatted as <paramref name="other"/>.</summary>
   internal bool LooksLike(TextFormatting other) => Formats == other.Formats && Link?.Target == other.Link?.Target && IsShownAsSource == other.IsShownAsSource && Image?.Source == other.Image?.Source;

   public bool Has(TextFormats format) => Formats.HasFlag(format);

   internal TextFormatting With(TextFormats format) => this with { Formats = Formats | format };
   internal TextFormatting Without(TextFormats format) => this with { Formats = Formats & ~format };
   internal TextFormatting WithToggled(TextFormats format) => Has(format) ? Without(format) : With(format);
}
