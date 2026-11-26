using Avalonia.Interactivity;
using Mavalonia.Markdown.Rendering;

namespace Mavalonia.Markdown.Controls;

///<summary>Tells that a person has pasted into a <see cref="MarkdownEditor"/>, and where in its document.</summary>
public class PastedEventArgs : RoutedEventArgs
{
   internal PastedEventArgs(SourceRange written) : base(MarkdownEditor.PastedEvent) => Written = written;

   ///<summary>The stretch of the markdown of <see cref="MarkdownEditor.Document"/> that the paste wrote, as it is right after the paste.</summary>
   ///<remarks>In the rendered view, it may hold markup around what was pasted that was written anew, such as where bold text is split by a pasted list.</remarks>
   public SourceRange Written { get; }
}
