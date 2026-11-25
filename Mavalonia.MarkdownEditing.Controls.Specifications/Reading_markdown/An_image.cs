using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.Reading_markdown;

public class An_image
{
   static async Task<ViewInAWindow> Show(string markdown, double width = 120)
   {
      var view = new ViewInAWindow(markdown) { View = { LoadImage = source => Task.FromResult<IImage?>(source == "missing.png" ? null : PictureThatIs(width)) } };
      await Task.Delay(50);
      view.Window.RenderFrame();
      return view;
   }

   static RenderTargetBitmap PictureThatIs(double width)
   {
      var picture = new RenderTargetBitmap(new PixelSize((int)width, 60));
      using var context = picture.CreateDrawingContext();
      context.FillRectangle(Brushes.Orange, new Rect(picture.Size));
      return picture;
   }

   [XFOnUIThread] public void is_shown_as_its_description_when_none_are_loaded() => new ViewInAWindow("a ![b c](d.png)").BoundsOf("b c").Must().Satisfy(it => it.Height < 30 && it.Width < 60);
   [XFOnUIThread] public async Task that_is_loaded_is_shown_in_its_size() => (await Show("a ![b c](d.png) e")).BoundsOf("b c").Must().Satisfy(it => Math.Abs(it.Width - 120) < 1);
   [XFOnUIThread] public async Task that_is_loaded_makes_its_line_as_high_as_it_is() => (await Show("a ![b c](d.png) e")).View.DesiredSize.Height.Must().BeGreaterThan(75);
   [XFOnUIThread] public async Task that_is_wider_than_the_view_is_shown_as_wide_as_that() => (await Show("![b](d.png)", width: 5000)).BoundsOf("b").Must().Satisfy(it => Math.Abs(it.Width - 780) < 2);
   [XFOnUIThread] public async Task that_there_is_none_of_is_shown_as_its_description() => (await Show("![b c](missing.png)")).BoundsOf("b c").Height.Must().BeGreaterThan(0);
   [XFOnUIThread] public async Task that_is_loaded_late_is_not_shown_when_images_are_loaded_otherwise_by_then()
   {
      var late = new TaskCompletionSource<IImage?>();
      var view = new ViewInAWindow("![b](c.png)") { View = { LoadImage = _ => late.Task } };
      view.Window.RenderFrame();
      view.View.LoadImage = _ => Task.FromResult<IImage?>(PictureThatIs(50));
      await Task.Delay(50);
      late.SetResult(PictureThatIs(200));
      await Task.Delay(50);
      view.View.FontSize++; // So that it's laid out anew.
      view.Window.RenderFrame();
      view.BoundsOf("b").Width.Must().Be(50.0);
   }

   [XFOnUIThread] public async Task is_copied_as_its_markdown() => (await Show("a ![b c](d.png) e")).Select("a", to: "e").View.SelectedMarkdown.Must().Be("a ![b c](d.png) e");
   [XFOnUIThread] public async Task tells_that_it_is_clicked() => (await Show("![b c](d.png)")).Click("b c").ClickedLinks.Must().SequenceEqual(["d.png"]);
   [XFOnUIThread] public async Task in_a_link_tells_that_the_link_is_clicked() => (await Show("[![b c](d.png)](https://e.f)")).Click("b c").ClickedLinks.Must().SequenceEqual(["https://e.f"]);
   [XFOnUIThread] public async Task is_seen() => (await Show("- a ![b c](d.png) e\n- [x] ticked\n- [ ] not ticked")).Window.RenderFrame().SaveForInspectionAs("Images and boxes");
}
