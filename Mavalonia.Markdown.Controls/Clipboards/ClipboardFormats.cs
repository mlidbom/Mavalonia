using Avalonia.Input;

namespace Mavalonia.Markdown.Controls.Clipboards;

///<summary>The formats that we have on the clipboard besides text.</summary>
static class ClipboardFormats
{
   ///<summary>Rich text, as on the clipboard of Windows.</summary>
   internal static readonly DataFormat<byte[]> Html = DataFormat.CreateBytesPlatformFormat("HTML Format");

   ///<summary>Says that the text is markdown, which what is copied as rich text besides was made from.</summary>
   internal static readonly DataFormat<string> IsMarkdown = DataFormat.CreateStringApplicationFormat("Mavalonia.Markdown");
}
