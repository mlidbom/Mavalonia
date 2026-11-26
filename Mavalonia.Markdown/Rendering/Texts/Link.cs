namespace Mavalonia.Markdown.Rendering.Texts;

///<summary>A link in a markdown text. The chars that are its label refer to it.</summary>
public class Link
{
   internal Link(string target, string opening, string closing, string? definition = null)
   {
      Definition = definition;
      Target = target;
      Opening = opening;
      Closing = closing;
   }

   ///<summary>Where the link leads.</summary>
   public string Target { get; }

   ///<summary>The markdown elsewhere in the text that says where the link leads, such as <c>[a]: https://example.org</c>. Null when the link itself says it.</summary>
   internal string? Definition { get; }

   ///<summary>The markdown before the link's label, such as <c>[</c>.</summary>
   internal string Opening { get; }

   ///<summary>The markdown after the link's label, such as <c>](https://example.org)</c>.</summary>
   internal string Closing { get; }
}
