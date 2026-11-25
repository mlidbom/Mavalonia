using Avalonia.Input;
using Avalonia.Input.Platform;
using Mavalonia.MarkdownEditing.Html;

namespace Mavalonia.MarkdownEditing.Controls;

///<summary>What there is on the clipboard to paste: text, and rich text when that was copied too.</summary>
class ClipboardContent
{
   static readonly DataFormat<byte[]> HtmlFormatOfWindows = DataFormat.CreateBytesPlatformFormat("HTML Format");

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

      var html = await data.TryGetValueAsync(HtmlFormatOfWindows);
      return new ClipboardContent(await data.TryGetTextAsync() ?? "", html is null ? null : CopiedHtml.ParseHtmlFormatOfWindows(html));
   }

   ///<summary>The text alone. Empty when there is none.</summary>
   internal string Text { get; }

   ///<summary>The rich text as markdown, or else the text, which may be markdown already.</summary>
   internal string Markdown => _html is { IsEmpty: false } html && (!html.IsNothingButCode || Text.Length == 0) ? html.Markdown : Text;
}
