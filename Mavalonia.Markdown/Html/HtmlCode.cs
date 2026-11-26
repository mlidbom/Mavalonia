using System.Text;
using AngleSharp.Dom;

namespace Mavalonia.Markdown.Html;

///<summary>Reads HTML as code: with all its white space.</summary>
static class HtmlCode
{
   const char NoBreakSpace = '\u00A0';

   internal static string TextIn(IElement element)
   {
      var code = new StringBuilder();
      AddTextIn(element);
      return code.ToString().ReplaceLineEndings("\n").Replace(NoBreakSpace, ' ').TrimEnd('\n').TrimStart('\n');

      void AddTextIn(INode node)
      {
         foreach(var child in node.ChildNodes)
            if(child is IText text) code.Append(text.Data);
            else if(child is IElement { LocalName: "br" }) code.Append('\n');
            else if(child is IElement { IsABlock: true } block) AddLine(block); // Some editors have each line of code in an element of its own.
            else AddTextIn(child);
      }

      void AddLine(IElement block)
      {
         if(code.Length > 0 && code[^1] != '\n') code.Append('\n');
         AddTextIn(block);
         if(block.TextContent.Length > 0) code.Append('\n');
      }
   }
}
