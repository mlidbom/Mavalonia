using Avalonia.Input;
using Avalonia.Input.Platform;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Html;

namespace Mavalonia.MarkdownEditing.Controls;

///<summary>What there is on the clipboard to paste: text, and rich text when that was copied too.</summary>
class ClipboardContent
{
   static readonly DataFormat<byte[]> Html = DataFormat.CreateBytesPlatformFormat("HTML Format");

   ///<summary>Says that the text is markdown, which what is copied as rich text besides was made from.</summary>
   static readonly DataFormat<string> IsMarkdown = DataFormat.CreateStringApplicationFormat("Mavalonia.Markdown");

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

      var html = data.Contains(IsMarkdown) ? null : await data.TryGetValueAsync(Html);
      return new ClipboardContent(await data.TryGetTextAsync() ?? "", html is null ? null : CopiedHtml.ParseHtmlFormatOfWindows(html));
   }

   ///<summary>Puts what is selected on the clipboard.</summary>
   internal static async Task Write(SelectableMarkdown selectable, IClipboard clipboard)
   {
      var item = new DataTransferItem();
      item.SetText(selectable.SelectedMarkdown);
      item.Set(IsMarkdown, "");
      if(selectable.SelectedHtml is { Length: > 0 } html) item.Set(Html, HtmlFormatOfWindows.Write(html));

      var content = new DataTransfer();
      content.Add(item);
      await clipboard.SetDataAsync(content);
   }

   ///<summary>The text alone. Empty when there is none.</summary>
   internal string Text { get; }

   ///<summary>The rich text as markdown, or else the text, which may be markdown already.</summary>
   internal string Markdown => _html is { IsEmpty: false } html && (!html.IsNothingButCode || Text.Length == 0) ? html.Markdown : Text;
}
