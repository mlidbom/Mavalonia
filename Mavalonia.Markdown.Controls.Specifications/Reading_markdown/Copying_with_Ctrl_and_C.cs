using Avalonia.Input;
using Avalonia.Input.Platform;
using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.Markdown.Html;

namespace Mavalonia.Markdown.Controls.Specifications.Reading_markdown;

public class Copying_with_Ctrl_and_C
{
   const RawInputModifiers Ctrl = RawInputModifiers.Control;

   static readonly DataFormat<byte[]> HtmlFormat = DataFormat.CreateBytesPlatformFormat("HTML Format");

   [XFOnUIThread] public async Task copies_the_markdown_of_what_is_selected() =>
      (await CopiedFrom(new ViewInAWindow("- a **two** b").Select("a", to: "two")).TryGetTextAsync()).Must().Be("a **two**");

   [XFOnUIThread] public async Task copies_rich_text_too() =>
      CopiedHtml.ParseHtmlFormatOfWindows((await CopiedFrom(new ViewInAWindow("a **two** b").Select("a", to: "two")).TryGetValueAsync(HtmlFormat))!).Markdown.Must().Be("a **two**");

   [XFOnUIThread] public async Task copies_code_as_text_only() =>
      new { Text = await CopiedFrom(SelectedCode()).TryGetTextAsync(), Html = await CopiedFrom(SelectedCode()).TryGetValueAsync(HtmlFormat) }.Must().Satisfy(it => it.Text == "two" && it.Html == null);

   [XFOnUIThread] public async Task and_pasting_in_an_editor_pastes_the_markdown_as_it_is_written()
   {
      var copied = await CopiedFrom(new ViewInAWindow("__a__ *  b\n\n*  c").Select("a", to: "c")).TryGetDataAsync();
      var editor = new EditorInAWindow("‸");
      await editor.Window.Clipboard.SetDataAsync(copied);
      editor.Press(PhysicalKey.V, Ctrl);
      await Task.Yield();
      editor.Markdown.Must().Be("__a__ *  b\n\n*  c‸");
   }

   static ViewInAWindow SelectedCode() => new ViewInAWindow("```\none two\n```").Select("two", to: "two");

   static IClipboard CopiedFrom(ViewInAWindow view) => view.Press(PhysicalKey.C, Ctrl).Window.Clipboard;
}
