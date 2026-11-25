using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Specifications.Reading;

public class Markdown_that_is_read
{
   static MarkdownToRead Read(string markdown)
   {
      var parsed = MarkdownWithSelection.Parse(markdown);
      var read = new MarkdownToRead(parsed.Markdown, MarkdownDialect.ForReading);
      read.Select(parsed.Selection);
      return read;
   }

   public class Copying : Markdown_that_is_read
   {
      static string Copied(string markdown) => Read(markdown).SelectedMarkdown;

      [XF] public void nothing_copies_nothing() => Copied("a‸b").Must().Be("");
      [XF] public void formatted_text_copies_its_markup() => Copied("a **b⟦c** [d](e) `f⟧g`").Must().Be("**c** [d](e) `f`");
      [XF] public void a_bare_URL_copies_it_as_it_is_written() => Copied("a ⟦https://example.org⟧ b").Must().Be("https://example.org");
      [XF] public void tasks_copies_their_boxes() => Copied("⟦- [ ] a\n- [x] b⟧").Must().Be("- [ ] a\n- [x] b");
      [XF] public void an_alert_copies_its_kind() => Copied("⟦> [!NOTE]\n> a⟧").Must().Be("> [!NOTE]\n> a");
      [XF] public void the_title_of_an_alert_alone_copies_its_text() => Copied("> ⟦[!NOTE]⟧\n> a").Must().Be("Note");
      [XF] public void from_the_middle_of_an_alert_copies_no_kind() => Copied("> [!NOTE]\n> a⟦b\n>\n> c⟧").Must().Be("b\n\nc");
      [XF] public void indented_code_with_text_copies_its_indentation() => Copied("⟦a\n\n    b\n    c⟧").Must().Be("a\n\n    b\n    c");
      [XF] public void a_part_of_indented_code_copies_it_without_the_indentation() => Copied("    a⟦b\n    c⟧d").Must().Be("b\nc");
      [XF] public void an_underlined_heading_copies_its_line() => Copied("⟦a\n===\nb⟧").Must().Be("a\n===\nb");
      [XF] public void a_table_copies_it() => Copied("⟦| a | b |\n|---|---|\n| 1 | 2⟧ |").Must().Be("| a | b |\n|---|---|\n| 1 | 2 |");
      [XF] public void all_copies_all() => AllCopied("# a\n\n- b\n  - c\n\n```cs\nd\n```").Must().Be("# a\n\n- b\n  - c\n\n```cs\nd\n```");

      static string AllCopied(string markdown)
      {
         var read = Read(markdown + "‸");
         read.SelectAll();
         return read.SelectedMarkdown;
      }
   }

   public class Copying_as_HTML : Markdown_that_is_read
   {
      static string Copied(string markdown) => Read(markdown).SelectedHtml.Trim();

      [XF] public void formatted_text_copies_its_formatting() => Copied("a ⟦**b** [c](https://d.e)⟧").Must().Be("<p><strong>b</strong> <a href=\"https://d.e\">c</a></p>");
      [XF] public void a_line_break_copies_a_line_break() => Copied("⟦a\nb⟧").Must().Be("<p>a<br />\nb</p>");
      [XF] public void HTML_copies_it_as_text() => Copied("⟦a <b>c⟧").Must().Be("<p>a &lt;b&gt;c</p>");
      [XF] public void nothing_but_code_copies_none() => Copied("```\n⟦a⟧\n```").Must().Be("");
      [XF] public void nothing_copies_none() => Copied("a‸").Must().Be("");
   }

   public class Showing_other_markdown : Markdown_that_is_read
   {
      static string Show(string markdown, string insteadOf)
      {
         var read = Read(insteadOf);
         read.Show(markdown, MarkdownDialect.ForReading);
         return new MarkdownWithSelection(read.Rendered.Source, read.Selection).ToString();
      }

      [XF] public void that_goes_on_after_what_was_there_leaves_what_is_selected_selected() => Show("a b c d", insteadOf: "a ⟦b⟧ c").Must().Be("a ⟦b⟧ c d");
      [XF] public void that_is_another_selects_nothing() => Show("x b c", insteadOf: "a ⟦b⟧ c").Must().Be("‸x b c");
      [XF] public void that_is_shorter_selects_nothing() => Show("a", insteadOf: "a ⟦b⟧ c").Must().Be("‸a");
   }
}
