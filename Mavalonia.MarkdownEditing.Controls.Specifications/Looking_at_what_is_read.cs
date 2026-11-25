using Avalonia;
using Avalonia.Styling;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

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

   [XFOnUIThread] public void shows_headings_in_the_sizes_that_are_set()
   {
      static RenderedFrame ShowWith(params IReadOnlyList<double> sizes) => new HeadlessWindow(new MarkdownView { Markdown = "# a\n\n## b\n\n### c", HeadingFontSizes = sizes }).RenderFrame();
      ShowWith(30, 20).Fingerprint.Must().NotBe(ShowWith(30, 14).Fingerprint);
   }
}
