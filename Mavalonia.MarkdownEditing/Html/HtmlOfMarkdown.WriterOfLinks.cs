using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;
using Markdig.Syntax.Inlines;

namespace Mavalonia.MarkdownEditing.Html;

static partial class HtmlOfMarkdown
{
   ///<summary>Writes an image as a link to it.</summary>
   ///<remarks>
   /// A program that it's pasted into would load the image, whoever wrote it. What an AI writes can hold one whose address carries what it knows to somebody else.
   /// </remarks>
   class WriterOfLinks : LinkInlineRenderer
   {
      protected override void Write(HtmlRenderer renderer, LinkInline link)
      {
         if(!link.IsImage) base.Write(renderer, link);
         else if(IsInALink(link)) renderer.WriteChildren(link);
         else WriteAsALink(renderer, link);
      }

      static bool IsInALink(LinkInline image) => image.Parent is LinkInline { IsImage: false } || (image.Parent is {} parent && parent.FindParentOfType<LinkInline>().Any(it => !it.IsImage));

      static void WriteAsALink(HtmlRenderer renderer, LinkInline image)
      {
         renderer.Write("<a href=\"").WriteEscapeUrl(image.GetDynamicUrl?.Invoke() ?? image.Url).Write("\">");
         if(image.FirstChild is null) renderer.Write("image");
         renderer.WriteChildren(image);
         renderer.Write("</a>");
      }
   }
}
