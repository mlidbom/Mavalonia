using System.Text;
using System.Text.RegularExpressions;
using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.MarkdownEditing.Html;

namespace Mavalonia.MarkdownEditing.Specifications.Html;

public class Html_for_the_clipboard_of_Windows
{
   readonly byte[] _written = HtmlFormatOfWindows.Write("<p>å <b>b</b> 😀</p>");

   [XF] public void says_where_the_HTML_is() => Between("StartHTML", "EndHTML").Must().Be("<html><body><!--StartFragment--><p>å <b>b</b> 😀</p><!--EndFragment--></body></html>");
   [XF] public void says_where_the_part_that_was_copied_is() => Between("StartFragment", "EndFragment").Must().Be("<p>å <b>b</b> 😀</p>");
   [XF] public void is_read_as_what_was_written() => CopiedHtml.ParseHtmlFormatOfWindows(_written).Markdown.Must().Be("å **b** 😀");

   string Between(string start, string end) => Encoding.UTF8.GetString(_written[OffsetOf(start)..OffsetOf(end)]);
   int OffsetOf(string name) => int.Parse(Regex.Match(Encoding.ASCII.GetString(_written), name + @":(\d+)").Groups[1].Value);
}
