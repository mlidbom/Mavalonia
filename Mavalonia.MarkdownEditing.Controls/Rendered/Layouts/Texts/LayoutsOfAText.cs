using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts.Texts;

///<summary>The layouts of a text, in lines at the widths that it has been needed at lately.</summary>
///<remarks>
/// Most texts fit on their lines, so that a layout of them is the same at any other width that they fit in too.
/// </remarks>
class LayoutsOfAText : IDisposable
{
   const int MostLayoutsKeptBesidesThoseInUse = 1;

   readonly Func<double, RunsOfABlock> _runsAtWidth;
   readonly bool _dependsOnTheWidth;

   ///<summary>The one needed latest last.</summary>
   readonly List<(double Width, TextLayout Layout)> _layouts = [];

   ///<summary>How many of the last layouts are in use. The same text may be in several places that are of different widths.</summary>
   int _countInUse;

   ///<param name="runsAtWidth">The runs of the text, where it may be so wide.</param>
   ///<param name="dependsOnTheWidth">Whether its runs are others at another width, as an image's are.</param>
   internal LayoutsOfAText(Func<double, RunsOfABlock> runsAtWidth, bool dependsOnTheWidth)
   {
      _runsAtWidth = runsAtWidth;
      _dependsOnTheWidth = dependsOnTheWidth;
   }

   ///<summary>To be done when all texts are to be laid out anew, so that no layout is in use any more.</summary>
   internal void StartAnew() => _countInUse = 0;

   internal TextLayout At(double width)
   {
      var index = _layouts.FindIndex(it => it.Width.Equals(width) || (!_dependsOnTheWidth && it.Layout.WidthIncludingTrailingWhitespace <= Math.Min(width, it.Width) && !IsWrapped(it.Layout)));
      return index < 0 ? Use((width, LayOut(width))) : UseAgain(index);
   }

   TextLayout UseAgain(int index)
   {
      var layout = _layouts[index];
      if(index >= _layouts.Count - _countInUse) return layout.Layout;

      _layouts.RemoveAt(index);
      return Use(layout);
   }

   TextLayout Use((double Width, TextLayout Layout) layout)
   {
      _layouts.Add(layout);
      _countInUse++;
      while(_layouts.Count > _countInUse + MostLayoutsKeptBesidesThoseInUse) ForgetTheOldest();
      return layout.Layout;
   }

   TextLayout LayOut(double width)
   {
      var runs = _runsAtWidth(width);
      var paragraph = new GenericTextParagraphProperties(FlowDirection.LeftToRight, TextAlignment.Left, firstLineInParagraph: true, alwaysCollapsible: false, runs.PropertiesOfTheParagraph, TextWrapping.Wrap, lineHeight: double.NaN, indent: 0, letterSpacing: 0);
      return new TextLayout(runs.Text, paragraph, maxWidth: width);
   }

   ///<summary>Whether a line ends where the text has no line break.</summary>
   static bool IsWrapped(TextLayout layout) => layout.TextLines.SkipLast(1).Any(it => it.NewLineLength == 0);

   void ForgetTheOldest()
   {
      _layouts[0].Layout.Dispose();
      _layouts.RemoveAt(0);
   }

   public void Dispose()
   {
      while(_layouts.Count > 0) ForgetTheOldest();
   }
}
