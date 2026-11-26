using System.Text.RegularExpressions;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Mavalonia.Markdown.Rendering.Reading;

///<summary>The HTML that isn't shown as its source where markdown is only read.</summary>
static partial class HtmlInMarkdownEX
{
   extension(HtmlInline @this)
   {
      internal bool IsALineBreak => TagOfALineBreak().IsMatch(@this.Tag);
      internal bool IsAComment => @this.Tag.StartsWith("<!--", StringComparison.Ordinal);
   }

   extension(HtmlBlock @this)
   {
      ///<summary>A comment that isn't ended goes on to the end of the markdown.</summary>
      internal bool IsNothingButComments => @this.Type == HtmlBlockType.Comment && Comments().IsMatch(@this.Lines.ToString());
   }

   [GeneratedRegex(@"^<br\s*/?>$", RegexOptions.IgnoreCase)]
   private static partial Regex TagOfALineBreak();

   [GeneratedRegex(@"^(\s|<!--(-?>|((?!-->).)*(-->|\z)))*\z", RegexOptions.Singleline | RegexOptions.ExplicitCapture)]
   private static partial Regex Comments();
}
