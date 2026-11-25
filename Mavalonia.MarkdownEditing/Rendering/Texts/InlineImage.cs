namespace Mavalonia.MarkdownEditing.Rendering.Texts;

///<summary>An image in a markdown text. The chars of its description refer to it, and stand for all of its markup together.</summary>
public class InlineImage
{
   internal InlineImage(string source) => Source = source;

   ///<summary>Where the image is, as the markdown says it.</summary>
   public string Source { get; }
}
