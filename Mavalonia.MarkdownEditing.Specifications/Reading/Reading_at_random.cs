using Compze.Must;
using Compze.xUnitBDD;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Specifications.Reading;

///<summary>Reads markdown that nobody would write. It is the same each time the specification runs.</summary>
public class Reading_at_random
{
   const int CountOfDocuments = 8000;

   static readonly string[] Pieces = ["a", "b c", " ", "  ", "\n", "\n\n", "\r\n", "\t", "\0", "*", "**", "_", "`", "```", "~~", "[a](b)", "[a]: b", "![a b](c)", "<br>", "<!-- c -->", "<!--", "&amp;", "\\", "|", "|-|", "| a | b |\n|---|---|\n| 1 | 2 |\n", "---", "# ", "- ", "- [ ] ", "1. ", "> ", "> [!NOTE]\n> ", "    ", "\n====\n", "https://x.y", "é", "👩‍👩‍👧"];

   static readonly CaretMovement[] Movements = [CaretMovement.ToTheCharacterBefore, CaretMovement.ToTheCharacterAfter, CaretMovement.ToTheWordBefore, CaretMovement.ToTheWordAfter, CaretMovement.ToTheBlockBefore, CaretMovement.ToTheBlockAfter];

   static string MarkdownOf(Random random) => string.Concat(Enumerable.Range(0, random.Next(1, 14)).Select(_ => Pieces[random.Next(Pieces.Length)]));

   [XF] public void while_it_is_written_and_selected_in_never_fails()
   {
      for(var seed = 0; seed < CountOfDocuments; seed++)
      {
         var random = new Random(seed);
         var markdown = MarkdownOf(random);
         var read = new MarkdownToRead("", MarkdownDialect.ForReading);
         for(var end = 0; end <= markdown.Length; end += random.Next(1, 6))
         {
            read.Show(markdown[..end], MarkdownDialect.ForReading);
            if(random.Next(3) == 0) read.Select(new TextSelection(random.Next(end + 1), random.Next(end + 1)));
            if(random.Next(3) == 0) read.Move(Movements[random.Next(Movements.Length)], extendSelection: true);
            _ = read.SelectedMarkdown + read.SelectedHtml;
         }
      }
   }

   [XF] public void moving_the_caret_a_character_at_a_time_reaches_the_start_from_the_end_and_the_end_from_the_start() =>
      Enumerable.Range(0, CountOfDocuments).Select(it => MarkdownOf(new Random(it))).Where(it => !IsReachedFromEachOther(it)).Must().BeEmpty();

   static bool IsReachedFromEachOther(string markdown)
   {
      var read = new MarkdownToRead(markdown, MarkdownDialect.ForReading);
      read.MoveTo(read.Rendered.End);

      MoveOften(CaretMovement.ToTheCharacterBefore);
      var start = read.CaretPosition;
      MoveOften(CaretMovement.ToTheCharacterAfter);
      return start == read.Rendered.Start && read.CaretPosition == read.Rendered.End;

      void MoveOften(CaretMovement movement)
      {
         for(var time = 0; time <= markdown.Length * 2; time++) read.Move(movement);
      }
   }
}
