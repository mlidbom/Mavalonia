using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts.Texts;

///<summary>A text, laid out in lines at the widths that it has been needed at lately.</summary>
///<remarks>
/// Most texts fit on their lines, so that a layout of them is the same at any other width that they fit in too.
/// </remarks>
class LaidOutText : IDisposable
{
   const int MostLayoutsKept = 2;

   readonly Func<double, RunsOfABlock> _runsAtWidth;
   readonly bool _dependsOnTheWidth;
   readonly List<(double Width, TextLayout Layout)> _layouts = [];

   ///<summary>The glyphs of the text, which are the same at all widths.</summary>
   readonly TextRunCache _glyphs = new();

   ///<param name="runsAtWidth">The runs of the text, where it may be so wide.</param>
   ///<param name="dependsOnTheWidth">Whether its runs are others at another width, as an image's are.</param>
   internal LaidOutText(Func<double, RunsOfABlock> runsAtWidth, bool dependsOnTheWidth)
   {
      _runsAtWidth = runsAtWidth;
      _dependsOnTheWidth = dependsOnTheWidth;
   }

   internal TextLayout At(double width)
   {
      var index = _layouts.FindIndex(it => it.Width.Equals(width) || (!_dependsOnTheWidth && it.Layout.WidthIncludingTrailingWhitespace <= Math.Min(width, it.Width) && !IsWrapped(it.Layout)));
      if(index < 0) return LayOut(width);

      var found = _layouts[index];
      _layouts.RemoveAt(index);
      _layouts.Add(found);
      return found.Layout;
   }

   TextLayout LayOut(double width)
   {
      var runs = _runsAtWidth(width);
      var paragraph = new GenericTextParagraphProperties(FlowDirection.LeftToRight, TextAlignment.Left, firstLineInParagraph: true, alwaysCollapsible: false, runs.PropertiesOfTheParagraph, TextWrapping.Wrap, lineHeight: double.NaN, indent: 0, letterSpacing: 0);
      var layout = new TextLayout(runs.Text, paragraph, maxWidth: width, textRunCache: _dependsOnTheWidth ? null : _glyphs);

      _layouts.Add((width, layout));
      if(_layouts.Count > MostLayoutsKept) ForgetTheOldest();
      return layout;
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
      _glyphs.Dispose();
   }
}
