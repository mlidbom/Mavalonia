using AvaloniaEdit.Document;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Specifications.Editing;

///<summary>What comes of inserting a quote of <see cref="TextOfTheQuote"/> at some offset of some document.</summary>
record InsertionOfAQuote(string Markdown, int Offset, string MarkdownAfterwards, int CountOfQuotes, bool IsTheCaretInTheQuote, string OtherTextBefore, string OtherTextAfterwards)
{
   internal const string TextOfTheQuote = "QUOTED";

   static readonly string[] Documents =
   [
      "",
      "# a\n\nb **c** d",
      "- a\n- b\n  - c\n\n1. d",
      "> a\n>\n> | A | B |\n> |---|---|\n> | 1 | 2 |",
      "```\na\n\nb\n```\n\nc",
      "a\r\n\r\n> b\r\n> c\r\n",
      "| A |  |\n|---|---|\n|  | b |\n\n---\n\n<div>\nx\n</div>",
      "- ```\n  x\n  ```\n- > y",
      "a\n- b\nc\n> d\n***\ne",
      "a **bb *cc* dd** [ee `ff`](gg)\n**hh**", "# aa *bb* ##\n- **cc**\n- # dd"
   ];

   ///<summary>Inserts the quote at each offset of varied documents at which <paramref name="isPossible"/> says that it can be.</summary>
   internal static IEnumerable<InsertionOfAQuote> AtEachOffset(Func<RenderedBlock, bool> isPossible, Action<RenderedEditor> insert) =>
      from document in Documents
      from offset in Enumerable.Range(0, document.Length + 1)
      where isPossible(EditorOf(document, offset).CaretPosition.Block)
      select At(offset, document, insert);

   static RenderedEditor EditorOf(string markdown, int offset) => new(new TextDocument(markdown), TextSelection.CaretAt(offset));

   static InsertionOfAQuote At(int offset, string markdown, Action<RenderedEditor> insert)
   {
      using var editor = EditorOf(markdown, offset);
      var textBefore = LooksOfTheTextOf(editor.Rendered.Blocks);
      insert(editor);

      var quotes = editor.Rendered.Blocks.Where(it => it.Text.Value == TextOfTheQuote && it.Containers.Innermost is Quote).ToList();
      var isTheCaretInside = quotes.Any(it => editor.CaretPosition.Block.Containers.OutermostFirst.Contains(it.Containers.Innermost));
      return new InsertionOfAQuote(markdown, offset, editor.Document.Text, quotes.Count, isTheCaretInside, textBefore, LooksOfTheTextOf(editor.Rendered.Blocks.Except(quotes)));
   }

   ///<summary>Without the white space, some of which goes where a block is split.</summary>
   static string LooksOfTheTextOf(IEnumerable<RenderedBlock> blocks) =>
      string.Concat(blocks.SelectMany(it => it.Text.Chars).Where(it => !char.IsWhiteSpace(it.Value)).Select(it => $"{it.Value}{it.Formatting.Formats}{it.Formatting.Link?.Target}"));
}
