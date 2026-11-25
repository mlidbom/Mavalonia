using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing;

///<summary>A way to move the caret that doesn't depend on how the text is laid out in lines.</summary>
public class CaretMovement
{
   public static readonly CaretMovement ToTheCharacterBefore = new(selectable => Before(selectable, (text, index) => text.StartOfCharacterBefore(index)));
   public static readonly CaretMovement ToTheCharacterAfter = new(selectable => After(selectable, (text, index) => text.EndOfCharacterAfter(index)));
   public static readonly CaretMovement ToTheWordBefore = new(selectable => Before(selectable, (text, index) => text.StartOfWordBefore(index)));
   public static readonly CaretMovement ToTheWordAfter = new(selectable => After(selectable, (text, index) => text.EndOfWordAfter(index)));
   public static readonly CaretMovement ToTheBlockBefore = new(selectable => selectable.Rendered.BlockBefore(selectable.CaretPosition.Block) is {} previous ? RenderedPosition.AtTheEndOf(previous) : selectable.CaretPosition);
   public static readonly CaretMovement ToTheBlockAfter = new(selectable => selectable.Rendered.BlockAfter(selectable.CaretPosition.Block) is {} next ? RenderedPosition.AtTheEndOf(next) : selectable.CaretPosition);
   public static readonly CaretMovement ToTheStart = new(selectable => selectable.Rendered.Start);
   public static readonly CaretMovement ToTheEnd = new(selectable => selectable.Rendered.End);

   public static readonly CaretMovement ToTheStartOfTheSelection = new(selectable => selectable.Selected.Start);
   public static readonly CaretMovement ToTheEndOfTheSelection = new(selectable => selectable.Selected.End);

   readonly Func<SelectableMarkdown, RenderedPosition> _move;

   CaretMovement(Func<SelectableMarkdown, RenderedPosition> move) => _move = move;

   internal RenderedPosition From(SelectableMarkdown selectable) => _move(selectable);

   static RenderedPosition Before(SelectableMarkdown selectable, Func<TextBoundaries, int, int> indexBefore)
   {
      var caret = selectable.CaretPosition;
      var text = caret.Block.Text;
      if(!caret.IsAtTheStartOfItsBlock) return caret with { Index = text.StartOfCharsWithTheSameSourceAs(indexBefore(text.Boundaries, caret.Index)) };
      return selectable.Rendered.BlockBefore(caret.Block) is {} previous ? RenderedPosition.AtTheEndOf(previous) : caret;
   }

   static RenderedPosition After(SelectableMarkdown selectable, Func<TextBoundaries, int, int> indexAfter)
   {
      var caret = selectable.CaretPosition;
      if(!caret.IsAtTheEndOfItsBlock) return caret with { Index = indexAfter(caret.Block.Text.Boundaries, caret.Index) };
      return selectable.Rendered.BlockAfter(caret.Block) is {} next ? RenderedPosition.AtTheStartOf(next) : caret;
   }
}
