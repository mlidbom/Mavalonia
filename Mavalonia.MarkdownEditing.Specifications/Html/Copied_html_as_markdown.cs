using System.Text;
using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.MarkdownEditing.Html;

namespace Mavalonia.MarkdownEditing.Specifications.Html;

public static class Copied_html_as_markdown
{
   static string MarkdownOf(string html) => CopiedHtml.Parse(html).Markdown;

   public class Text
   {
      [XF] public void in_a_paragraph_is_a_paragraph() => MarkdownOf("<p>a</p>").Must().Be("a");
      [XF] public void in_two_paragraphs_has_a_blank_line_between_them() => MarkdownOf("<p>a</p><p>b</p>").Must().Be("a\n\nb");
      [XF] public void in_two_divs_is_two_lines_of_a_paragraph() => MarkdownOf("<div>a</div><div>b</div>").Must().Be("a\nb");
      [XF] public void in_two_divs_with_an_empty_line_between_them_is_two_paragraphs() => MarkdownOf("<div>a</div><div><br></div><div>b</div>").Must().Be("a\n\nb");
      [XF] public void with_a_line_break_is_two_lines_of_a_paragraph() => MarkdownOf("<p>a<br>b</p>").Must().Be("a\nb");
      [XF] public void with_two_line_breaks_is_two_paragraphs() => MarkdownOf("<p>a<br><br>b</p>").Must().Be("a\n\nb");
      [XF] public void has_one_space_where_the_html_has_any_amount_of_white_space() => MarkdownOf("<p> a\n   b  <b> c </b>d </p>").Must().Be("a b **c** d");
      [XF] public void has_a_space_for_each_no_break_space() => MarkdownOf("<p>a&nbsp;&nbsp;b</p>").Must().Be("a  b");
      [XF] public void leaves_out_scripts_styles_buttons_and_what_is_hidden() => MarkdownOf("<style>p{}</style><p>a<script>x()</script><button>Copy</button><span style='display: none'>b</span></p>").Must().Be("a");
      [XF] public void has_an_emoji_that_is_an_image_as_the_emoji() => MarkdownOf("<p>a <img alt='😀' src='x'></p>").Must().Be("a 😀");
      [XF] public void leaves_out_other_images() => MarkdownOf("<p>a <img alt='Logo' src='x'> b</p>").Must().Be("a b");
   }

   public class Formatting
   {
      [XF] public void is_kept_when_tags_say_it() => MarkdownOf("<p><strong>a</strong> <em>b</em> <del>c</del> <code>d*e</code></p>").Must().Be("**a** *b* ~~c~~ `d*e`");
      [XF] public void is_kept_when_styles_say_it() => MarkdownOf("<span style='font-weight: 700'>a</span> <span style='font-style:italic'>b</span> <span style='text-decoration: line-through'>c</span>").Must().Be("**a** *b* ~~c~~");
      [XF] public void is_kept_for_all_the_blocks_inside_it() => MarkdownOf("<b><p>a</p><p>b</p></b>").Must().Be("**a**\n\n**b**");
      [XF] public void is_none_inside_a_bold_tag_whose_style_says_that_it_is_not_bold_as_Google_Docs_has() => MarkdownOf("<b style='font-weight:normal'><p><span style='font-weight:700'>a</span><span style='font-weight:400'> b</span></p></b>").Must().Be("**a** b");
      [XF] public void that_no_markdown_shows_is_left_out() => MarkdownOf("<p>a<b>\"b\"</b>c</p>").Must().Be("a\"b\"c");
      [XF] public void of_a_link_is_kept() => MarkdownOf("<p><a href='https://x.y/z?a=1&amp;b=(2)'>a <b>b</b></a></p>").Must().Be("[a **b**](https://x.y/z?a=1&b=%282%29)");
      [XF] public void of_a_link_that_leads_nowhere_outside_its_page_is_left_out() => MarkdownOf("<p><a href='#top'>a</a> <a href='javascript:x()'>b</a></p>").Must().Be("a b");
   }

