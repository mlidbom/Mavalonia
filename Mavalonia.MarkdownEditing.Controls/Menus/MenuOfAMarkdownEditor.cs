using Avalonia;
using Avalonia.Controls;

namespace Mavalonia.MarkdownEditing.Controls.Menus;

///<summary>The menu that a <see cref="MarkdownEditor"/> has when the application gives it no other.</summary>
class MenuOfAMarkdownEditor : MenuMadeWhenItOpens
{
   readonly MarkdownEditor _editor;

   internal MenuOfAMarkdownEditor(MarkdownEditor editor) : base(editor) => _editor = editor;

   private protected override Point PlaceBelowTheCaret => _editor.PlaceBelowTheCaret;

   private protected override IEnumerable<Control> MakeItems()
   {
      var keys = _editor.Hotkeys;
      var isAnythingSelected = !_editor.Selection.IsEmpty;

      yield return new MenuCommand("Cut", _editor.Cut, keys.Cut.FirstOrDefault()) { IsEnabled = isAnythingSelected };
      yield return new MenuCommand("Copy", _editor.Copy, keys.Copy.FirstOrDefault()) { IsEnabled = isAnythingSelected };
      yield return new MenuCommand("Paste", _editor.Paste, keys.Paste.FirstOrDefault());
      yield return new MenuCommand("Paste as text", _editor.PasteText, _editor.PasteTextGesture);
      yield return new MenuCommand("Paste as quote", _editor.PasteAsQuote, _editor.PasteAsQuoteGesture);
      yield return new Separator();
      yield return new MenuCommand("Select all", _editor.SelectAll, keys.SelectAll.FirstOrDefault()) { IsEnabled = _editor.Document.TextLength > 0 };
   }
}
