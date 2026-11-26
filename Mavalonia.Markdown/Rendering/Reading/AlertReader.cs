using Markdig.Extensions.Alerts;
using Mavalonia.Markdown.Rendering.Blocks;
using Mavalonia.Markdown.Rendering.Containers;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Rendering.Reading;

///<summary>Reads the first line of an alert, such as <c>&gt; [!NOTE]</c>. Markdig leaves it out of the blocks inside the alert.</summary>
class AlertReader
{
   const string BeforeTheName = "[!";

   readonly string _markdown;
   readonly AlertBlock _alert;

   internal AlertReader(string markdown, AlertBlock alert)
   {
      _markdown = markdown;
      _alert = alert;
      Container = Enum.TryParse<AlertKind>(alert.Kind.ToString(), ignoreCase: true, out var kind) ? new Alert(kind) : new Quote();
   }

   ///<summary>The alert, or an ordinary quote, when it's an alert of a kind that there isn't.</summary>
   internal Quote Container { get; }

   internal int EndOfTheQuoteMark => Container.EndOfItsPrefix(_markdown, _alert.Span.Start) ?? _alert.Span.Start;

   SourceRange SourceOfTheTitle
   {
      get
      {
         var start = _markdown.IndexOf(BeforeTheName, _alert.Span.Start, StringComparison.Ordinal);
         return new SourceRange(start, _markdown.IndexOf(']', start) + 1);
      }
   }

   ///<param name="containers">Those of the blocks inside the alert.</param>
   internal RenderedBlock ReadTitle(BlockContainers containers) =>
      Container is Alert alert
         ? new AlertTitle(containers, TextOfTheTitle(alert), SourceOfTheTitle, alert)
         : new Paragraph(containers, TextOfLines.Read(_markdown, [SourceOfTheTitle], TextFormatting.None));

   ///<summary>Each char of the title stands for one of the name's. The first stands for what precedes the name too, and the last for what follows it.</summary>
   RenderedText TextOfTheTitle(Alert alert)
   {
      var title = alert.Kind.ToString();
      var source = SourceOfTheTitle;

      return new RenderedText([..title.Select((it, index) => new RenderedChar(it, SourceOfTheChar(index), TextFormatting.None))], [], source);

      SourceRange SourceOfTheChar(int index)
      {
         var start = source.Start + BeforeTheName.Length + index;
         return new SourceRange(index == 0 ? source.Start : start, index == title.Length - 1 ? source.End : start + 1);
      }
   }
}
