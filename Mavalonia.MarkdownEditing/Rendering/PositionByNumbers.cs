namespace Mavalonia.MarkdownEditing.Rendering;

///<summary>A place where a caret can be, said by numbers alone: in the text of block number <paramref name="BlockNumber"/>, before its char number <paramref name="Index"/>.</summary>
///<remarks>
/// A <see cref="RenderedPosition"/> is in what is rendered now. Its numbers say where the same place is in what is rendered after a change.
/// </remarks>
readonly record struct PositionByNumbers(int BlockNumber, int Index)
{
   internal static PositionByNumbers AtTheStartOf(int blockNumber) => new(blockNumber, 0);
}
