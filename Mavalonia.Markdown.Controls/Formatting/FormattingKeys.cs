using Avalonia.Input;
using Mavalonia.Markdown.Rendering.Containers;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Controls.Formatting;

///<summary>Which format, or which kind of list, a key that is pressed toggles: by the keys set now on <see cref="IHasFormattingKeys"/>, such as <see cref="IHasFormattingKeys.ToggleBoldGesture"/>.</summary>
class FormattingKeys(IHasFormattingKeys keys)
{
   ///<summary>The format that <paramref name="press"/> toggles. Null when it toggles none.</summary>
   internal TextFormats? FormatToggledBy(KeyEventArgs press)
   {
      if(keys.ToggleBoldGesture?.Matches(press) == true) return TextFormats.Bold;
      if(keys.ToggleItalicGesture?.Matches(press) == true) return TextFormats.Italic;
      if(keys.ToggleCodeGesture?.Matches(press) == true) return TextFormats.Code;
      if(keys.ToggleStrikethroughGesture?.Matches(press) == true) return TextFormats.Strikethrough;
      return null;
   }

   ///<summary>The kind of list that <paramref name="press"/> toggles. Null when it toggles none.</summary>
   internal ListKind? ListToggledBy(KeyEventArgs press)
   {
      if(keys.ToggleBulletedListGesture?.Matches(press) == true) return ListKind.Bulleted;
      if(keys.ToggleNumberedListGesture?.Matches(press) == true) return ListKind.Numbered;
      return null;
   }
}
