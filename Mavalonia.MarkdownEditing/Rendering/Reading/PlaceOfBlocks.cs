using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Where blocks are.</summary>
///<param name="Containers">The containers they are inside.</param>
///<param name="Column">The column that text inside those containers starts at.</param>
readonly record struct PlaceOfBlocks(BlockContainers Containers, int Column)
{
   internal static readonly PlaceOfBlocks OutsideAllContainers = new(BlockContainers.None, 0);

   internal PlaceOfBlocks In(BlockContainer container, int column) => new(Containers.Inside(container), column);
}
