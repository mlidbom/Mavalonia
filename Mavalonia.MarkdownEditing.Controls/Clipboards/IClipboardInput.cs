namespace Mavalonia.MarkdownEditing.Controls.Clipboards;

///<summary>What a person does with the clipboard in one of the views of a <see cref="MarkdownEditor"/>.</summary>
interface IClipboardInput
{
   ///<summary>Puts what is selected on the clipboard. Does nothing when nothing is selected.</summary>
   Task Copy();

   ///<summary>Puts what is selected on the clipboard, and deletes it. Does nothing when nothing is selected.</summary>
   Task Cut();

   ///<summary>Pastes what is on the clipboard in place of the selection: rich text as the markdown for it.</summary>
   Task Paste();

   ///<summary>Pastes only the text of what is on the clipboard, in place of the selection.</summary>
   Task PasteText();

   ///<summary>Pastes what is on the clipboard as a quote, in place of the selection.</summary>
   Task PasteAsQuote();
}
