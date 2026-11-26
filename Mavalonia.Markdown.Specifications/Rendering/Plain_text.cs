using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Specifications.Rendering;

public class Plain_text
{
   readonly PlainText _text = RenderedMarkdown.Of("# one\n\n- **two**\n\n| a | b |\n|---|---|", MarkdownDialect.ForReading).PlainText;

   [XF] public void has_the_text_of_each_block_on_lines_of_its_own() => _text.Value.Must().Be("one\ntwo\na\nb");
   [XF] public void has_the_part_of_a_block_that_a_range_is_in() => PartsOf("wo").Must().SequenceEqual(["two 1..3"]);
   [XF] public void has_the_parts_of_all_the_blocks_that_a_range_is_in() => PartsOf("ne\ntwo\na").Must().SequenceEqual(["one 1..3", "two 0..3", "a 0..1"]);
   [XF] public void has_no_part_for_the_end_of_a_block_that_a_range_ends_with() => PartsOf("e\n").Must().SequenceEqual(["one 2..3"]);

   IReadOnlyList<string> PartsOf(string text) => [.._text.PartsOf(new PlainTextRange(_text.Value.IndexOf(text, StringComparison.Ordinal), text.Length)).Select(it => $"{it.Block.Text.Value} {it.Start}..{it.End}")];
}
