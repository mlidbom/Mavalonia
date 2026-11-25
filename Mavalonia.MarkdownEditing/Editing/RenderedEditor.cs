using AvaloniaEdit.Document;
using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Editing.Edits;
using Mavalonia.MarkdownEditing.Editing.Edits.Deletion;
using Mavalonia.MarkdownEditing.Editing.Edits.Nesting;
using Mavalonia.MarkdownEditing.Editing.Edits.NewBlocks;
using Mavalonia.MarkdownEditing.Editing.Edits.Tables;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing;

///<summary>Edits a markdown document as it is rendered: what a person does is said in terms of what they see, and changes the markdown text.</summary>
public sealed class RenderedEditor : SelectableMarkdown, IDisposable
{
   readonly RenderedDocument _document;
   readonly UndoSteps _undoSteps;

   ///<summary>The formatting of the next text typed, when it isn't that of the markup where the caret is.</summary>
   TextFormatting? _formattingToType;

   public RenderedEditor(TextDocument document, TextSelection selection)
   {
      _document = new RenderedDocument(document);
      _undoSteps = new UndoSteps(document);
      Selection = WhereItIsShown(selection);
      _document.ChangedByOthers += FollowTheChange;
   }

   public TextDocument Document => _document.Document;

   public void Dispose() => _document.Dispose();

   public override RenderedMarkdown Rendered => _document.Rendered;

   ///<summary>The formatting that text typed now gets.</summary>
   TextFormatting FormattingToType => _formattingToType ?? FormattingOfTheSelection;

   TextFormatting FormattingOfTheSelection => Selection.IsEmpty ? Caret.Formatting : FirstSelectedChar?.Formatting ?? TextFormatting.None;

   RenderedChar? FirstSelectedChar => Selected.Start is { IsAtTheEndOfItsBlock: false } start ? start.Block.Text.Chars[start.Index] : null;

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

   ///<summary>Goes from a cell of a table to the next. From the last, that is in a row that is added.</summary>
   public void GoToTheNextCell()
   {
      if(Caret.Block is TableCell { Table: var table } cell && cell == table.LastCell) AsOneUndo(() => Make(new AddingARow(Rendered, table).Change()));
      else Move(CaretMovement.ToTheBlockAfter);
   }

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
         AsOneUndo(Selected.IsAllThatIsRendered ? DeleteAll : DeleteTheSelection);
   }

   ///<summary>With nothing left to see, there is nothing to be a heading, or to be in a list.</summary>
   void DeleteAll() => Make(SourceChange.Remove(Rendered, new SourceRange(0, Rendered.Source.Length)));

   void DeleteTheSelection()
   {
      if(Selection.IsEmpty) return;

      var selected = Selected;
      if(selected is { IsAllThatIsRendered: true, IsInOneBlock: false })
      {
         DeleteAll();
         return;
      }

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

   private protected override void WhenASelectionIsMade() => StopTyping();

   void StopTyping()
   {
      _formattingToType = null;
      _undoSteps.EndTyping();
   }

   void FollowTheChange(object? sender, DocumentChangeEventArgs change) => Selection = new TextSelection(change.GetNewOffset(Selection.Anchor), change.GetNewOffset(Selection.Caret));
}
