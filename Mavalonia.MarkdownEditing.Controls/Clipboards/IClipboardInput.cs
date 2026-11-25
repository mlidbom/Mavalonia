namespace Mavalonia.MarkdownEditing.Controls.Clipboards;

///<summary>What a person does with the clipboard in one of the views of a <see cref="MarkdownEditor"/>.</summary>
interface IClipboardInput
{
   ///<summary>Does nothing when nothing is selected.</summary>
   Task Copy();

   ///<inheritdoc cref="Copy"/>
   Task Cut();

   ///<summary>Pastes rich text as the markdown for it.</summary>
   Task Paste();

   ///<summary>Pastes only the text of what is on the clipboard.</summary>
   Task PasteText();

   Task PasteAsQuote();
}
