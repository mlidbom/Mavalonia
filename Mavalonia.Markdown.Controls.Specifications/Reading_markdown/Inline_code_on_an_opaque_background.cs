using System.Runtime.CompilerServices;
using Avalonia.Media;
using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.Markdown.Controls.Specifications.Reading_markdown;

///<summary>Code in a line of text, on a <see cref="Rendered.RenderedMarkdownView.CodeBackground"/> that hides what is drawn behind it, as an application may choose.</summary>
public class Inline_code_on_an_opaque_background
{
   readonly ViewInAWindow _view = new("a `code` b") { View = { CodeBackground = Brushes.White } };

   [XFOnUIThread] public void shows_the_selection_over_the_background() =>
      PixelsOfTheCodeInTheColourSetBy(it =>
      {
         _view.View.SelectAll();
         _view.View.SelectionBrush = it;
      }).Must().Satisfy(it => it.OnEachRow.All(row => row > it.WidthOfTheCode / 4));

   [XFOnUIThread] public void shows_a_highlight_over_the_background() =>
      PixelsOfTheCodeInTheColourSetBy(it => _view.View.Highlights = [new TextHighlight(_view.RangeOf("code"), it)])
        .Must().Satisfy(it => it.OnEachRow.All(row => row > it.WidthOfTheCode / 4));

   ///<summary>
   /// How many pixels on each row of where the code is drawn change with the colour that <paramref name="setTheColour"/> sets: those drawn in it.<br/>
   /// The frame in orange is saved for inspection, named after <paramref name="specification"/>.
   ///</summary>
   PixelsInTheColour PixelsOfTheCodeInTheColourSetBy(Action<IBrush> setTheColour, [CallerMemberName] string specification = "")
   {
      setTheColour(Brushes.Orange);
      var orange = _view.Window.RenderFrame();
      orange.SaveForInspectionAs($"Inline code on an opaque background, {specification}");
      setTheColour(Brushes.Blue);
      return new PixelsInTheColour(orange.PixelsThatDifferFrom(_view.Window.RenderFrame(), _view.PixelsOf("code")), _view.PixelsOf("code").Width);
   }

   record PixelsInTheColour(IReadOnlyList<int> OnEachRow, int WidthOfTheCode);
}
