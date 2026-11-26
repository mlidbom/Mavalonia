using AngleSharp.Dom;
using Mavalonia.Markdown.Editing.Writing;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Html;

///<summary>Reads HTML as text on one line, for the blocks that can't have more lines than that.</summary>
static class HtmlOnOneLine
{
   ///<param name="element">What is read.</param>
   ///<param name="formatting">How text right inside the element is formatted.</param>
   internal static FormattedText TextIn(IElement element, TextFormatting formatting)
   {
      var text = new TextCollector();
      AddTextIn(element, formatting);
      return text.Take();

      void AddTextIn(INode node, TextFormatting formattingInside)
      {
         foreach(var child in node.ChildNodes)
            if(child is IText childText)
               text.Add(childText.Data, formattingInside);
            else if(child is IElement { IsHidden: false } childElement)
               Add(childElement, childElement.Format(formattingInside));
      }

      void Add(IElement childElement, TextFormatting formattingInside)
      {
         var spaceAround = childElement.IsABlock || childElement.LocalName == "br" ? " " : "";

         text.Add(spaceAround + childElement.TextThatStandsForIt, formattingInside);
         AddTextIn(childElement, formattingInside);
         text.Add(spaceAround, formattingInside);
      }
   }
}
