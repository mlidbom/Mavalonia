using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace Mavalonia.Markdown.Controls.Rendered.Layouts.Texts;

///<summary>The lines that a text is laid out in, as they are drawn in its <see cref="ColoursOfAText"/>. Each is made when it's first drawn.</summary>
class LinesInColours : IDisposable
{
   readonly TextOfABlock _text;
   readonly TextParagraphProperties _paragraph;
   readonly IReadOnlyList<TextLine> _linesWithoutColours;
   readonly TextLine?[] _lines;

   internal LinesInColours(RunsOfABlock runs, ColoursOfAText colours, IReadOnlyList<TextLine> linesWithoutColours)
   {
      Colours = colours;
      _text = runs.TextIn(colours);
      _paragraph = runs.PropertiesOfTheParagraph(TextWrapping.NoWrap);
      _linesWithoutColours = linesWithoutColours;
      _lines = new TextLine?[linesWithoutColours.Count];
   }

   internal ColoursOfAText Colours { get; }

   ///<summary>Null when there is no text on the line.</summary>
   internal TextLine? this[int number] => _lines[number] ??= InColours(_linesWithoutColours[number]);

   TextLine? InColours(TextLine line) => TextFormatter.Current.FormatLine(_text.UpTo(line.FirstTextSourceIndex + line.Length), line.FirstTextSourceIndex, double.PositiveInfinity, _paragraph);

   public void Dispose()
   {
      foreach(var line in _lines) line?.Dispose();
   }
}
