using AvaloniaEdit.Document;

namespace Mavalonia.Markdown.Editing;

///<summary>Decides which changes to a document one undo undoes together.</summary>
class UndoSteps
{
   readonly TextDocument _document;

   ///<summary>Marks the changes that are made while text is typed without a break.</summary>
   object _typingWithoutABreak = new();

   internal UndoSteps(TextDocument document) => _document = document;

   ///<summary>Has the changes that <paramref name="makeChanges"/> makes undone together.</summary>
   internal void AsOneStep(Action makeChanges)
   {
      EndTyping();
      using(_document.RunUpdate()) makeChanges();
   }

   ///<summary>Has the changes that <paramref name="makeChanges"/> makes undone together.</summary>
   ///<returns>What <paramref name="makeChanges"/> returns.</returns>
   internal TResult AsOneStep<TResult>(Func<TResult> makeChanges)
   {
      EndTyping();
      using(_document.RunUpdate()) return makeChanges();
   }

   ///<summary>Has the changes that <paramref name="makeChanges"/> makes undone together with those made by typing just before.</summary>
   internal void AsPartOfTyping(Action makeChanges)
   {
      if(_document.UndoStack.LastGroupDescriptor == _typingWithoutABreak)
         _document.UndoStack.StartContinuedUndoGroup(_typingWithoutABreak);
      else
         _document.UndoStack.StartUndoGroup(_typingWithoutABreak);

      makeChanges();
      _document.UndoStack.EndUndoGroup();
   }

   ///<summary>Has text that is typed from now on undone apart from what was typed until now.</summary>
   internal void EndTyping() => _typingWithoutABreak = new object();
}
