using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing;

///<summary>A way to move the caret that doesn't depend on how the text is laid out in lines.</summary>
public class CaretMovement
{
   public static readonly CaretMovement ToTheCharacterBefore = new(editor => Before(editor, (text, index) => text.StartOfCharacterBefore(index)));
   public static readonly CaretMovement ToTheCharacterAfter = new(editor => After(editor, (text, index) => text.EndOfCharacterAfter(index)));
   public static readonly CaretMovement ToTheWordBefore = new(editor => Before(editor, (text, index) => text.StartOfWordBefore(index)));
   public static readonly CaretMovement ToTheWordAfter = new(editor => After(editor, (text, index) => text.EndOfWordAfter(index)));
   public static readonly CaretMovement ToTheBlockBefore = new(editor => editor.Rendered.BlockBefore(editor.CaretPosition.Block) is {} previous ? RenderedPosition.AtTheEndOf(previous) : editor.CaretPosition);
   public static readonly CaretMovement ToTheBlockAfter = new(editor => editor.Rendered.BlockAfter(editor.CaretPosition.Block) is {} next ? RenderedPosition.AtTheEndOf(next) : editor.CaretPosition);
   public static readonly CaretMovement ToTheStart = new(editor => editor.Rendered.Start);
   public static readonly CaretMovement ToTheEnd = new(editor => editor.Rendered.End);

   public static readonly CaretMovement ToTheStartOfTheSelection = new(editor => editor.Selected.Start);
   public static readonly CaretMovement ToTheEndOfTheSelection = new(editor => editor.Selected.End);

   readonly Func<RenderedEditor, RenderedPosition> _move;

   CaretMovement(Func<RenderedEditor, RenderedPosition> move) => _move = move;

   internal RenderedPosition From(RenderedEditor editor) => _move(editor);

   static RenderedPosition Before(RenderedEditor editor, Func<TextBoundaries, int, int> indexBefore)
   {
      var caret = editor.CaretPosition;
      if(!caret.IsAtTheStartOfItsBlock) return caret with { Index = indexBefore(caret.Block.Text.Boundaries, caret.Index) };
      return editor.Rendered.BlockBefore(caret.Block) is {} previous ? RenderedPosition.AtTheEndOf(previous) : caret;
   }

   static RenderedPosition After(RenderedEditor editor, Func<TextBoundaries, int, int> indexAfter)
   {
      var caret = editor.CaretPosition;
      if(!caret.IsAtTheEndOfItsBlock) return caret with { Index = indexAfter(caret.Block.Text.Boundaries, caret.Index) };
      return editor.Rendered.BlockAfter(caret.Block) is {} next ? RenderedPosition.AtTheStartOf(next) : caret;
   }
}
