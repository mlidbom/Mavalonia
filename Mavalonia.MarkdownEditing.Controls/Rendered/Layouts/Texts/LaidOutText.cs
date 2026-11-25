using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Mavalonia.MarkdownEditing.Controls.Rendered.CodeColours;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts.Texts;

///<summary>The text of a block, laid out in lines. Code is drawn in its colours on the lines that it's laid out in without them, so that colours change no sizes.</summary>
///<remarks>
/// Laid out in its colours, code would be wrapped elsewhere: Avalonia looks for where a line may end inside each stretch of one colour alone.
/// </remarks>
class LaidOutText : IDisposable
{
   readonly RunsOfABlock _runs;
   readonly TextLayout _layout;

   ///<summary>Null until the text is drawn in colours.</summary>
   LinesInColours? _linesInColours;

   internal LaidOutText(RunsOfABlock runs, double width)
   {
      _runs = runs;
      _layout = new TextLayout(runs.Text, runs.PropertiesOfTheParagraph(TextWrapping.Wrap), maxWidth: width);
   }

   internal double Height => _layout.Height;
   internal double Baseline => _layout.Baseline;
   internal double WidthIncludingTrailingWhitespace => _layout.WidthIncludingTrailingWhitespace;

   internal IReadOnlyList<TextLine> Lines => _layout.TextLines;

   ///<summary>Whether a line ends where the text has no line break.</summary>
   internal bool IsWrapped => Lines.SkipLast(1).Any(it => it.NewLineLength == 0);

   ///<summary>The number of the line that a caret before the character at <paramref name="index"/> is on.</summary>
   internal int NumberOfTheLineAt(int index) => _layout.GetLineIndexFromCharacterIndex(index, trailingEdge: false);

   ///<summary>Where the characters from <paramref name="start"/> up to <paramref name="end"/> are: a rectangle for each line of them.</summary>
   internal IEnumerable<Rect> BoundsOf(int start, int end) => _layout.HitTestTextRange(start, end - start);

   ///<summary>Where the character at <paramref name="index"/> is.</summary>
   internal Rect BoundsAt(int index) => _layout.HitTestTextPosition(index);

   ///<summary>Draws the lines that are in <paramref name="viewport"/>, in <paramref name="colours"/> when there are any.</summary>
   internal void Draw(DrawingContext context, Point origin, ColouredCode? colours, Rect viewport)
   {
      var top = origin.Y;
      foreach(var (number, line) in Lines.Index())
      {
         if(top < viewport.Bottom && top + line.Height > viewport.Top) (colours is null ? line : LinesIn(colours)[number])?.Draw(context, new Point(origin.X, top));
         top += line.Height;
      }
   }

   LinesInColours LinesIn(ColouredCode colours)
   {
      if(_linesInColours is not null && _linesInColours.Colours == colours) return _linesInColours;

      _linesInColours?.Dispose();
      return _linesInColours = new LinesInColours(_runs, colours, Lines);
   }

   public void Dispose()
   {
      _linesInColours?.Dispose();
      _layout.Dispose();
   }
}
