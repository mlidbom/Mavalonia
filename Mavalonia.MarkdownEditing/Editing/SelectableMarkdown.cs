using Mavalonia.MarkdownEditing.Html;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Editing;

///<summary>What a markdown text is rendered as, with a selection in it that a person moves.</summary>
public abstract class SelectableMarkdown
{
   TextSelection _selection;

   ///<summary>Tells that what is rendered or selected has changed.</summary>
   public event EventHandler? Changed;

   public abstract RenderedMarkdown Rendered { get; }

   public TextSelection Selection
   {
      get => _selection;
      private protected set
      {
         _selection = value;
         Changed?.Invoke(this, EventArgs.Empty);
      }
   }

   private protected Caret Caret => new(Rendered, Selection.Caret);

   ///<summary>The end of the selection that moves.</summary>
   public RenderedPosition CaretPosition => Caret.Position;

   internal RenderedRange Selected => new(Rendered, Rendered.PositionAt(Selection.Start), Rendered.PositionAt(Selection.End));

   ///<summary>The markdown of what is selected.</summary>
   public string SelectedMarkdown => Selection.IsEmpty ? "" : new MarkdownOfASelection(Selected).Markdown;

   ///<summary>What is selected as HTML, for programs that take rich text. Empty when it's nothing but code, which is text.</summary>
   public string SelectedHtml => Selection.IsEmpty || Selected.IsCode ? "" : HtmlOfMarkdown.Of(SelectedMarkdown, Rendered.Dialect);

   ///<summary>Puts the caret at <paramref name="position"/>. With <paramref name="extendSelection"/>, what is selected then reaches to there.</summary>
   public void MoveTo(RenderedPosition position, bool extendSelection = false)
   {
      WhenASelectionIsMade();
      Selection = extendSelection ? Selection with { Caret = position.Offset } : TextSelection.CaretAt(position.Offset);
   }

   ///<summary>Selects what is between two offsets in the markdown.</summary>
   public void Select(TextSelection selection)
   {
      WhenASelectionIsMade();
      Selection = WhereItIsShown(selection);
   }

   ///<summary>The selection in terms of what is shown. In the source, it may start or end where there is nothing to see, such as inside an entity.</summary>
   private protected TextSelection WhereItIsShown(TextSelection selection) => new(Rendered.OffsetWhereTextCanBeTyped(selection.Anchor), Rendered.OffsetWhereTextCanBeTyped(selection.Caret));

   public void Move(CaretMovement movement, bool extendSelection = false) => MoveTo(movement.From(this), extendSelection);

   public void SelectAll()
   {
      WhenASelectionIsMade();
      Selection = new TextSelection(Rendered.Start.Offset, Rendered.End.Offset);
   }

   private protected virtual void WhenASelectionIsMade() {}
}
