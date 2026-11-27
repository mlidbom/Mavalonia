using Avalonia.Input;
using Mavalonia.Markdown.Controls.Rendered;

namespace Mavalonia.Markdown.Controls.Formatting;

///<summary>
/// What the keys that toggle formats and lists are set on: a <see cref="MarkdownEditor"/>, where the application sets them,<br/>
/// and its <see cref="RenderedEditorView"/>, which is bound to the editor's.
///</summary>
interface IHasFormattingKeys
{
   ///<inheritdoc cref="MarkdownEditor.ToggleBoldGesture"/>
   KeyGesture? ToggleBoldGesture { get; }

   ///<inheritdoc cref="MarkdownEditor.ToggleItalicGesture"/>
   KeyGesture? ToggleItalicGesture { get; }

   ///<inheritdoc cref="MarkdownEditor.ToggleCodeGesture"/>
   KeyGesture? ToggleCodeGesture { get; }

   ///<inheritdoc cref="MarkdownEditor.ToggleStrikethroughGesture"/>
   KeyGesture? ToggleStrikethroughGesture { get; }

   ///<inheritdoc cref="MarkdownEditor.ToggleBulletedListGesture"/>
   KeyGesture? ToggleBulletedListGesture { get; }

   ///<inheritdoc cref="MarkdownEditor.ToggleNumberedListGesture"/>
   KeyGesture? ToggleNumberedListGesture { get; }
}
