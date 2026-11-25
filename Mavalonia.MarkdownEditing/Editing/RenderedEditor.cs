using AvaloniaEdit.Document;
using Mavalonia.MarkdownEditing.Editing.Edits;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing;

///<summary>Edits a markdown document as it is rendered: what a person does is said in terms of what they see, and changes the markdown text.</summary>
///<remarks>
/// The document is the only state, apart from the selection. Others may change it at any time, such as an editor of its source.
/// </remarks>
public sealed class RenderedEditor : IDisposable
{
   RenderedMarkdown? _rendered;
   TextSelection _selection;
   bool _isMakingAChange;

   ///<summary>The formatting of the next text typed, when it isn't that of the markup where the caret is.</summary>
   TextFormatting? _formattingToType;

   ///<summary>Marks the changes that one undo undoes together, while text is typed without a break.</summary>
   object _typingWithoutABreak = new();

   public RenderedEditor(TextDocument document, TextSelection selection)
   {
      Document = document;
      _selection = WhereItIsShown(selection);
      document.Changed += FollowTheChange;
   }

   public TextDocument Document { get; }

   ///<summary>Tells that what is rendered or selected has changed.</summary>
   public event EventHandler? Changed;

   public void Dispose() => Document.Changed -= FollowTheChange;

   public RenderedMarkdown Rendered => _rendered ??= RenderedMarkdown.Of(Document.Text);

   public TextSelection Selection
   {
      get => _selection;
      private set
      {
         _selection = value;
         Changed?.Invoke(this, EventArgs.Empty);
      }
   }

   public RenderedPosition CaretPosition => Rendered.PositionAt(Selection.Caret);
   RenderedPosition StartOfSelection => Rendered.PositionAt(Selection.Start);
   RenderedPosition EndOfSelection => Rendered.PositionAt(Selection.End);

   ///<summary>The formatting that text typed now gets.</summary>
   TextFormatting FormattingToType => _formattingToType ?? FormattingOfTheSelection;

   TextFormatting FormattingOfTheSelection => Selection.IsEmpty ? CaretPosition.Block.Text.FormattingAt(Selection.Caret) : FirstSelectedCharacter?.Formatting ?? TextFormatting.None;

   RenderedCharacter? FirstSelectedCharacter => StartOfSelection is { IsAtTheEndOfItsBlock: false } start ? start.Block.Text.Characters[start.Index] : null;

   ///<summary>The markdown of what is selected.</summary>
   public string SelectedMarkdown => Selection.IsEmpty ? "" : new MarkdownOfASelection(Rendered, StartOfSelection, EndOfSelection).Markdown;

   ///<summary>Puts the caret at <paramref name="position"/>. With <paramref name="extendSelection"/>, what is selected then reaches to there.</summary>
   public void MoveTo(RenderedPosition position, bool extendSelection = false)
   {
      StopTyping();
      Selection = extendSelection ? Selection with { Caret = position.Offset } : TextSelection.CaretAt(position.Offset);
   }

   ///<summary>Selects what is between two offsets in the markdown.</summary>
   public void Select(TextSelection selection)
   {
      StopTyping();
      Selection = WhereItIsShown(selection);
   }

   ///<summary>The selection in terms of what is shown. In the source, it may start or end where there is nothing to see, such as inside an entity.</summary>
   TextSelection WhereItIsShown(TextSelection selection) => new(Rendered.OffsetWhereTextCanBeTyped(selection.Anchor), Rendered.OffsetWhereTextCanBeTyped(selection.Caret));

   public void Move(CaretMovement movement, bool extendSelection = false) => MoveTo(movement.From(this), extendSelection);

   public void SelectAll()
   {
      StopTyping();
      Selection = new TextSelection(Rendered.Start.Offset, Rendered.End.Offset);
   }

   public void Type(string text)
   {
      var formatting = FormattingToType;

      AsPartOfTyping(() =>
      {
         DeleteTheSelection();
         var typed = new Typing(Rendered, Selection.Caret, formatting).Of(text);
         Make(typed);
      });

      _formattingToType = FormattingAroundTheCaretHas(formatting) || !LooksTyped() ? null : formatting;

      bool LooksTyped() => CaretPosition is { Index: > 0 } caret && caret.Index >= text.Length && caret.Block.Text.Value[(caret.Index - text.Length)..caret.Index] == text;
   }

   bool FormattingAroundTheCaretHas(TextFormatting formatting) => CaretPosition.Block.Text.FormattingAt(Selection.Caret) == formatting;

   ///<summary>Starts a new block, such as a paragraph or a list item, where the caret is.</summary>
   public void StartNewBlock() => Edit(() => new StartingANewBlock(Rendered, CaretPosition).Change());

   ///<summary>Breaks the line inside the block.</summary>
   public void BreakLine() => Edit(() => new BreakingTheLine(Rendered, Selection.Caret).Change());

   ///<summary>Whether the caret is in a list item that can be made an item of a list inside the one before it.</summary>
   public bool CanIndent => new IndentingAListItem(Rendered, CaretPosition).Change() is not null;

