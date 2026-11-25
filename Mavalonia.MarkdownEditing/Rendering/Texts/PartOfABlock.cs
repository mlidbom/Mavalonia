using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Rendering.Texts;

///<summary>A part of the text of a block: from the char at <paramref name="Start"/> up to that at <paramref name="End"/>.</summary>
public readonly record struct PartOfABlock(RenderedBlock Block, int Start, int End);
