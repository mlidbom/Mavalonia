using System.Net;
using System.Text;
using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.Markdown.Html;
using Mavalonia.Markdown.Rendering;

namespace Mavalonia.Markdown.Specifications.Html;

///<summary>Whatever the HTML, its text must be what the markdown shows: none of it lost, and none of it taken for markup.</summary>
public class Copied_html_at_random
{
   const int CountOfDocuments = 1500;

   static readonly string[] Texts = ["a", "b c", "# d", "- e", "1. f", "2) g", "> h", "*i*", "**", "_j_", "`", "```", "~~k~~", "[l](m)", "[n]: o", "<p>", "&amp;", "\\", "\\*", "|", "a | b", "|---|", "---", "===", "***", "+", "!", "é", "👩‍👩‍👧", "https://x.y", "<!--", "<?x", "~~~", "    p", "\t", "1", ".", ")"];
   static readonly string[] Inlines = ["b", "i", "del", "code", "span", "a href='https://x.y/z'"];
   static readonly string[] Blocks = ["p", "div", "blockquote", "ul", "ol", "h2", "pre", "section", "table"];

   readonly StringBuilder _html = new();
   Random _random = new(0);

   [XF] public void is_shown_with_all_of_its_text_as_text()
   {
      for(var seed = 0; seed < CountOfDocuments; seed++)
      {
         _random = new Random(seed);
         _html.Clear();
         AddBlocks(depth: 0);

         var html = _html.ToString();
         var markdown = CopiedHtml.Parse(html).Markdown;
         new { Shown = TextShownOf(markdown), Expected = TextOf(html), html, markdown }.Must().Satisfy(it => it.Shown == it.Expected);
      }
   }

   static string TextShownOf(string markdown) => WithoutWhiteSpace(string.Concat(RenderedMarkdown.Of(markdown).Blocks.Select(it => it.Text.Value)));
   static string TextOf(string html) => WithoutWhiteSpace(WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", "")));
   static string WithoutWhiteSpace(string text) => new([..text.Where(it => !char.IsWhiteSpace(it))]);

   string Pick(string[] from) => from[_random.Next(from.Length)];

   void AddBlocks(int depth)
   {
      var count = _random.Next(1, 4);
      for(var index = 0; index < count; index++) AddBlock(Pick(Blocks), depth);
   }

   void AddBlock(string name, int depth) => AddInside(name, () =>
   {
      if(name is "ul" or "ol") AddItems(depth);
      else if(name == "table") AddRows();
      else if(name is "p" or "h2" or "pre" || depth > 2 || _random.Next(2) == 0) AddText();
      else AddBlocks(depth + 1);
   });

   void AddItems(int depth)
   {
      var count = _random.Next(1, 4);
      for(var index = 0; index < count; index++)
         AddInside("li", () =>
         {
            AddText();
            if(depth < 3 && _random.Next(3) == 0) AddBlocks(depth + 1);
         });
   }

   void AddRows()
   {
      var count = _random.Next(1, 4);
      for(var index = 0; index < count; index++)
         AddInside("tr", () =>
         {
            AddInside("td", AddText);
            AddInside("td", AddText);
         });
   }

   void AddText()
   {
      var count = _random.Next(1, 5);
      for(var index = 0; index < count; index++)
      {
         if(_random.Next(3) == 0) AddInside(Pick(Inlines), () => _html.Append(WebUtility.HtmlEncode(Pick(Texts))));
         else _html.Append(WebUtility.HtmlEncode(Pick(Texts)));

         _html.Append(_random.Next(4) switch { 0 => " ", 1 => "<br>", _ => "" });
      }
   }

   void AddInside(string tag, Action addContent)
   {
      _html.Append('<').Append(tag).Append('>');
      addContent();
      _html.Append("</").Append(tag.Split(' ')[0]).Append('>');
   }
}
