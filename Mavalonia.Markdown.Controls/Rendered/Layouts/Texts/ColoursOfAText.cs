using Mavalonia.Markdown.Controls.Rendered.CodeColours;

namespace Mavalonia.Markdown.Controls.Rendered.Layouts.Texts;

///<summary>The colours that the text of a block is drawn in besides those it is laid out in: those of its code, and those of the links that the application shows in it, which are underlined too.</summary>
///<remarks>
/// Neither changes any size, as the text is drawn in them on the lines that it is laid out in without them: see <see cref="LaidOutText"/>.
///</remarks>
///<param name="OfCode">The colours of the code that the text is. Null when it isn't code, or its colours aren't known.</param>
///<param name="Links">Where the application shows links in it.</param>
readonly record struct ColoursOfAText(ColouredCode? OfCode, LinkedStretches Links)
{
   internal bool AreAny => OfCode is not null || Links.AreAny;
}
