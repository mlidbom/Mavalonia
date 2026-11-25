using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.Reading_markdown;

public class Code
{
   const string Markdown = "text\n\n```csharp\nvar one = 1;\n```\n\n```\ntwo\n```";

   readonly ViewInAWindow _view = new(Markdown);

   Button? CopyButton => _view.View.GetVisualDescendants().OfType<Button>().SingleOrDefault(it => it.IsVisible);

   [XFOnUIThread] public void has_no_copy_button_while_the_pointer_is_elsewhere() => (CopyButtonAt("text")?.Content as string).Must().BeNull();
   [XFOnUIThread] public void has_a_copy_button_that_says_its_language_while_the_pointer_is_over_it() => CopyButtonAt("one")!.Content.Must().Be("csharp · Copy");
   [XFOnUIThread] public void without_a_language_has_a_copy_button_while_the_pointer_is_over_it() => CopyButtonAt("two")!.Content.Must().Be("Copy");
   [XFOnUIThread] public void has_the_copy_button_at_its_top_right() => CenterOf(CopyButtonAt("one")!).Must().Satisfy(it => it.X > 700 && Math.Abs(it.Y - _view.CenterOf("one").Y) < 15);

   [XFOnUIThread] public async Task is_copied_as_text_with_the_copy_button()
   {
      _view.Window.Click(CenterOf(CopyButtonAt("one")!));
      new { Text = await _view.Window.Clipboard.TryGetTextAsync(), CopyButton!.Content }.Must().Satisfy(it => it.Text == "var one = 1;" && (string?)it.Content == "Copied");
   }

   [XFOnUIThread] public async Task in_a_language_that_is_known_gets_colours()
   {
      var withoutColours = new ViewInAWindow(Markdown) { View = { ColoursCode = false } }.Window.RenderFrame().Fingerprint;
      for(var time = 0; time < 400 && _view.Window.RenderFrame().Fingerprint == withoutColours; time++) await Task.Delay(10);

      _view.Window.RenderFrame().SaveForInspectionAs("Code with colours");
      _view.Window.RenderFrame().Fingerprint.Must().NotBe(withoutColours);
   }

   [XFOnUIThread] public async Task is_as_high_with_colours_as_without_them()
   {
      var height = _view.View.DesiredSize.Height;
      await Task.Delay(500);
      _view.View.InvalidateMeasure();
      _view.Window.RenderFrame();
      _view.View.DesiredSize.Height.Must().Be(height);
   }

   public class when_the_markdown_changes_under_the_pointer : Code
   {
      public when_the_markdown_changes_under_the_pointer() => _view.Window.MoveThePointerTo(CenterOf(CopyButtonAt("one")!));

      Button? CopyButtonAfterChangingTo(string markdown)
      {
         _view.View.Markdown = markdown;
         _view.Window.RenderFrame();
         return CopyButton;
      }

      [XFOnUIThread] public async Task is_copied_as_it_is_then()
      {
         _view.Window.Click(CenterOf(CopyButtonAfterChangingTo("text\n\n```csharp\nvar one = 1;\nvar more = 2;\n```")!));
         (await _view.Window.Clipboard.TryGetTextAsync()).Must().Be("var one = 1;\nvar more = 2;");
      }

      [XFOnUIThread] public void has_a_copy_button_when_it_comes_there_then()
      {
         var view = new ViewInAWindow("text");
         view.Window.MoveThePointerTo(view.CenterOf("text"));
         view.View.Markdown = "```\ntext\n```";
         view.Window.RenderFrame();
         view.View.GetVisualDescendants().OfType<Button>().Where(it => it is { IsVisible: true, Bounds.Width: > 20 }).Select(it => it.Content).Must().SequenceEqual(["Copy"]);
      }

      [XFOnUIThread] public void has_no_copy_button_when_other_text_is_there_then() => (CopyButtonAfterChangingTo("text\n\nother")?.Content as string).Must().BeNull();
      [XFOnUIThread] public void has_the_copy_button_at_its_top_right_when_it_is_lower_then() => CenterOf(CopyButtonAfterChangingTo("# text\n\n```csharp\nvar one = 1;\n```")!).Y.Must().Satisfy(it => Math.Abs(it - _view.CenterOf("one").Y) < 2);
   }

   Button? CopyButtonAt(string text)
   {
      _view.Window.MoveThePointerTo(_view.CenterOf(text));
      return CopyButton;
   }

   Point CenterOf(Control control) => _view.Window.PointOf(control, new Rect(control.Bounds.Size).Center);
}
