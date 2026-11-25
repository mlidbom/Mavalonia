using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Specifications.Rendering;

public class Markdown_rendered_for_reading
{
   static IReadOnlyList<string> BlocksOf(string markdown) => RenderedMarkdown.Of(markdown, MarkdownDialect.ForReading).Described;
   static string TextOf(string markdown) => RenderedMarkdown.Of(markdown, MarkdownDialect.ForReading).Blocks.Single().Text.DescribedWithFormatting;

   [XF] public void shows_text_that_is_underlined_as_a_heading() => BlocksOf("a\n===\nb\n---\nc").Must().SequenceEqual(["heading 1 \"a\"", "heading 2 \"b\"", "paragraph \"c\""]);
   [XF] public void shows_indented_lines_as_code() => BlocksOf("a\n\n    b\n\n      c\n\nd").Must().SequenceEqual(["paragraph \"a\"", "code \"b\n\n  c\"", "paragraph \"d\""]);
   [XF] public void shows_lines_indented_by_a_tab_as_code() => BlocksOf("\ta\n\t\tb").Must().SequenceEqual(["code \"a\n\tb\""]);
   [XF] public void shows_indented_lines_in_a_list_item_as_code() => BlocksOf("- a\n\n      b\n      c").Must().SequenceEqual(["• > paragraph \"a\"", "• > code \"b\nc\""]);
   [XF] public void shows_indented_lines_in_a_quote_as_code() => BlocksOf(">     a\n>     b").Must().SequenceEqual(["quote > code \"a\nb\""]);
   [XF] public void shows_a_bare_URL_as_a_link() => TextOf("see https://example.org/a_b_c, then").Must().Be("see [link to https://example.org/a_b_c: https://example.org/a_b_c], then");
   [XF] public void shows_no_empty_paragraph_where_there_are_many_blank_lines() => BlocksOf("a\n\n\n\n\nb\n\n\n").Must().SequenceEqual(["paragraph \"a\"", "paragraph \"b\""]);
   [XF] public void shows_no_line_break_at_the_end() => BlocksOf("a\n").Must().SequenceEqual(["paragraph \"a\""]);
   [XF] public void shows_an_empty_paragraph_when_there_is_no_markdown() => BlocksOf("").Must().SequenceEqual(["paragraph \"\""]);
   [XF] public void shows_tasks_as_boxes() => BlocksOf("- [ ] a\n- [x] b").Must().SequenceEqual(["• > paragraph \"☐ a\"", "• > paragraph \"☑ b\""]);
   [XF] public void shows_the_kind_of_an_alert_as_its_title() => BlocksOf("> [!WARNING]\n> a\n>\n> b").Must().SequenceEqual(["Warning > title \"Warning\"", "Warning > paragraph \"a\"", "Warning > paragraph \"b\""]);
   [XF] public void shows_an_alert_without_text_as_its_title() => BlocksOf("> [!NOTE]").Must().SequenceEqual(["Note > title \"Note\""]);
   [XF] public void shows_an_alert_of_an_unknown_kind_as_a_quote() => BlocksOf("> [!SOMETHING]\n> a").Must().SequenceEqual(["quote > paragraph \"[!SOMETHING]\"", "quote > paragraph \"a\""]);
   [XF] public void shows_a_br_tag_as_a_line_break() => TextOf("a<br>b<br />c").Must().Be("a\nb\nc");
   [XF] public void shows_a_br_tag_in_a_cell_as_a_line_break() => BlocksOf("| a<br>b |\n|---|").Must().SequenceEqual(["header 0,0 \"a\nb\""]);
   [XF] public void shows_an_image_as_its_description() => TextOf("a ![b *c*](d.png) e").Must().Be("a [image d.png: b c] e");
   [XF] public void shows_code_entities_links_and_line_breaks_in_the_description_of_an_image_as_text() => TextOf("![a `b` &amp; [c](d)\ne <https://f.g>](h.png)").Must().Be("[image h.png: a b & c e https://f.g]");
   [XF] public void shows_an_image_without_a_description_as_the_word_image() => TextOf("![](d.png)").Must().Be("[image d.png: image]");
   [XF] public void shows_an_image_in_a_link_as_both() => TextOf("[![a](b.png)](c)").Must().Be("[link to c, image b.png: a]");
   [XF] public void hides_comments_and_definitions_of_links() => BlocksOf("[a][b]\n\n<!-- c -->\n\n[b]: d").Must().SequenceEqual(["paragraph \"a\""]);
   [XF] public void hides_comments_inside_text() => TextOf("a <!-- b --> c").Must().Be("a  c");
   [XF] public void hides_comments_one_after_the_other() => BlocksOf("<!-- a --> <!-- b -->\n\nc").Must().SequenceEqual(["paragraph \"c\""]);
   [XF] public void shows_text_on_the_same_line_as_a_comment_with_it_as_source() => BlocksOf("<!-- a --> b").Must().SequenceEqual(["source \"<!-- a --> b\""]);
   [XF] public void shows_other_HTML_as_source() => BlocksOf("<div>\na\n</div>").Must().SequenceEqual(["source \"<div>\na\n</div>\""]);

   public class as_plain_text
   {
      static IReadOnlyList<string> PlainBlocksOf(string text) => RenderedMarkdown.Of(text, MarkdownDialect.PlainText).Described;

      [XF] public void shows_all_as_it_is_written() => PlainBlocksOf("# a\n\n- **b** <c>").Must().SequenceEqual(["paragraph \"# a\n\n- **b** <c>\""]);
      [XF] public void shows_an_empty_paragraph_when_there_is_no_text() => PlainBlocksOf("").Must().SequenceEqual(["paragraph \"\""]);
      [XF] public void shows_text_with_line_breaks_of_Windows() => PlainBlocksOf("a\r\n  b").Must().SequenceEqual(["paragraph \"a\n  b\""]);
   }
}
