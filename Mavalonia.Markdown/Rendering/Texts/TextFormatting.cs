using JetBrains.Annotations;

namespace Mavalonia.Markdown.Rendering.Texts;

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

   ///<summary>Whether white space formatted so looks the same as white space formatted as <paramref name="other"/>: whether both are code, or neither is.</summary>
   ///<remarks>
   /// Code shows on white space, in its background and in the width of a space of its font. Bold and italic don't show on it.<br/>
   /// Strikethrough shows on it too, but markdown can't start or end strikethrough at white space, so a change written anew moves it on white space to fit the chars around it: counting it would make some changes look wrong that are right.<br/>
   /// The underline of a link shows on white space as well, and isn't counted either.
   ///</remarks>
   internal bool LooksTheSameOnWhiteSpaceAs(TextFormatting other) => Has(TextFormats.Code) == other.Has(TextFormats.Code);

   [PublicAPI]
   public bool Has(TextFormats format) => Formats.HasFlag(format);

   internal TextFormatting With(TextFormats format) => this with { Formats = Formats | format };
   internal TextFormatting Without(TextFormats format) => this with { Formats = Formats & ~format };
   internal TextFormatting WithToggled(TextFormats format) => Has(format) ? Without(format) : With(format);
}
