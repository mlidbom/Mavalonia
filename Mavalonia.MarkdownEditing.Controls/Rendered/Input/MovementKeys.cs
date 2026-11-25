using Avalonia.Input;
using Avalonia.Input.Platform;
using Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Input;

///<summary>The keys that move the caret, or, together with Shift, the end of the selection.</summary>
class MovementKeys
{
   readonly RenderedMarkdownView _view;
   readonly VerticalMovement _verticalMovement;

   internal MovementKeys(RenderedMarkdownView view)
   {
      _view = view;
      _verticalMovement = new VerticalMovement(view);
   }

   PlatformHotkeyConfiguration Hotkeys => _view.Hotkeys;

   internal Action? ActionOf(KeyEventArgs e, SelectableMarkdown selectable)
   {
      var extendsSelection = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
      var modifiers = e.KeyModifiers & ~KeyModifiers.Shift;
      var isForWords = modifiers == Hotkeys.WholeWordTextActionModifiers;

      return (e.Key, modifiers) switch
      {
         (Key.Left, KeyModifiers.None) when !extendsSelection && !selectable.Selection.IsEmpty => Move(CaretMovement.ToTheStartOfTheSelection),
         (Key.Right, KeyModifiers.None) when !extendsSelection && !selectable.Selection.IsEmpty => Move(CaretMovement.ToTheEndOfTheSelection),
         (Key.Left, KeyModifiers.None) => Move(CaretMovement.ToTheCharacterBefore),
         (Key.Right, KeyModifiers.None) => Move(CaretMovement.ToTheCharacterAfter),
         (Key.Left, _) when isForWords => Move(CaretMovement.ToTheWordBefore),
         (Key.Right, _) when isForWords => Move(CaretMovement.ToTheWordAfter),
         (Key.Home, KeyModifiers.None) => MoveTo(line => line.Start),
         (Key.End, KeyModifiers.None) => MoveTo(line => line.End),
         (Key.Home, _) when modifiers == Hotkeys.CommandModifiers => Move(CaretMovement.ToTheStart),
         (Key.End, _) when modifiers == Hotkeys.CommandModifiers => Move(CaretMovement.ToTheEnd),
         (Key.Up, KeyModifiers.None) => () => _verticalMovement.Up(selectable, extendsSelection),
         (Key.Down, KeyModifiers.None) => () => _verticalMovement.Down(selectable, extendsSelection),
         _ => null
      };

      Action Move(CaretMovement movement) => () => selectable.Move(movement, extendsSelection);
      Action MoveTo(Func<LineLayout, RenderedPosition> positionOn) => () => selectable.MoveTo(positionOn(_view.Layout.LineOf(selectable.CaretPosition)), extendsSelection);
   }
}
