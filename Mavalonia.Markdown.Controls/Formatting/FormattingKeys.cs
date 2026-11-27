using Avalonia.Input;
using Mavalonia.Markdown.Rendering.Containers;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Controls.Formatting;

///<summary>Which format, or which kind of list, a key that is pressed toggles: by the keys set now on <see cref="IHasFormattingKeys"/>, such as <see cref="IHasFormattingKeys.ToggleBoldGesture"/>.</summary>
class FormattingKeys
{
   readonly IHasFormattingKeys _keys;

   internal FormattingKeys(IHasFormattingKeys keys) => _keys = keys;

   ///<summary>The format that <paramref name="press"/> toggles. Null when it toggles none.</summary>
   internal TextFormats? FormatToggledBy(KeyEventArgs press)
   {
      if(_keys.ToggleBoldGesture?.Matches(press) == true) return TextFormats.Bold;
      if(_keys.ToggleItalicGesture?.Matches(press) == true) return TextFormats.Italic;
      if(_keys.ToggleCodeGesture?.Matches(press) == true) return TextFormats.Code;
      if(_keys.ToggleStrikethroughGesture?.Matches(press) == true) return TextFormats.Strikethrough;
      return null;
   }

   ///<summary>The kind of list that <paramref name="press"/> toggles. Null when it toggles none.</summary>
   internal ListKind? ListToggledBy(KeyEventArgs press)
   {
      if(_keys.ToggleBulletedListGesture?.Matches(press) == true) return ListKind.Bulleted;
      if(_keys.ToggleNumberedListGesture?.Matches(press) == true) return ListKind.Numbered;
      return null;
   }
}
