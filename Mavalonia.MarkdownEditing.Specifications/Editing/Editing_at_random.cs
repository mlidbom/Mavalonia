using AvaloniaEdit.Document;
using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Specifications.Editing;

///<summary>Edits documents in ways that nobody has thought of. The ways are the same each time the specification runs.</summary>
public class Editing_at_random
{
   static readonly string[] Documents =
   [
      "", "# a\n\nb **c** d", "- a\n- b\n  - c\n\n1. d", "> a\n>\n> | A | B |\n> |---|---|\n> | 1 | 2 |", "```\na\n\nb\n```\n\nc", "a\r\n\r\n> b\r\n> c\r\n", "| A |  |\n|---|---|\n|  | b |\n\n---\n\n<div>\nx\n</div>\n\n[a]: /b", "- ```\n  x\n  ```\n- > y"
   ];

   static readonly string[] Typed = ["a", "| a | b |", "|---|---|", " ", "b", "*", "**", "`", "```", "#", "-", ">", "1.", "|", "~~", "[", "](x)", "<", "&", "\\", "_", "!", "👩‍👩‍👧", "é", "\t"];
   static readonly string[] Pasted = ["x", "x\ny", "- a\n- b", "> q\n> r", "| A |\n|---|\n| 1 |", "```\nc\n```", "# h\n\np", "a\r\nb\r\n", "\n", "  \n\n", "1. a\n   - b"];
   static readonly TextFormats[] Formats = [TextFormats.Bold, TextFormats.Italic, TextFormats.Code, TextFormats.Strikethrough];

   static readonly CaretMovement[] Movements =
   [
      CaretMovement.ToTheCharacterBefore, CaretMovement.ToTheCharacterAfter, CaretMovement.ToTheWordBefore, CaretMovement.ToTheWordAfter, CaretMovement.ToTheBlockBefore, CaretMovement.ToTheBlockAfter, CaretMovement.ToTheStart, CaretMovement.ToTheEnd
   ];

   [XF] public void never_fails_and_leaves_what_is_rendered_in_the_order_of_its_source() => Enumerable.Range(0, 600).Select(DocumentsThatAreNotInOrderAfterEditingAtRandom).SelectMany(it => it).Must().BeEmpty();

   static IReadOnlyList<string> DocumentsThatAreNotInOrderAfterEditingAtRandom(int seed)
   {
      var random = new Random(seed);
      var document = new TextDocument(Documents[random.Next(Documents.Length)]);
      using var editor = new RenderedEditor(document, TextSelection.CaretAt(random.Next(document.TextLength + 1)));

      var notInOrder = new List<string>();
      for(var edit = 0; edit < 40 && document.TextLength < 400; edit++)
      {
         EditAtRandom(editor, random);
         if(!IsInOrder(editor.Rendered) || editor.Rendered.Source != document.Text || editor.Selection.End > document.TextLength) notInOrder.Add(document.Text);
      }

      return notInOrder;
   }

   static bool IsInOrder(RenderedMarkdown rendered) => rendered.Blocks.Zip(rendered.Blocks.Skip(1)).All(it => it.First.Text.Source.End <= it.Second.Text.Source.Start);

   static void EditAtRandom(RenderedEditor editor, Random random)
   {
      switch(random.Next(19))
      {
         case 0 or 1 or 2: editor.Type(AnyOf(Typed)); break;
         case 3 or 4: editor.StartNewBlock(); break;
         case 5: editor.BreakLine(); break;
         case 6 or 7: editor.DeleteCharacterBefore(); break;
         case 8: editor.DeleteCharacterAfter(); break;
         case 9: editor.DeleteWordBefore(); break;
         case 10: editor.DeleteWordAfter(); break;
         case 11: editor.Paste(AnyOf(Pasted)); break;
         case 12: editor.Toggle(AnyOf(Formats)); break;
         case 17: editor.Quote(AnyOf(Pasted)); break;
         case 18: editor.GoToTheNextCell(); break;
         case 13: editor.Select(new TextSelection(AnyOffset(), AnyOffset())); break;
         case 14: editor.Move(AnyOf(Movements), extendSelection: random.Next(2) == 0); break;
         case 15 when editor.CanIndent: editor.Indent(); break;
         case 15 when editor.CanOutdent: editor.Outdent(); break;
         case 16 when random.Next(2) == 0: editor.Undo(); break;
         case 16: _ = editor.SelectedMarkdown; break;
      }

      T AnyOf<T>(T[] all) => all[random.Next(all.Length)];
      int AnyOffset() => random.Next(editor.Document.TextLength + 1);
   }
}
