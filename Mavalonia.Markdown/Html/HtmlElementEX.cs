using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Html;

///<summary>What an element of HTML means to us.</summary>
static partial class HtmlElementEX
{
   static readonly HashSet<string> NamesOfHidden = ["script", "style", "head", "title", "template", "noscript", "button", "select", "textarea", "svg", "iframe", "object", "audio", "video"];

   ///<summary>Elements that have space above and below them, so that their text is paragraphs of its own.</summary>
   static readonly HashSet<string> NamesOfParagraphs = ["p", "dl", "figure", "section", "article", "main", "header", "footer", "aside", "nav", "form", "fieldset", "details", "tr"];

   ///<summary>Elements that are on lines of their own, without space above and below them.</summary>
   static readonly HashSet<string> NamesOfLines = ["div", "dt", "dd", "address", "center", "summary", "figcaption", "legend", "hgroup", "body", "caption", "tbody", "thead", "tfoot"];

   static readonly HashSet<string> NamesOfOtherBlocks = ["h1", "h2", "h3", "h4", "h5", "h6", "pre", "blockquote", "ul", "ol", "menu", "li", "table", "td", "th", "hr"];

   extension(IElement @this)
   {
      internal bool IsHidden => NamesOfHidden.Contains(@this.LocalName) || @this.HasAttribute("hidden") || @this.Style("display") == "none";

      internal bool IsAParagraph => NamesOfParagraphs.Contains(@this.LocalName);
      internal bool IsALine => NamesOfLines.Contains(@this.LocalName);
      internal bool IsABlock => @this.IsAParagraph || @this.IsALine || NamesOfOtherBlocks.Contains(@this.LocalName);

      ///<summary>Whether white space in it is shown as it is, as that of code is.</summary>
      internal bool IsCode => @this.LocalName == "pre" || (@this.LocalName == "div" && @this.Style("white-space") == "pre");

      ///<summary>The level of a heading. Null when it isn't one.</summary>
      internal int? HeadingLevel => @this.LocalName is ['h', >= '1' and <= '6' and var level] ? level - '0' : null;

      internal bool IsAList => @this.LocalName is "ul" or "ol" or "menu";

      ///<summary>The text that stands for an element that has none in it: an emoji that is an image, or a checkbox.</summary>
      internal string TextThatStandsForIt => @this switch
      {
         { LocalName: "img" } when @this.GetAttribute("alt") is {} alternative && IsAnEmoji(alternative) => alternative,
         { LocalName: "input" } when @this.GetAttribute("type") == "checkbox" => @this.HasAttribute("checked") ? "[x] " : "[ ] ",
         _ => ""
      };

      ///<summary>How text in the element is formatted, when the text around it is formatted as <paramref name="formatting"/>.</summary>
      internal TextFormatting Format(TextFormatting formatting)
      {
         var byName = @this.LocalName switch
         {
            "b" or "strong" => formatting.With(TextFormats.Bold),
            "i" or "em" => formatting.With(TextFormats.Italic),
            "s" or "strike" or "del" => formatting.With(TextFormats.Strikethrough),
            "code" or "kbd" or "samp" or "tt" => formatting.With(TextFormats.Code),
            "a" when @this.LinkOfIt() is {} link => formatting with { Link = link },
            _ => formatting
         };

         return @this.FormatAsItsStyleSays(byName);
      }

      TextFormatting FormatAsItsStyleSays(TextFormatting formatting)
      {
         if(@this.Style("font-weight") is {} weight) formatting = IsBold(weight) ? formatting.With(TextFormats.Bold) : formatting.Without(TextFormats.Bold);
         if(@this.Style("font-style") is {} style) formatting = style is "italic" or "oblique" ? formatting.With(TextFormats.Italic) : formatting.Without(TextFormats.Italic);
         if((@this.Style("text-decoration") + @this.Style("text-decoration-line")).Contains("line-through", StringComparison.Ordinal)) formatting = formatting.With(TextFormats.Strikethrough);

         return formatting;
      }

      Link? LinkOfIt()
      {
         if(!Uri.TryCreate(@this.GetAttribute("href"), UriKind.Absolute, out var uri) || uri.Scheme is "javascript" or "data" or "about") return null;

         var target = uri.AbsoluteUri.Replace("(", "%28", StringComparison.Ordinal).Replace(")", "%29", StringComparison.Ordinal);
         return new Link(target, "[", $"]({target})");
      }

      ///<summary>What its style attribute says that the property named <paramref name="property"/> is. Null when it doesn't say.</summary>
      string? Style(string property) =>
         @this.GetAttribute("style")?.Split(';')
              .Select(it => it.Split(':', 2))
              .LastOrDefault(it => it.Length == 2 && it[0].Trim().Equals(property, StringComparison.OrdinalIgnoreCase))?[1]
              .Replace("!important", "", StringComparison.OrdinalIgnoreCase).Trim().ToLowerInvariant();

      ///<summary>The language that code is said to be in, as highlighters of code say it. Empty when none is said.</summary>
      internal string LanguageOfCode =>
         new[] { @this, @this.QuerySelector("code"), @this.ParentElement }.Select(it => ClassThatSaysTheLanguage().Match(it?.ClassName ?? "").Groups["language"].Value).FirstOrDefault(it => it.Length > 0) ?? "";
   }

   static bool IsBold(string weight) => weight is "bold" or "bolder" || (int.TryParse(weight, CultureInfo.InvariantCulture, out var number) && number >= 600);

   static bool IsAnEmoji(string text) => new StringInfo(text).LengthInTextElements == 1 && !text.Any(char.IsAscii);

   [GeneratedRegex(@"(^|\s)(language|lang|highlight-source)-(?<language>[\w+#.-]+)")]
   private static partial Regex ClassThatSaysTheLanguage();
}
