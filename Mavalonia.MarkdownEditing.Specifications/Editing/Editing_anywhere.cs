using AvaloniaEdit.Document;
using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Specifications.Editing;

///<summary>Does the same edit at every position of documents with all the markup that messages have, and requires that what is shown changes as the edit says, and nothing else.</summary>
public class Editing_anywhere
{
   static readonly IReadOnlyList<string> Documents =
   [
      "# Head *one*\n\nSome **bold**, *italic*, ***both***, ~~gone~~ and `code` text, [a *link*](https://example.org) and <https://example.org>.",
      "- one **two**\n- *three*\n  - four `five`\n    second line\n- six\n\n1. seven\n2. eight\n\n   nine",
      "> quote **one** two\n> three\n>\n> - four\n> - *five*\n>\n> > six",
      "Before\n\n| Aa | **Bb** |\n|----|----|\n| `cc` dd | [ee](https://example.org) |\n| ff \\| gg | ~~hh~~ |\n\nAfter",
      "> | Aa | Bb |\n> |---|---|\n> | cc | *dd* |\n>\n> after",
      "one\n\n```cs\nvar x = 1;\n\n  y();\n```\n\n- two\n\n  ```\n  z\n  ```",
      "one **two three** four **five**\n**six**\\\nseven  \neight \nnine",
      "a \\* b &amp; c \\_d\\_ 👩‍👩‍👧 åäö 日本語 **é**",
      "\n\none\n\n\n\n**two**\n\n---\n\n"
   ];

   static IEnumerable<string> DocumentsInEveryForm => Documents.SelectMany(it => new[] { it, it.Replace("\n", "\r\n"), string.Join('\n', it.Split('\n').Select(line => "> " + line)), string.Join('\n', it.Split('\n').Select((line, number) => number == 0 ? "- " + line : line.Length == 0 ? "" : "  " + line)) });

   static IReadOnlyList<string> TextsOf(RenderedMarkdown rendered) => [..rendered.Blocks.Select(it => it.Text.Value)];

   static IEnumerable<(string Document, int Block, int Index)> PositionsIn(string document) =>
      RenderedMarkdown.Of(document).Blocks.Index().SelectMany(block => Enumerable.Range(0, block.Item.Text.Length + 1).Select(index => (document, block.Index, index)));

   ///<summary>Where the edit does something else than what <paramref name="expected"/> says it should.</summary>
   static IReadOnlyList<string> Exceptions(Action<RenderedEditor> edit, Func<RenderedBlock, int, IReadOnlyList<string>?> expected) =>
   [
      ..from position in DocumentsInEveryForm.SelectMany(PositionsIn)
        let before = RenderedMarkdown.Of(position.Document)
        let block = before.Blocks[position.Block]
        let expectedTexts = expected(block, position.Index)
        where expectedTexts is not null
        let actual = After(edit, position.Document, new RenderedPosition(block, position.Index).Offset)
        let allExpected = TextsOf(before).Take(position.Block).Concat(expectedTexts).Concat(TextsOf(before).Skip(position.Block + 1)).ToList()
        where !allExpected.SequenceEqual(actual.Texts)
        select $"{position.Document.Insert(new RenderedPosition(block, position.Index).Offset, "‸")} => {actual.Markdown}"
   ];

   static (string Markdown, IReadOnlyList<string> Texts) After(Action<RenderedEditor> edit, string document, int caret)
   {
      using var editor = new RenderedEditor(new TextDocument(document), TextSelection.CaretAt(caret));
      edit(editor);
      return (editor.Document.Text, TextsOf(editor.Rendered));
   }

   static bool IsTextThatCanBeSplit(RenderedBlock block, int index) => block is Paragraph or Heading && index > 0 && index < block.Text.Length && !block.Text.Characters[index - 1].IsWhiteSpace && !block.Text.Characters[index].IsWhiteSpace && IsBetweenCharacters(block, index);

   static bool IsBetweenCharacters(RenderedBlock block, int index) => index == block.Text.Length || block.Text.Boundaries.StartOfCharacterBefore(index + 1) == index || block.Text.Boundaries.StartOfCharacterBefore(block.Text.Boundaries.EndOfCharacterAfter(index)) == index;

   ///<summary>Markdown shows no space at the start of text.</summary>
   static bool LeavesASpaceAtTheStart(RenderedBlock block, int index) => block is not CodeBlock && block.Text.Boundaries.StartOfCharacterBefore(index) == 0 && index < block.Text.Length && block.Text.Characters[index].IsWhiteSpace;

   [XF] public void by_typing_a_letter_shows_that_letter_there_and_nothing_else_changes() =>
      Exceptions(it => it.Type("x"), (block, index) => block is ThematicBreak || !IsBetweenCharacters(block, index) ? null : [block.Text.Value.Insert(index, "x")]).Must().BeEmpty();

   [XF] public void by_typing_a_space_between_two_letters_shows_that_space_there_and_nothing_else_changes() =>
      Exceptions(it => it.Type(" "), (block, index) => index == 0 || index == block.Text.Length || !char.IsLetter(block.Text.Value[index - 1]) || !char.IsLetter(block.Text.Value[index]) || !IsBetweenCharacters(block, index) ? null : [block.Text.Value.Insert(index, " ")]).Must().BeEmpty();

   [XF] public void by_deleting_the_character_before_the_caret_takes_that_character_away_and_nothing_else_changes() =>
      Exceptions(it => it.DeleteCharacterBefore(), (block, index) => index == 0 || !IsBetweenCharacters(block, index) || LeavesASpaceAtTheStart(block, index) ? null : [block.Text.Value.Remove(block.Text.Boundaries.StartOfCharacterBefore(index), index - block.Text.Boundaries.StartOfCharacterBefore(index))]).Must().BeEmpty();

   [XF] public void by_starting_a_new_block_inside_text_makes_two_blocks_of_it_and_nothing_else_changes() =>
      Exceptions(it => it.StartNewBlock(), (block, index) => IsTextThatCanBeSplit(block, index) ? [block.Text.Value[..index], block.Text.Value[index..]] : null).Must().BeEmpty();

   [XF] public void by_starting_a_new_block_inside_text_outside_lists_and_deleting_backwards_shows_what_was_shown_before() =>
      Exceptions(it =>
      {
         it.StartNewBlock();
         it.DeleteCharacterBefore();
      }, (block, index) => IsTextThatCanBeSplit(block, index) && block.Containers.Innermost is not ListItem ? [block.Text.Value] : null).Must().BeEmpty();

   [XF] public void and_undoing_it_restores_the_markdown_exactly() =>
      DocumentsInEveryForm.SelectMany(PositionsIn).Where(it => After(editor =>
      {
         editor.Type("x");
         editor.StartNewBlock();
         editor.DeleteCharacterBefore();
         while(editor.Document.UndoStack.CanUndo) editor.Undo();
      }, it.Document, new RenderedPosition(RenderedMarkdown.Of(it.Document).Blocks[it.Block], it.Index).Offset).Markdown != it.Document).Must().BeEmpty();
}
