using System.Globalization;
using System.Text;

namespace Mavalonia.Markdown.Html;

///<summary>The form in which HTML is on the clipboard of Windows, as <c>HTML Format</c>: UTF-8, where lines that say where the HTML starts and ends precede it.</summary>
public static class HtmlFormatOfWindows
{
   const string StartOfTheDocument = "<html><body><!--StartFragment-->";
   const string EndOfTheDocument = "<!--EndFragment--></body></html>";

   ///<summary>Writes a part of a document in the form.</summary>
   public static byte[] Write(string fragment)
   {
      var startOfHtml = Header(0, 0, 0, 0).Length;
      var startOfFragment = startOfHtml + StartOfTheDocument.Length;
      var endOfFragment = startOfFragment + Encoding.UTF8.GetByteCount(fragment);
      var endOfHtml = endOfFragment + EndOfTheDocument.Length;

      return Encoding.UTF8.GetBytes(Header(startOfHtml, endOfHtml, startOfFragment, endOfFragment) + StartOfTheDocument + fragment + EndOfTheDocument);
   }

   static string Header(int startOfHtml, int endOfHtml, int startOfFragment, int endOfFragment) =>
      string.Create(CultureInfo.InvariantCulture, $"Version:0.9\r\nStartHTML:{startOfHtml:D10}\r\nEndHTML:{endOfHtml:D10}\r\nStartFragment:{startOfFragment:D10}\r\nEndFragment:{endOfFragment:D10}\r\n");

   ///<summary>The HTML document that is on the clipboard, or nothing, when there is none.</summary>
   internal static string Read(byte[] htmlFormat)
   {
      var text = Encoding.UTF8.GetString(htmlFormat).TrimEnd('\0');
      var startOfHtml = text.IndexOf('<', StringComparison.Ordinal);
      return startOfHtml < 0 ? "" : text[startOfHtml..];
   }
}
