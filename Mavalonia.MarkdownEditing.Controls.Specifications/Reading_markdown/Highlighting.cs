using Avalonia.Media;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.Reading_markdown;

public class Highlighting
{
   readonly ViewInAWindow _view = new("# one two\n\na **two** b\n\n- three");

   PlainTextRange RangeOf(string text) => new(_view.View.PlainText.Value.IndexOf(text, StringComparison.Ordinal), text.Length);

   [XFOnUIThread] public void some_text_shows_it_otherwise()
   {
      var before = _view.Window.RenderFrame().Fingerprint;
      _view.View.Highlights = [new TextHighlight(RangeOf("two"), Brushes.Yellow)];
      _view.Window.RenderFrame().SaveForInspectionAs("A highlight");
      _view.Window.RenderFrame().Fingerprint.Must().NotBe(before);
   }

   [XFOnUIThread] public void nothing_any_more_shows_it_as_before()
   {
      var before = _view.Window.RenderFrame().Fingerprint;
      _view.View.Highlights = [new TextHighlight(RangeOf("two"), Brushes.Yellow)];
      _view.Window.RenderFrame();
      _view.View.Highlights = [];
      _view.Window.RenderFrame().Fingerprint.Must().Be(before);
   }

   [XFOnUIThread] public void null_as_a_binding_may_shows_it_as_before()
   {
      var before = _view.Window.RenderFrame().Fingerprint;
      _view.View.Highlights = null!;
      _view.Window.RenderFrame().Fingerprint.Must().Be(before);
   }

   [XFOnUIThread] public void what_is_beyond_the_text_shows_it_as_before()
   {
      var before = _view.Window.RenderFrame().Fingerprint;
      _view.View.Highlights = [new TextHighlight(new PlainTextRange(1000, 5), Brushes.Yellow), new TextHighlight(new PlainTextRange(-10, 5), Brushes.Yellow)];
      _view.Window.RenderFrame().Fingerprint.Must().Be(before);
   }

   [XFOnUIThread] public void the_same_text_twice_shows_the_last() => FrameWith(Brushes.Yellow, Brushes.Orange).Must().NotBe(FrameWith(Brushes.Orange, Brushes.Yellow));
   [XFOnUIThread] public void text_across_blocks_says_where_all_of_it_is() => _view.View.BoundsOf(RangeOf("two\na two b\nth")).Must().Satisfy(it => it.Count >= 3 && it.Zip(it.Skip(1)).All(pair => pair.First.Top <= pair.Second.Top));

   string FrameWith(params IBrush[] brushes)
   {
      _view.View.Highlights = [..brushes.Select(it => new TextHighlight(RangeOf("two"), it))];
      return _view.Window.RenderFrame().Fingerprint;
   }
}
