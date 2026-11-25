using Avalonia.Input;
using Avalonia.Input.Platform;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Html;

namespace Mavalonia.MarkdownEditing.Controls.Clipboards;

///<summary>What is selected, to be put on the clipboard: its markdown as text, and rich text for programs that take that.</summary>
class CopiedSelection
{
   readonly string _markdown;
   readonly string _html;

   internal CopiedSelection(SelectableMarkdown selectable)
   {
      _markdown = selectable.SelectedMarkdown;
      _html = selectable.SelectedHtml;
   }

   internal async Task PutOn(IClipboard clipboard)
   {
      var item = new DataTransferItem();
      item.SetText(_markdown);
      item.Set(ClipboardFormats.IsMarkdown, "");
      if(_html.Length > 0) item.Set(ClipboardFormats.Html, HtmlFormatOfWindows.Write(_html));

      var content = new DataTransfer();
      content.Add(item);
      await clipboard.SetDataAsync(content);
   }
}
