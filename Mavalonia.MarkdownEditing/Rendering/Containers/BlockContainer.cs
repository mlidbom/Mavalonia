namespace Mavalonia.MarkdownEditing.Rendering.Containers;

///<summary>Something that blocks can be inside: a quote or a list item.<br/>
/// The blocks inside the same quote or list item refer to the same container.</summary>
public abstract class BlockContainer
{
   ///<summary>What precedes the text on the first line inside the container, such as <c>- </c>.</summary>
   internal abstract string PrefixOfItsFirstLine { get; }

   ///<summary>What precedes the text on every other line inside the container, such as two spaces.</summary>
   internal abstract string PrefixOfItsOtherLines { get; }

   ///<summary>The offset after the container's prefix on one of its lines other than the first, where the prefix would start at <paramref name="offset"/>.<br/>
   /// Null when it isn't there, so that the line is not inside the container.</summary>
   internal abstract int? EndOfItsPrefix(string markdown, int offset);

   ///<summary>Whether <paramref name="other"/> is the same kind of container, so that blocks inside either look alike.</summary>
   internal abstract bool LooksLike(BlockContainer other);
}
