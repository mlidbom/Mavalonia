using AvaloniaEdit.Document;
using Mavalonia.MarkdownEditing.Editing.Changes;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Editing;

///<summary>A markdown document together with what it is rendered as. Others may change the document at any time, such as an editor of its source.</summary>
sealed class RenderedDocument : IDisposable
{
   RenderedMarkdown? _rendered;
   bool _isMakingAChange;

   internal RenderedDocument(TextDocument document)
   {
      Document = document;
      Document.Changed += FollowTheChange;
   }

   internal TextDocument Document { get; }

   internal RenderedMarkdown Rendered => _rendered ??= RenderedMarkdown.Of(Document.Text);

   ///<summary>Tells of a change that wasn't made with <see cref="Make"/>: one made by another editor, or by undoing.</summary>
   internal event EventHandler<DocumentChangeEventArgs>? ChangedByOthers;

   public void Dispose() => Document.Changed -= FollowTheChange;

   internal void Make(SourceChange change)
   {
      _isMakingAChange = true;
      if(!change.ChangesNothing) Document.Replace(change.Replaced.Start, change.Replaced.Length, change.Replacement);
      _isMakingAChange = false;

      _rendered = change.After;
   }

   void FollowTheChange(object? sender, DocumentChangeEventArgs change)
   {
      if(_isMakingAChange) return;

      _rendered = null;
      ChangedByOthers?.Invoke(this, change);
   }
}
