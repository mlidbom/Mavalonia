using System.Diagnostics;
using Avalonia;
using Avalonia.Styling;
using AvaloniaEdit.Document;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

public class Looking_at_documents
{
   const string Message = """
                          > ## What changes
                          > Some **bold** and `code`:
                          >
                          > | Project | Responsibility |
                          > | --- | --- |
                          > | `Mavalonia` | All of *it* |
                          > | `Host` | [Trying it](https://example.org) |
                          >
                          > - first
                          >    - nested
                          > - second

                          I **agree**, but:

                          1. Why the ~~first~~ second?
                          2. What about this?
                             ```csharp
                             var x = 1;
                             ```
                          """;

   static RenderedFrame Show(string markdown, ThemeVariant? theme = null, bool showsSource = false) =>
      new HeadlessWindow(new MarkdownEditor { Document = new TextDocument(markdown), Padding = new Thickness(12, 10), ShowsSource = showsSource }, theme).RenderFrame();

   [XFOnUIThread] public void shows_a_message_that_quotes_another() => Show(Message).SaveForInspectionAs("A message that quotes another");
   [XFOnUIThread] public void shows_a_message_that_quotes_another_in_the_dark_theme() => Show(Message, ThemeVariant.Dark).SaveForInspectionAs("A message that quotes another, dark");
   [XFOnUIThread] public void shows_text_differently_in_the_dark_theme() => Show("Some text", ThemeVariant.Dark).Fingerprint.Must().NotBe(Show("Some text").Fingerprint);
   [XFOnUIThread] public void shows_the_source_of_a_message() => Show(Message, showsSource: true).SaveForInspectionAs("The source of a message");

   [XFOnUIThread] public void shows_every_construct() => Show(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "SampleDocuments", "Every construct.md"))).SaveForInspectionAs("Every construct");

   [XFOnUIThread] public void types_in_a_message_of_9_thousand_characters_in_less_than_a_tenth_of_a_second_a_character()
   {
      var markdown = string.Join("\n\n", Enumerable.Range(0, 30).Select(it => Message.Replace("agree", $"agree{it}")));
      var editor = new EditorInAWindow(markdown.Insert(markdown.Length / 2, "‸")).Type("warm up");

      var stopwatch = Stopwatch.StartNew();
      editor.Type("twenty characters ok");
      var perCharacter = stopwatch.Elapsed / 20;

      new { Milliseconds = perCharacter.TotalMilliseconds, markdown.Length }.Must().Satisfy(it => it.Milliseconds < 100);
   }
}
