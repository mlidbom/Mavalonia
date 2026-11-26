using AngleSharp.Html.Parser;

namespace Mavalonia.Markdown.Html;

///<summary>Rich text that was copied as HTML, such as from a web page or a note, to be pasted as markdown.</summary>
///<remarks>
/// All of its text is kept. Of what it looks like, what we render is: the rest has no markdown that is shown as anything but source.
/// </remarks>
public class CopiedHtml
{
   readonly IReadOnlyList<MarkdownBlock> _blocks;

   CopiedHtml(IReadOnlyList<MarkdownBlock> blocks) => _blocks = blocks;

   public static CopiedHtml Parse(string html) => new(HtmlReader.ReadBlocksIn(new HtmlParser().ParseDocument(html).Body!));

   ///<summary>Parses HTML in the form of <see cref="HtmlFormatOfWindows"/>.</summary>
   public static CopiedHtml ParseHtmlFormatOfWindows(byte[] htmlFormat) => Parse(HtmlFormatOfWindows.Read(htmlFormat));

   ///<summary>Whether there is no text in it, such as when it is an image.</summary>
   public bool IsEmpty => _blocks.Count == 0;

   ///<summary>Whether it is code and nothing else, as what is copied from an editor of code is.<br/>
   /// Such an editor copies its text in colors, whatever the text is: it may as well be markdown.</summary>
   public bool IsNothingButCode => _blocks is [{ IsCode: true }];

   public string Markdown => string.Join('\n', MarkdownBlock.LinesOf(_blocks));
}
