using JetBrains.Annotations;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Editing;

///<summary>A way to move the caret that doesn't depend on how the text is laid out in lines.</summary>
public class CaretMovement
{
   public static readonly CaretMovement ToTheCharacterBefore = new(PositionOfTheCharacterBefore, CaretStopBefore);
   public static readonly CaretMovement ToTheCharacterAfter = new(PositionOfTheCharacterAfter, CaretStopAfter);
   public static readonly CaretMovement ToTheWordBefore = new(selectable => Before(selectable, (text, index) => text.StartOfWordBefore(index)));
   public static readonly CaretMovement ToTheWordAfter = new(selectable => After(selectable, (text, index) => text.EndOfWordAfter(index)));
   public static readonly CaretMovement ToTheBlockBefore = new(selectable => selectable.Rendered.BlockBefore(selectable.CaretPosition.Block) is {} previous ? RenderedPosition.AtTheEndOf(previous) : selectable.CaretPosition);
   public static readonly CaretMovement ToTheBlockAfter = new(selectable => selectable.Rendered.BlockAfter(selectable.CaretPosition.Block) is {} next ? RenderedPosition.AtTheEndOf(next) : selectable.CaretPosition);
   public static readonly CaretMovement ToTheStart = new(selectable => selectable.Rendered.Start);
   public static readonly CaretMovement ToTheEnd = new(selectable => selectable.Rendered.End);

   [PublicAPI]
   public static readonly CaretMovement ToTheStartOfTheSelection = new(selectable => selectable.Selected.Start);
   [PublicAPI]
   public static readonly CaretMovement ToTheEndOfTheSelection = new(selectable => selectable.Selected.End);

   readonly Func<SelectableMarkdown, RenderedPosition> _move;
   readonly Func<SelectableMarkdown, int>? _moveToACaretStop;

   CaretMovement(Func<SelectableMarkdown, RenderedPosition> move, Func<SelectableMarkdown, int>? moveToACaretStop = null)
   {
      _move = move;
      _moveToACaretStop = moveToACaretStop;
   }

   internal RenderedPosition From(SelectableMarkdown selectable) => _move(selectable);

   ///<summary>
   /// The offset in the markdown that a caret goes to when it stops on both sides of the backticks where inline code starts or ends, as one does that text is typed at:
   /// see <see cref="RenderedText.CaretStopsAt"/>. That of the position <see cref="From"/> gives, for a movement that passes them.
   ///</summary>
   internal int CaretStopFrom(SelectableMarkdown selectable) => _moveToACaretStop?.Invoke(selectable) ?? From(selectable).Offset;

   static RenderedPosition PositionOfTheCharacterBefore(SelectableMarkdown selectable) => Before(selectable, (text, index) => text.StartOfCharacterBefore(index));
   static RenderedPosition PositionOfTheCharacterAfter(SelectableMarkdown selectable) => After(selectable, (text, index) => text.EndOfCharacterAfter(index));

   ///<summary>The stop before the caret's own, where it is shown: else the last at the character before.</summary>
   static int CaretStopBefore(SelectableMarkdown selectable)
   {
      var (caret, stops) = (selectable.Selection.Caret, selectable.CaretPosition.CaretStops);
      if(stops.ToList().IndexOf(caret) is var stop and > 0) return stops[stop - 1];

      var before = PositionOfTheCharacterBefore(selectable);
      return before == selectable.CaretPosition ? caret : before.CaretStops[^1];
   }

   ///<summary>The stop after the caret's own, where it is shown: else the first at the character after.</summary>
   static int CaretStopAfter(SelectableMarkdown selectable)
   {
      var (caret, stops) = (selectable.Selection.Caret, selectable.CaretPosition.CaretStops);
      if(stops.ToList().IndexOf(caret) is var stop and >= 0 && stop < stops.Count - 1) return stops[stop + 1];

      var after = PositionOfTheCharacterAfter(selectable);
      return after == selectable.CaretPosition ? caret : after.Offset;
   }

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
