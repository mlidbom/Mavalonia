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

   public class Selecting_with_the_caret : Markdown_that_is_read
   {
      static string Select(string markdown, CaretMovement movement, int times)
      {
         var read = Read(markdown);
         for(var time = 0; time < times; time++) read.Move(movement, extendSelection: true);
         return read.SelectedMarkdown;
      }

      [XF] public void a_character_back_over_an_image_selects_all_of_it() => Select("a ![b c](d.png)‸ e", CaretMovement.ToTheCharacterBefore, times: 1).Must().Be("![b c](d.png)");
      [XF] public void a_character_back_goes_on_before_an_image() => Select("a ![b c](d.png)‸ e", CaretMovement.ToTheCharacterBefore, times: 2).Must().Be(" ![b c](d.png)");
      [XF] public void a_word_back_over_an_image_selects_all_of_it() => Select("a ![b c](d.png)‸ e", CaretMovement.ToTheWordBefore, times: 1).Must().Be("![b c](d.png)");
      [XF] public void a_character_back_goes_on_before_a_table_on_the_next_line() => Select("a\n‸b | c\n---|---", CaretMovement.ToTheCharacterBefore, times: 2).Must().Be("a\n");
      [XF] public void a_character_on_over_an_image_selects_all_of_it() => Select("a ‸![b c](d.png) e", CaretMovement.ToTheCharacterAfter, times: 1).Must().Be("![b c](d.png)");
   }

   public class Selecting_a_word : Markdown_that_is_read
   {
      static string SelectTheWordAt(int index, string markdown)
      {
         var read = new MarkdownToRead(markdown, MarkdownDialect.ForReading);
         read.SelectTheWordAt(new RenderedPosition(read.Rendered.Blocks[0], index));
         return read.SelectedMarkdown;
      }

      [XF] public void selects_it_without_the_space_after_it() => SelectTheWordAt(3, "a **two** b").Must().Be("**two**");
      [XF] public void of_the_description_of_an_image_selects_the_image() => SelectTheWordAt(5, "a ![b cd](e.png) f").Must().Be("![b cd](e.png)");
   }

   public class Copying : Markdown_that_is_read
   {
      static string Copied(string markdown) => Read(markdown).SelectedMarkdown;

      [XF] public void nothing_copies_nothing() => Copied("a‸b").Must().Be("");
      [XF] public void formatted_text_copies_its_markup() => Copied("a **b⟦c** [d](e) `f⟧g`").Must().Be("**c** [d](e) `f`");
      [XF] public void a_bare_URL_copies_it_as_it_is_written() => Copied("a ⟦https://example.org⟧ b").Must().Be("https://example.org");
      [XF] public void an_image_copies_its_markup() => Copied("⟦a ![b c](d.png) e⟧").Must().Be("a ![b c](d.png) e");
      [XF] public void a_link_that_is_defined_elsewhere_copies_its_definition() => Copied("a ⟦[b][c]⟧ d\n\n[c]: https://e.f").Must().Be("[b][c]\n\n[c]: https://e.f");
      [XF] public void several_links_that_are_defined_elsewhere_copies_their_definitions() => Copied("⟦[b]\n\n- [c][d] [b]⟧\n\n[b]: https://e.f\n[d]: https://g.h").Must().Be("[b]\n\n- [c][d] [b]\n\n[b]: https://e.f\n[d]: https://g.h");
      [XF] public void a_link_with_its_definition_copies_that_once() => Copied("⟦[b]\n\n[b]: https://e.f\n\nc⟧").Must().Be("[b]\n\n[b]: https://e.f\n\nc");
      [XF] public void tasks_copies_their_boxes() => Copied("⟦- [ ] a\n- [x] b⟧").Must().Be("- [ ] a\n- [x] b");
      [XF] public void an_alert_copies_its_kind() => Copied("⟦> [!NOTE]\n> a⟧").Must().Be("> [!NOTE]\n> a");
      [XF] public void the_title_of_an_alert_alone_copies_its_text() => Copied("> ⟦[!NOTE]⟧\n> a").Must().Be("Note");
      [XF] public void from_the_middle_of_an_alert_copies_no_kind() => Copied("> [!NOTE]\n> a⟦b\n>\n> c⟧").Must().Be("b\n\nc");
      [XF] public void from_the_middle_of_the_title_of_an_alert_copies_its_text() => Copied("> [!NO⟦TE]\n> a⟧").Must().Be("te\na");
      [XF] public void to_the_middle_of_the_title_of_an_alert_copies_its_text() => Copied("⟦a\n\n> [!NO⟧TE]\n> b").Must().Be("a\n\n> No");
      [XF] public void one_task_copies_its_box() => Copied("- ⟦[x] a⟧").Must().Be("- [x] a");
      [XF] public void one_task_of_several_lines_copies_its_box_and_lines() => Copied("> 1. ⟦[ ] a\n>    b⟧").Must().Be("1. [ ] a\n   b");
      [XF] public void a_task_without_its_box_copies_its_text() => Copied("- [x] ⟦a⟧").Must().Be("a");
      [XF] public void up_to_the_start_of_a_block_of_code_copies_nothing_of_it() => Copied("⟦a\n\n```\n⟧b\n```").Must().Be("a\n\n");
      [XF] public void up_to_the_start_of_a_table_copies_nothing_of_it() => Copied("⟦a\n\n| ⟧b | c |\n|---|---|").Must().Be("a\n\n");
      [XF] public void up_to_the_start_of_a_table_on_the_next_line_copies_nothing_of_it() => Copied("> ⟦a\n> | ⟧b | c |\n|---|---|").Must().Be("a\n");
      [XF] public void up_to_the_start_of_a_heading_copies_nothing_of_it() => Copied("⟦a\n\n# ⟧b").Must().Be("a\n\n");
      [XF] public void up_to_the_start_of_a_list_item_copies_nothing_of_it() => Copied("⟦a\n\n- ⟧b").Must().Be("a\n\n");
      [XF] public void from_the_end_of_a_block_of_code_copies_nothing_of_it() => Copied("```\na⟦\n```\n\nb⟧").Must().Be("\n\nb");
      [XF] public void from_the_end_of_a_table_copies_nothing_of_it() => Copied("| a | b⟦ |\n|---|---|\n\nc⟧").Must().Be("\n\nc");
      [XF] public void from_one_cell_to_the_start_of_the_next_copies_the_table() => Copied("| ⟦a | ⟧b |\n|---|---|").Must().Be("| a | b |\n|---|---|");
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
      [XF] public void blocks_of_HTML_copies_them_as_text() => Copied("⟦<div>\n**a**\n</div>\n\nb⟧").Must().Be("<p>&lt;div&gt;<br />\n**a**<br />\n&lt;/div&gt;</p>\n<p>b</p>");
      [XF] public void a_br_tag_copies_a_line_break() => Copied("⟦a<br>b⟧").Must().Be("<p>a<br />b</p>");
      [XF] public void comments_copies_none_of_them() => Copied("⟦a\n\n<!-- b -->\n\nc <!-- d --> e⟧").Must().Be("<p>a</p>\n<p>c  e</p>");
      [XF] public void an_alert_copies_its_kind_as_text() => Copied("⟦> [!WARNING]\n> a⟧").Must().Satisfy(it => it.Contains("<strong>Warning</strong>") && !it.Contains("<svg"));
      [XF] public void an_image_copies_a_link_to_it() => Copied("⟦![b](https://c.d)⟧").Must().Be("<p><a href=\"https://c.d\">b</a></p>");
      [XF] public void an_image_without_a_description_copies_a_link_to_it() => Copied("⟦![](https://c.d)⟧").Must().Be("<p><a href=\"https://c.d\">image</a></p>");
      [XF] public void an_image_in_a_link_copies_the_link() => Copied("⟦[![b](https://c.d)](https://e.f)⟧").Must().Be("<p><a href=\"https://e.f\">b</a></p>");
      [XF] public void nothing_but_code_copies_none() => Copied("```\n⟦a⟧\n```").Must().Be("");
      [XF] public void nothing_copies_none() => Copied("a‸").Must().Be("");
      [XF] public void hundreds_of_opening_brackets_that_none_closes_copies_them() => Copied($"⟦{new string('[', 500)}⟧").Must().Be($"<p>{new string('[', 500)}</p>");
      [XF] public void formats_inside_one_another_in_quotes_as_deep_as_can_be_read_copies_none() => Copied($"⟦{new string('>', 125)}*a **b *c ~~d~~ c* b** a*⟧").Must().Be("");
      [XF] public void quotes_inside_one_another_hundreds_deep_copies_none() => Copied($"⟦{new string('>', 500)}a⟧").Must().Be("");
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
