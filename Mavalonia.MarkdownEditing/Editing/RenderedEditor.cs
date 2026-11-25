using AvaloniaEdit.Document;
using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Edits;
using Mavalonia.MarkdownEditing.Editing.Edits.Deletion;
using Mavalonia.MarkdownEditing.Editing.Edits.Nesting;
using Mavalonia.MarkdownEditing.Editing.Edits.NewBlocks;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing;

///<summary>Edits a markdown document as it is rendered: what a person does is said in terms of what they see, and changes the markdown text.</summary>
public sealed class RenderedEditor : IDisposable
{
   readonly RenderedDocument _document;
   readonly UndoSteps _undoSteps;
   TextSelection _selection;

   ///<summary>The formatting of the next text typed, when it isn't that of the markup where the caret is.</summary>
   TextFormatting? _formattingToType;

   public RenderedEditor(TextDocument document, TextSelection selection)
   {
      _document = new RenderedDocument(document);
      _undoSteps = new UndoSteps(document);
      _selection = WhereItIsShown(selection);
      _document.ChangedByOthers += FollowTheChange;
   }

   public TextDocument Document => _document.Document;

   ///<summary>Tells that what is rendered or selected has changed.</summary>
   public event EventHandler? Changed;

   public void Dispose() => _document.Dispose();

   public RenderedMarkdown Rendered => _document.Rendered;

   public TextSelection Selection
   {
      get => _selection;
      private set
      {
         _selection = value;
         Changed?.Invoke(this, EventArgs.Empty);
      }
   }

   Caret Caret => new(Rendered, Selection.Caret);
   public RenderedPosition CaretPosition => Caret.Position;
   internal RenderedRange Selected => new(Rendered, Rendered.PositionAt(Selection.Start), Rendered.PositionAt(Selection.End));

   ///<summary>The formatting that text typed now gets.</summary>
   TextFormatting FormattingToType => _formattingToType ?? FormattingOfTheSelection;

   TextFormatting FormattingOfTheSelection => Selection.IsEmpty ? Caret.Formatting : FirstSelectedChar?.Formatting ?? TextFormatting.None;

   RenderedChar? FirstSelectedChar => Selected.Start is { IsAtTheEndOfItsBlock: false } start ? start.Block.Text.Chars[start.Index] : null;

   ///<summary>The markdown of what is selected.</summary>
   public string SelectedMarkdown => Selection.IsEmpty ? "" : new MarkdownOfASelection(Selected).Markdown;

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

      _undoSteps.AsPartOfTyping(() =>
      {
         DeleteTheSelection();
         Make(new Typing(Caret, formatting).Of(text));
      });

      _formattingToType = formatting != Caret.Formatting && IsShownAsTyped() ? formatting : null;

      bool IsShownAsTyped() => Caret is { Index: > 0 } caret && caret.Index >= text.Length && caret.Block.Text.Value[(caret.Index - text.Length)..caret.Index] == text;
   }

   ///<summary>Starts a new block, such as a paragraph or a list item, where the caret is.</summary>
   public void StartNewBlock() => Edit(() => new StartingANewBlock(Caret).Change());

   ///<summary>Breaks the line inside the block.</summary>
   public void BreakLine() => Edit(() => new BreakingTheLine(Caret).Change());

   ///<summary>Whether the caret is in a list item that can be made an item of a list inside the one before it.</summary>
   public bool CanIndent => IndentingAListItem.At(Caret) is not null;

   ///<summary>Makes the list item that the caret is in an item of a list inside the one before it.</summary>
   public void Indent() => AsOneUndo(() => Make(IndentingAListItem.At(Caret)!.Change()));

   ///<summary>Whether the caret is in a list item that can be taken out of its list.</summary>
   public bool CanOutdent => Caret.Block is Paragraph or Heading && Caret.Block.Containers.Innermost is ListItem;

   ///<summary>Takes the list item that the caret is in out of its list, into the list that that is in, if any.</summary>
   public void Outdent() => AsOneUndo(() => Make(new LeavingAContainer(Caret).Change()));

   public void DeleteCharacterBefore() => DeleteTheSelectionOr(it => it.TheCharacterBefore());
   public void DeleteCharacterAfter() => DeleteTheSelectionOr(it => it.TheCharacterAfter());
   public void DeleteWordBefore() => DeleteTheSelectionOr(it => it.TheWordBefore());
   public void DeleteWordAfter() => DeleteTheSelectionOr(it => it.TheWordAfter());

   ///<summary>Puts markdown, which may be of several blocks, in place of the selection.</summary>
   public void Paste(string markdown) => Edit(() => new Pasting(Caret).Of(markdown));

   ///<summary>Puts a quote of markdown in place of the selection, and the caret in a paragraph after it.</summary>
   public void Quote(string markdown) => AsOneUndo(() =>
   {
      DeleteTheSelection();
      if(new Quoting(Caret) is { IsPossible: false } elsewhere) Make(elsewhere.GoToWhereItIsPossible());
      Make(new Quoting(Caret).Of(markdown));
   });

   ///<summary>Formats the selected text, or takes the format away when all of it has it. With nothing selected, it's the text typed next that gets or loses the format.</summary>
   public void Toggle(TextFormats format)
   {
      if(Selection.IsEmpty)
         _formattingToType = FormattingToType.WithToggled(format);
      else
         ToggleForTheSelection(format);
   }

   void ToggleForTheSelection(TextFormats format)
   {
      var selected = Selected;
      AsOneUndo(() => MakeAll(new TogglingAFormat(selected, format).Changes()));

      var selectedAfterwards = selected.ByNumbers.In(Rendered);
      Selection = new TextSelection(selectedAfterwards.Start.Offset, selectedAfterwards.End.Offset);
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
         Edit(() => delete(new Deleting(Caret)));
      else
         AsOneUndo(DeleteTheSelection);
   }

   void DeleteTheSelection()
   {
      if(Selection.IsEmpty) return;

      var selected = Selected;
      MakeAll(new DeletingASelection(selected).Changes());
      Selection = TextSelection.CaretAt(selected.ByNumbers.In(Rendered).Start.Offset);
   }

   void Edit(Func<SourceChange> change) => AsOneUndo(() =>
   {
      DeleteTheSelection();
      Make(change());
   });

   void MakeAll(IEnumerable<Func<RenderedMarkdown, SourceChange?>> changes)
   {
      foreach(var change in changes)
         if(change(Rendered) is {} toMake) Make(toMake);
   }

   void Make(SourceChange change)
   {
      _document.Make(change);
      Selection = TextSelection.CaretAt(change.Caret);
   }

   void AsOneUndo(Action action)
   {
      _formattingToType = null;
      _undoSteps.AsOneStep(action);
   }

   void StopTyping()
   {
      _formattingToType = null;
      _undoSteps.EndTyping();
   }

   void FollowTheChange(object? sender, DocumentChangeEventArgs change) => Selection = new TextSelection(change.GetNewOffset(_selection.Anchor), change.GetNewOffset(_selection.Caret));
}
