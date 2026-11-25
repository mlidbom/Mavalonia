namespace Mavalonia.MarkdownEditing.Rendering.Texts;

///<summary>A link in a markdown text. The characters that are its label refer to it.</summary>
public class Link
{
   internal Link(string target, string opening, string closing)
   {
      Target = target;
      Opening = opening;
      Closing = closing;
   }

   ///<summary>Where the link leads.</summary>
   public string Target { get; }

   ///<summary>The markdown before the link's label, such as <c>[</c>.</summary>
   internal string Opening { get; }

   ///<summary>The markdown after the link's label, such as <c>](https://example.org)</c>.</summary>
   internal string Closing { get; }
}