   public class Text_that_looks_like_markup
   {
      [XF] public void is_escaped_where_it_would_become_formatting() => MarkdownOf("<p>*a* _b_ `c` [d](e)</p>").Must().Be(@"\*a\* \_b\_ \`c\` \[d\](e)");
      [XF] public void is_not_escaped_where_it_would_not() => MarkdownOf("<p>2 * 3 * 4, snake_case, # and 1. and a | b</p>").Must().Be("2 * 3 * 4, snake_case, # and 1. and a | b");
      [XF] public void is_escaped_where_it_would_become_a_heading() => MarkdownOf("<p># a</p>").Must().Be(@"\# a");
      [XF] public void is_escaped_where_it_would_become_a_list() => MarkdownOf("<div>- a</div><div>1. b</div>").Must().Be(@"\- a" + "\n" + @"1\. b");
      [XF] public void is_escaped_where_it_would_become_html() => MarkdownOf("<p>&lt;div&gt; &amp;amp;</p>").Must().Be(@"\<div\> \&amp;");
      [XF] public void is_escaped_where_it_would_become_a_table() => MarkdownOf("<div>a | b</div><div>---|---</div>").Must().Satisfy(it => it.Contains(@"\|"));
      [XF] public void is_escaped_where_it_would_end_a_heading() => MarkdownOf("<h2>C #</h2>").Must().Be(@"## C \#");
   }

   public class A_heading
   {
      [XF] public void is_a_heading_of_the_same_level() => MarkdownOf("<h3>a</h3><p>b</p>").Must().Be("### a\n\nb");
      [XF] public void is_not_bold_besides() => MarkdownOf("<h2 style='font-weight:600'><b>a</b> <i>b</i></h2>").Must().Be("## a *b*");
      [XF] public void of_several_lines_is_on_one() => MarkdownOf("<h2>a<br>b</h2>").Must().Be("## a b");
   }

   public class A_list
   {
      [XF] public void is_a_list() => MarkdownOf("<ul><li>a</li><li><p>b</p></li></ul>").Must().Be("- a\n- b");
      [XF] public void with_numbers_keeps_its_first_number() => MarkdownOf("<ol start='9'><li>a</li><li>b</li></ol>").Must().Be("9. a\n10. b");
      [XF] public void inside_an_item_is_inside_the_item() => MarkdownOf("<ul><li>a<ol><li>b</li></ol></li><li>c</li></ul>").Must().Be("- a\n  1. b\n- c");
      [XF] public void right_inside_another_is_inside_the_item_before_it_as_Google_Docs_has() => MarkdownOf("<ul><li>a</li><ul><li>b</li></ul><li>c</li></ul>").Must().Be("- a\n  - b\n- c");
      [XF] public void with_several_blocks_in_an_item_has_them_all_in_the_item() => MarkdownOf("<ol><li><p>a</p><pre>b\n\nc</pre></li><li>d</li></ol>").Must().Be("1. a\n\n   ```\n   b\n\n   c\n   ```\n2. d");
      [XF] public void that_does_not_start_at_1_has_a_blank_line_before_it() => MarkdownOf("<ul><li>a<ol start='2'><li>b</li></ol></li></ul>").Must().Be("- a\n\n  2. b");
      [XF] public void with_checkboxes_has_them_as_text() => MarkdownOf("<ul><li><input type='checkbox' checked>a</li><li><input type='checkbox'>b</li></ul>").Must().Be("- [x] a\n- [ ] b");
      [XF] public void has_no_item_where_an_item_is_empty() => MarkdownOf("<ul><li>a</li><li></li></ul>").Must().Be("- a");
      [XF] public void has_no_item_more_for_a_thematic_break_in_it() => MarkdownOf("<ul><li><hr></li></ul>").Must().Be("- ***");
   }

   public class A_quote
   {
      [XF] public void is_a_quote() => MarkdownOf("<blockquote><p>a</p><p>b</p></blockquote>").Must().Be("> a\n>\n> b");
      [XF] public void inside_another_is_inside_it() => MarkdownOf("<blockquote>a<blockquote>b</blockquote></blockquote>").Must().Be("> a\n>\n> > b");
   }

   public class Code
   {
      [XF] public void keeps_its_white_space_and_its_language() => MarkdownOf("<p>a</p><div><pre><code class='hljs language-csharp'><span>if</span>(x)\n\n   y();\n</code></pre></div>").Must().Be("a\n\n```csharp\nif(x)\n\n   y();\n```");
      [XF] public void with_backticks_in_it_has_a_longer_fence() => MarkdownOf("<p>a</p><pre>```\nb\n```</pre>").Must().Be("a\n\n````\n```\nb\n```\n````");
      [XF] public void with_each_line_in_an_element_of_its_own_has_them_as_lines() => MarkdownOf("<p>a</p><div style='white-space: pre'><div><span>b</span></div><br><div>  c</div></div>").Must().Be("a\n\n```\nb\n\n  c\n```");
      [XF] public void on_several_lines_inside_text_is_on_lines_of_its_own_each_formatted() => MarkdownOf("<p><code>a<br>b</code></p>").Must().Be("`a`\n`b`");
   }

