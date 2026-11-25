using Avalonia.Input;
using Avalonia.Input.Platform;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Input;

///<summary>What a person does with the keyboard in a <see cref="RenderedEditorView"/>.</summary>
class KeyboardInput
{
   readonly RenderedEditorView _view;
   readonly MovementKeys _movement;
   readonly ClipboardInput _clipboard;

   internal KeyboardInput(RenderedEditorView view)
   {
      _view = view;
      _movement = new MovementKeys(view);
      _clipboard = new ClipboardInput(view);
   }

   PlatformHotkeyConfiguration Hotkeys => _view.Hotkeys;

   internal void Type(TextInputEventArgs e)
   {
      if(_view.Editor is not {} editor || string.IsNullOrEmpty(e.Text) || char.IsControl(e.Text[0])) return;

      e.Handled = true;
      editor.Type(e.Text);
      _view.BringTheCaretIntoView();
   }

   internal void Press(KeyEventArgs e)
   {
      if(_view.Editor is not {} editor || ActionOf(e, editor) is not {} action) return;

      e.Handled = true;
      action();
      _view.BringTheCaretIntoView();
   }

   Action? ActionOf(KeyEventArgs e, RenderedEditor editor) => EditOf(e, editor) ?? _movement.ActionOf(e, editor) ?? SelectionOf(e, editor) ?? ClipboardActionOf(e, editor);

   Action? EditOf(KeyEventArgs e, RenderedEditor editor)
   {
      if(_view.StartNewBlockGesture?.Matches(e) == true) return editor.StartNewBlock;
      if(_view.BreakLineGesture?.Matches(e) == true) return editor.BreakLine;
      if(Is(Hotkeys.Undo, e)) return editor.Undo;
      if(Is(Hotkeys.Redo, e)) return editor.Redo;

      var isForWords = e.KeyModifiers == Hotkeys.WholeWordTextActionModifiers;
      return (e.Key, e.KeyModifiers) switch
      {
         (Key.Back, KeyModifiers.None or KeyModifiers.Shift) => editor.DeleteCharacterBefore,
         (Key.Back, _) when isForWords => editor.DeleteWordBefore,
         (Key.Delete, KeyModifiers.None) => editor.DeleteCharacterAfter,
         (Key.Delete, _) when isForWords => editor.DeleteWordAfter,
         (Key.Tab, KeyModifiers.None) => Tab(editor),
         (Key.Tab, KeyModifiers.Shift) => ShiftTab(editor),
         (Key.B, _) when e.KeyModifiers == Hotkeys.CommandModifiers => () => editor.Toggle(TextFormats.Bold),
         (Key.I, _) when e.KeyModifiers == Hotkeys.CommandModifiers => () => editor.Toggle(TextFormats.Italic),
         (Key.E, _) when e.KeyModifiers == Hotkeys.CommandModifiers => () => editor.Toggle(TextFormats.Code),
         (Key.X, _) when e.KeyModifiers == (Hotkeys.CommandModifiers | KeyModifiers.Shift) => () => editor.Toggle(TextFormats.Strikethrough),
         _ => null
      };
   }

   ///<summary>Null where the key isn't for editing, so that it moves the focus as it does everywhere else.</summary>
   static Action? Tab(RenderedEditor editor) => editor.CaretPosition.Block switch
   {
      TableCell => editor.GoToTheNextCell,
      CodeBlock => () => editor.Type("   "),
      _ => editor.CanIndent ? editor.Indent : null
   };

   static Action? ShiftTab(RenderedEditor editor) => editor.CaretPosition.Block switch
   {
      TableCell => () => editor.Move(CaretMovement.ToTheBlockBefore),
      _ => editor.CanOutdent ? editor.Outdent : null
   };

   Action? SelectionOf(KeyEventArgs e, RenderedEditor editor) => Is(Hotkeys.SelectAll, e) ? editor.SelectAll : null;

   Action? ClipboardActionOf(KeyEventArgs e, RenderedEditor editor)
   {
      if(Is(Hotkeys.Copy, e)) return () => _clipboard.Copy(editor).SurfaceExceptions();
      if(Is(Hotkeys.Cut, e)) return () => _clipboard.Cut(editor).SurfaceExceptions();
      if(_view.PasteAsQuoteGesture?.Matches(e) == true) return () => _clipboard.PasteAsQuote(editor).SurfaceExceptions();
      if(_view.PasteTextGesture?.Matches(e) == true) return () => _clipboard.PasteText(editor).SurfaceExceptions();
      if(Is(Hotkeys.Paste, e)) return () => _clipboard.Paste(editor).SurfaceExceptions();
      return null;
   }

   static bool Is(IEnumerable<KeyGesture> gestures, KeyEventArgs e) => gestures.Any(it => it.Matches(e));
}
