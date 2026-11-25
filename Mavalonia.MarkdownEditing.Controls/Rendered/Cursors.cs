using Avalonia.Input;

namespace Mavalonia.MarkdownEditing.Controls.Rendered;

///<summary>What the pointer looks like.</summary>
static class Cursors
{
   internal static readonly Cursor OfText = new(StandardCursorType.Ibeam);
   internal static readonly Cursor OfALink = new(StandardCursorType.Hand);
}