   public class A_table
   {
      [XF] public void is_a_table() => MarkdownOf("<table><thead><tr><th>A</th><th>B | C</th></tr></thead><tbody><tr><td>1</td><td><b>2</b><br>x</td></tr></tbody></table>").Must().Be("| A | B \\| C |\n|---|---|\n| 1 | **2** x |");
      [XF] public void without_a_header_has_its_first_row_as_the_header() => MarkdownOf("<table><tr><td><p>1</p></td><td>2</td></tr><tr><td>3</td><td></td></tr></table>").Must().Be("| 1 | 2 |\n|---|---|\n| 3 |  |");
      [XF] public void with_a_cell_that_spans_columns_has_empty_cells_after_it() => MarkdownOf("<table><tr><td colspan='2'>1</td><td>2</td></tr></table>").Must().Be("| 1 |  | 2 |\n|---|---|---|");
      [XF] public void with_code_that_has_a_vertical_bar_in_a_cell_keeps_it_as_code() => MarkdownOf("<table><tr><td><code>a|b</code></td><td>2</td></tr></table>").Must().Be("| `a|b` | 2 |\n|---|---|");
      [XF] public void with_a_backtick_in_one_cell_and_another_in_the_next_keeps_them_as_text() => MarkdownOf("<table><tr><td>a ` b</td><td>c ` d</td></tr></table>").Must().Be("| a \\` b | c \\` d |\n|---|---|");
      [XF] public void of_one_column_is_paragraphs() => MarkdownOf("<table><tr><td>a</td></tr><tr><td>b</td></tr></table>").Must().Be("a\n\nb");
      [XF] public void with_a_table_in_it_is_taken_for_a_layout() => MarkdownOf("<table><tr><td><table><tr><td>a</td><td>b</td></tr></table></td><td>c</td></tr></table>").Must().Be("| a | b |\n|---|---|\n\nc");
   }

   public class On_the_clipboard_of_Windows
   {
      [XF] public void is_what_follows_the_lines_that_describe_it() =>
         CopiedHtml.ParseHtmlFormatOfWindows(Encoding.UTF8.GetBytes("Version:0.9\r\nStartHTML:00000097\r\nEndHTML:00000200\r\nStartFragment:00000131\r\nEndFragment:00000160\r\nSourceURL:https://x.y/\r\n<html><body>\r\n<!--StartFragment--><p>å <b>b</b></p><!--EndFragment-->\r\n</body></html>\0")).Markdown.Must().Be("å **b**");

      [XF] public void is_empty_when_there_is_no_html() => CopiedHtml.ParseHtmlFormatOfWindows("Version:0.9"u8.ToArray()).IsEmpty.Must().BeTrue();
   }

   public class Is_nothing_but_code
   {
      [XF] public void when_it_is_copied_from_Rider() => CopiedHtml.Parse("<html><head><meta></head><body><pre style=\"font-family:'JetBrains Mono'\"><span># a</span><br>b</pre></body></html>").IsNothingButCode.Must().BeTrue();
      [XF] public void when_it_is_copied_from_VS_Code() => CopiedHtml.Parse("<html><body><!--StartFragment--><div style='font-family: Consolas;white-space: pre;'><div><span>a</span></div><div>b</div></div><!--EndFragment--></body></html>").IsNothingButCode.Must().BeTrue();
      [XF] public void but_not_with_text_around_the_code() => CopiedHtml.Parse("<p>a</p><pre>b</pre>").IsNothingButCode.Must().BeFalse();
   }

   public class Is_empty
   {
      [XF] public void when_it_is_an_image() => CopiedHtml.Parse("<html><body><img alt='Logo' src='https://x.y/z.png'></body></html>").IsEmpty.Must().BeTrue();
      [XF] public void but_not_when_it_has_text() => CopiedHtml.Parse("a").IsEmpty.Must().BeFalse();
   }
}
