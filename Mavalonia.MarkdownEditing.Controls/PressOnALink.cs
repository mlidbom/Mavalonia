using Avalonia;

namespace Mavalonia.MarkdownEditing.Controls;

///<summary>The button has been pressed on a link, or on an image, that leads to <see cref="Target"/>. Released on it again, near where it was pressed, it has clicked it.</summary>
///<param name="At">Where the button was pressed.</param>
///<param name="Target">Where the link, or the image, leads.</param>
readonly record struct PressOnALink(Point At, string Target)
{
   ///<summary>Whether releasing the button at <paramref name="releasedAt"/>, on what leads to <paramref name="targetThere"/>, clicks the link.</summary>
   internal bool IsClickedBy(Point releasedAt, string? targetThere) => releasedAt.IsWithinAClickOf(At) && targetThere == Target;
}
