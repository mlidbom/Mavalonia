using Avalonia.Media;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.CodeColours;

///<summary>A part of some code that is drawn in a colour of its own.</summary>
readonly record struct ColouredStretch(int Start, int Length, IBrush Foreground)
{
   internal int End => Start + Length;
}
