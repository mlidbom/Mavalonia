using System.Text;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

public class Using_the_clipboard
{
   const RawInputModifiers Ctrl = RawInputModifiers.Control;
   const RawInputModifiers CtrlShift = RawInputModifiers.Control | RawInputModifiers.Shift;

   static readonly DataFormat<byte[]> HtmlFormatOfWindows = DataFormat.CreateBytesPlatformFormat("HTML Format");

   [XFOnUIThread] public async Task copies_what_is_selected_as_markdown_with_Ctrl_and_C()
   {
      var editor = new EditorInAWindow("a **⟦b⟧** c").Press(PhysicalKey.C, Ctrl);
      (await editor.Window.Clipboard.TryGetTextAsync()).Must().Be("**b**");
   }

   [XFOnUIThread] public async Task cuts_what_is_selected_with_Ctrl_and_X()
   {
      var editor = new EditorInAWindow("a ⟦b⟧c").Press(PhysicalKey.X, Ctrl);
      new { Clipboard = await editor.Window.Clipboard.TryGetTextAsync(), editor.Markdown }.Must().Satisfy(it => it.Clipboard == "b" && it.Markdown == "a ‸c");
   }

   static async Task<string> Press(PhysicalKey key, RawInputModifiers modifiers, string into, string text, string? html = null, bool inTheSource = false)
   {
      var editor = new EditorInAWindow(into) { Editor = { ShowsSource = inTheSource } };
      editor.Editor.FocusTheView();
      await PutOnTheClipboard(editor.Window.Clipboard, text, html);

      editor.Press(key, modifiers);
      await Task.Yield();
      return editor.Markdown;
   }

   static async Task PutOnTheClipboard(IClipboard clipboard, string text, string? html)
   {
      var item = new DataTransferItem();
      item.SetText(text);
      if(html is not null) item.Set(HtmlFormatOfWindows, Encoding.UTF8.GetBytes($"Version:0.9\r\nStartHTML:0000000030\r\n<html><body>{html}</body></html>"));

      var content = new DataTransfer();
      content.Add(item);
      await clipboard.SetDataAsync(content);
   }

   public class Pasting_with_Ctrl_and_V : Using_the_clipboard
   {
      static Task<string> Paste(string into, string text, string? html = null) => Press(PhysicalKey.V, Ctrl, into, text, html);

      [XFOnUIThread] public async Task pastes_text_as_the_markdown_it_is() => (await Paste("> a ‸", "**b**\nc")).Must().Be("> a **b**\n> c‸");
      [XFOnUIThread] public async Task pastes_rich_text_as_markdown() => (await Paste("> a ‸", "b c", "<b>b</b> c")).Must().Be("> a **b** c‸");
      [XFOnUIThread] public async Task pastes_a_table_into_a_quote() => (await Paste("> ‸", "A B", "<table><tr><th>A</th><th>B</th></tr></table>")).Must().Be("> | A | B |\n> |---|---|‸");
      [XFOnUIThread] public async Task pastes_rich_text_that_is_nothing_but_code_as_its_text() => (await Paste("‸", "**b**", "<pre>**b**</pre>")).Must().Be("**b**‸");
      [XFOnUIThread] public async Task pastes_rich_text_that_has_no_text_in_it_as_its_text() => (await Paste("‸", "x", "<img src='y'>")).Must().Be("x‸");
      [XFOnUIThread] public async Task pastes_rich_text_into_code_as_its_text() => (await Paste("```\n‸\n```", "b c", "<b>b</b> c")).Must().Be("```\nb c‸\n```");
      [XFOnUIThread] public async Task changes_nothing_when_there_is_only_an_image() => (await Paste("a‸", "", "<img src='y'>")).Must().Be("a‸");
      [XFOnUIThread] public async Task changes_nothing_when_there_is_nothing_on_the_clipboard() => (await Paste("a‸", "")).Must().Be("a‸");
   }

   public class Pasting_with_Ctrl_and_Shift_and_V : Using_the_clipboard
   {
      [XFOnUIThread] public async Task pastes_the_text_of_rich_text() => (await Press(PhysicalKey.V, CtrlShift, "a ‸", "b c", "<b>b</b> c")).Must().Be("a b c‸");
   }

   public class Pasting_with_Ctrl_and_Shift_and_Q : Using_the_clipboard
   {
      static Task<string> PasteAsQuote(string into, string text, string? html = null) => Press(PhysicalKey.Q, CtrlShift, into, text, html);

      [XFOnUIThread] public async Task pastes_text_as_a_quote() => (await PasteAsQuote("a‸", "b\n\nc")).Must().Be("a\n\n> b\n>\n> c\n\n‸");
      [XFOnUIThread] public async Task pastes_rich_text_as_a_quote() => (await PasteAsQuote("‸", "b c", "<p><b>b</b></p><ul><li>c</li></ul>")).Must().Be("> **b**\n>\n> - c\n\n‸");
      [XFOnUIThread] public async Task changes_nothing_when_there_is_nothing_on_the_clipboard() => (await PasteAsQuote("a‸", "")).Must().Be("a‸");
   }

   public class In_the_source_view : Using_the_clipboard
   {
      static Task<string> Press(PhysicalKey key, RawInputModifiers modifiers, string into, string text, string? html = null) => Press(key, modifiers, into, text, html, inTheSource: true);

      [XFOnUIThread] public async Task pastes_rich_text_as_markdown_with_Ctrl_and_V() => (await Press(PhysicalKey.V, Ctrl, "a ⟦x⟧", "b c", "<b>b</b> c")).Must().Be("a **b** c‸");
      [XFOnUIThread] public async Task pastes_exactly_where_the_caret_is_in_the_markup_with_Ctrl_and_V() => (await Press(PhysicalKey.V, Ctrl, "a *‸*", "b")).Must().Be("a *b‸*");
      [XFOnUIThread] public async Task pastes_text_with_the_line_breaks_of_the_document_with_Ctrl_and_V() => (await Press(PhysicalKey.V, Ctrl, "x\na ‸", "**b**\r\nc")).Must().Be("x\na **b**\nc‸");
      [XFOnUIThread] public async Task pastes_the_text_of_rich_text_with_Ctrl_and_Shift_and_V() => (await Press(PhysicalKey.V, CtrlShift, "a ‸", "b c", "<b>b</b> c")).Must().Be("a b c‸");
      [XFOnUIThread] public async Task pastes_as_a_quote_with_Ctrl_and_Shift_and_Q() => (await Press(PhysicalKey.Q, CtrlShift, "a‸", "b", "<b>b</b>")).Must().Be("a\n\n> **b**\n\n‸");
   }
}