   ///<summary>Makes the list item that the caret is in an item of a list inside the one before it.</summary>
   public void Indent() => AsOneUndo(() => Make(new IndentingAListItem(Rendered, CaretPosition).Change()!));

   ///<summary>Whether the caret is in a list item that can be taken out of its list.</summary>
   public bool CanOutdent => CaretPosition.Block is Paragraph or Heading && CaretPosition.Block.Containers.Innermost is ListItem;

   ///<summary>Takes the list item that the caret is in out of its list, into the list that that is in, if any.</summary>
   public void Outdent() => AsOneUndo(() => Make(new LeavingAContainer(Rendered, CaretPosition).Change()));

   public void DeleteCharacterBefore() => DeleteTheSelectionOr(it => it.TheCharacterBefore());
   public void DeleteCharacterAfter() => DeleteTheSelectionOr(it => it.TheCharacterAfter());
   public void DeleteWordBefore() => DeleteTheSelectionOr(it => it.TheWordBefore());
   public void DeleteWordAfter() => DeleteTheSelectionOr(it => it.TheWordAfter());

   ///<summary>Puts markdown, which may be of several blocks, in place of the selection.</summary>
   public void Paste(string markdown) => Edit(() => new Pasting(Rendered, Selection.Caret).Of(markdown));

   ///<summary>Formats the selected text, or takes the format away when all of it has it. With nothing selected, it's the text typed next that gets or loses the format.</summary>
   public void Toggle(TextFormats format)
   {
      if(Selection.IsEmpty)
         _formattingToType = FormattingToType.Has(format) ? FormattingToType.Without(format) : FormattingToType.With(format);
      else
         ToggleOfTheSelection(format);
   }

   void ToggleOfTheSelection(TextFormats format)
   {
      var (start, end) = (Place.Of(StartOfSelection, Rendered), Place.Of(EndOfSelection, Rendered));

      AsOneUndo(() =>
      {
         foreach(var change in new TogglingAFormat(Rendered, StartOfSelection, EndOfSelection, format).Changes())
            if(change(Rendered) is {} toggling) Make(toggling);
      });

      Selection = new TextSelection(start.In(Rendered).Offset, end.In(Rendered).Offset);
   }

   public void Undo() => Revert(Document.UndoStack.Undo);
   public void Redo() => Revert(Document.UndoStack.Redo);

   void Revert(Action revert)
   {
      StopTyping();
      Document.Changed += PutTheCaretAfterTheChange;
      revert();
      Document.Changed -= PutTheCaretAfterTheChange;

      void PutTheCaretAfterTheChange(object? sender, DocumentChangeEventArgs change) => Selection = TextSelection.CaretAt(change.Offset + change.InsertionLength);
   }

   void DeleteTheSelectionOr(Func<Deleting, SourceChange> delete)
   {
      if(Selection.IsEmpty)
         Edit(() => delete(new Deleting(Rendered, CaretPosition)));
      else
         AsOneUndo(DeleteTheSelection);
   }

   void DeleteTheSelection()
   {
      if(Selection.IsEmpty) return;

      var start = Place.Of(StartOfSelection, Rendered);
      foreach(var change in new DeletingASelection(Rendered, StartOfSelection, EndOfSelection).Changes().ToList()) Make(change(Rendered));
      Selection = TextSelection.CaretAt(start.In(Rendered).Offset);
   }

   void Edit(Func<SourceChange> change) => AsOneUndo(() =>
   {
      DeleteTheSelection();
      Make(change());
   });

   void Make(SourceChange change)
   {
      _isMakingAChange = true;
      if(!change.ChangesNothing) Document.Replace(change.Replaced.Start, change.Replaced.Length, change.Replacement);
      _isMakingAChange = false;

      _rendered = change.After;
      Selection = TextSelection.CaretAt(change.Caret);
   }

   void AsOneUndo(Action action)
   {
      StopTyping();
      using(Document.RunUpdate()) action();
   }

   void AsPartOfTyping(Action action)
   {
      if(Document.UndoStack.LastGroupDescriptor == _typingWithoutABreak)
         Document.UndoStack.StartContinuedUndoGroup(_typingWithoutABreak);
      else
         Document.UndoStack.StartUndoGroup(_typingWithoutABreak);

      action();
      Document.UndoStack.EndUndoGroup();
   }

   void StopTyping()
   {
      _formattingToType = null;
      _typingWithoutABreak = new object();
   }

   void FollowTheChange(object? sender, DocumentChangeEventArgs change)
   {
      if(_isMakingAChange) return;

      _rendered = null;
      Selection = new TextSelection(change.GetNewOffset(_selection.Anchor), change.GetNewOffset(_selection.Caret));
   }

   ///<summary>A position that stays the same when the text of the block is formatted in other ways.</summary>
   readonly record struct Place(int BlockNumber, int Index)
   {
      internal static Place Of(RenderedPosition position, RenderedMarkdown rendered) => new(rendered.NumberOf(position.Block), position.Index);

      internal RenderedPosition In(RenderedMarkdown rendered) => rendered.PositionNearestTo(BlockNumber, Index);
   }
}
