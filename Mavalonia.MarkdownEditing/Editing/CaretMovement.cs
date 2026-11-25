using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Editing;

///<summary>A way to move the caret that doesn't depend on how the text is laid out in lines.</summary>
public class CaretMovement
{
   public static readonly CaretMovement ToTheCharacterBefore = new(editor => Before(editor, (text, index) => text.StartOfCharacterBefore(index)));
   public static readonly CaretMovement ToTheCharacterAfter = new(editor => After(editor, (text, index) => text.EndOfCharacterAfter(index)));
   public static readonly CaretMovement ToTheWordBefore = new(editor => Before(editor, (text, index) => text.StartOfWordBefore(index)));
   public static readonly CaretMovement ToTheWordAfter = new(editor => After(editor, (text, index) => text.EndOfWordAfter(index)));
   public static readonly CaretMovement ToTheStart = new(editor => RenderedPosition.AtTheStartOf(editor.Rendered.Blocks[0]));
   public static readonly CaretMovement ToTheEnd = new(editor => RenderedPosition.AtTheEndOf(editor.Rendered.Blocks[^1]));

   readonly Func<RenderedEditor, RenderedPosition> _move;

   CaretMovement(Func<RenderedEditor, RenderedPosition> move) => _move = move;

   internal RenderedPosition From(RenderedEditor editor) => _move(editor);

   static RenderedPosition Before(RenderedEditor editor, Func<Rendering.Texts.RenderedText, int, int> indexBefore)
   {
      var caret = editor.CaretPosition;
      if(!caret.IsAtTheStartOfItsBlock) return caret with { Index = indexBefore(caret.Block.Text, caret.Index) };
      return editor.Rendered.BlockBefore(caret.Block) is {} previous ? RenderedPosition.AtTheEndOf(previous) : caret;
   }

   static RenderedPosition After(RenderedEditor editor, Func<Rendering.Texts.RenderedText, int, int> indexAfter)
   {
      var caret = editor.CaretPosition;
      if(!caret.IsAtTheEndOfItsBlock) return caret with { Index = indexAfter(caret.Block.Text, caret.Index) };
      return editor.Rendered.BlockAfter(caret.Block) is {} next ? RenderedPosition.AtTheStartOf(next) : caret;
   }
}
