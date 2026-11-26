using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.Markdown.Controls.Specifications;

public class Looking_at_what_is_read
{
   const string Reply = """
                        ## Result
                        All **passed**, see https://example.org/run/7 or [the log](https://example.org/log). `dotnet test` says:

                        ```csharp
                        // The answer
                        public static int Answer(string question) => question.Length > 0 ? 42 : throw new ArgumentException("No question");
                        ```

                        - [x] specifications
                        - [ ] documentation
                          1. of the *control*
                          2. of the ~~host~~

                        > [!NOTE]
                        > Useful information.

                        > [!TIP]
                        > Helpful advice.

                        > [!IMPORTANT]
                        > Key information.

                        > [!WARNING]
                        > Urgent information.

                        > [!CAUTION]
                        > Risky.

                        | Project | What<br>for |
                        | --- | --- |
                        | `Mavalonia` | All of *it* |
                        """;

   static RenderedFrame Show(string markdown, ThemeVariant? theme = null) => new HeadlessWindow(new MarkdownView { Markdown = markdown, Padding = new Thickness(12, 10) }, theme).RenderFrame();

   [XFOnUIThread] public void shows_a_reply() => Show(Reply).SaveForInspectionAs("A reply");
   [XFOnUIThread] public void shows_a_reply_in_the_dark_theme() => Show(Reply, ThemeVariant.Dark).SaveForInspectionAs("A reply, dark");
   [XFOnUIThread] public void shows_text_differently_in_the_dark_theme() => Show("Some text", ThemeVariant.Dark).Fingerprint.Must().NotBe(Show("Some text").Fingerprint);

   [XFOnUIThread] public void shows_text_as_it_is_written_where_controls_are_from_right_to_left() =>
      new HeadlessWindow(new MarkdownView { Markdown = "a **b**", FlowDirection = FlowDirection.RightToLeft }).RenderFrame().Fingerprint.Must().Be(new HeadlessWindow(new MarkdownView { Markdown = "a **b**" }).RenderFrame().Fingerprint);

   public class in_a_theme_made_from_the_dark_one
   {
      static string FrameIn(ThemeVariant theme) => new HeadlessWindow(new MarkdownView { Markdown = "> [!NOTE]\n> a" }, theme).RenderFrame().Fingerprint;

      [XFOnUIThread] public void shows_the_colours_of_the_dark_one() => FrameIn(new ThemeVariant("Night", ThemeVariant.Dark)).Must().Be(FrameIn(ThemeVariant.Dark));
   }

   public class when_the_application_changes_the_resources_of_the_theme
   {
      readonly MarkdownView _view = new() { Markdown = "- a <b> [c](d)" };
      readonly HeadlessWindow _window;

      public when_the_application_changes_the_resources_of_the_theme() => _window = new HeadlessWindow(_view);

      string FrameWith(string key, IBrush brush)
      {
         Application.Current!.Resources[key] = brush;
         return _window.RenderFrame().Fingerprint;
      }

      [XFOnUIThread] public void shows_its_bullets_and_source_in_them() => FrameWith("TextControlPlaceholderForeground", Brushes.Red).Must().NotBe(FrameWith("TextControlPlaceholderForeground", Brushes.Green));
      [XFOnUIThread] public void shows_its_links_in_them() => FrameWith("SystemAccentColor", Brushes.Red).Must().NotBe(FrameWith("SystemAccentColor", Brushes.Green));
   }

   [XFOnUIThread] public void shows_headings_in_the_sizes_that_are_set()
   {
      static RenderedFrame ShowWith(params IReadOnlyList<double> sizes) => new HeadlessWindow(new MarkdownView { Markdown = "# a\n\n## b\n\n### c", HeadingFontSizes = sizes }).RenderFrame();
      ShowWith(30, 20).Fingerprint.Must().NotBe(ShowWith(30, 14).Fingerprint);
   }
}
