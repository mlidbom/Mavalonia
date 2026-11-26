using Avalonia.Input;
using Avalonia.Input.Platform;
using Mavalonia.Markdown.Html;

namespace Mavalonia.Markdown.Controls.Clipboards;

///<summary>What there is on the clipboard to paste: text, and rich text when that was copied too.</summary>
class ClipboardContent
{
   readonly CopiedHtml? _html;

   ClipboardContent(string text, CopiedHtml? html)
   {
      Text = text;
      _html = html;
   }

   internal static async Task<ClipboardContent> ReadFrom(IClipboard clipboard)
   {
      using var data = await clipboard.TryGetDataAsync();
      if(data is null) return new ClipboardContent("", null);

      var html = data.Contains(ClipboardFormats.IsMarkdown) ? null : await data.TryGetValueAsync(ClipboardFormats.Html);
      return new ClipboardContent(await data.TryGetTextAsync() ?? "", html is null ? null : CopiedHtml.ParseHtmlFormatOfWindows(html));
   }

   ///<summary>The text alone. Empty when there is none.</summary>
   internal string Text { get; }

   ///<summary>The rich text as markdown, or else the text, which may be markdown already.</summary>
   internal string Markdown => _html is { IsEmpty: false } html && (!html.IsNothingButCode || Text.Length == 0) ? html.Markdown : Text;
}
