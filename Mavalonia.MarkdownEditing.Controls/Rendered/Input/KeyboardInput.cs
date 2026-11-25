using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;
using Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Input;

///<summary>What a person does with the keyboard in a <see cref="RenderedMarkdownView"/>.</summary>
class KeyboardInput
{
   readonly RenderedMarkdownView _view;
   readonly VerticalMovement _verticalMovement;

   internal KeyboardInput(RenderedMarkdownView view)
   {
      _view = view;
      _verticalMovement = new VerticalMovement(view);
   }

   TopLevel TopLevel => TopLevel.GetTopLevel(_view)!;
   PlatformHotkeyConfiguration Hotkeys => _view.GetPlatformSettings()!.HotkeyConfiguration;

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

   Action? ActionOf(KeyEventArgs e, RenderedEditor editor) => EditOf(e, editor) ?? MovementOf(e, editor) ?? ClipboardActionOf(e, editor);

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
      TableCell => () => editor.Move(CaretMovement.ToTheBlockAfter),
      CodeBlock => () => editor.Type("   "),
      _ => editor.CanIndent ? editor.Indent : null
   };

   static Action? ShiftTab(RenderedEditor editor) => editor.CaretPosition.Block switch
   {
      TableCell => () => editor.Move(CaretMovement.ToTheBlockBefore),
      _ => editor.CanOutdent ? editor.Outdent : null
   };

   Action? MovementOf(KeyEventArgs e, RenderedEditor editor)
   {
      var extendsSelection = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
      var modifiers = e.KeyModifiers & ~KeyModifiers.Shift;
      var isForWords = modifiers == Hotkeys.WholeWordTextActionModifiers;

      return (e.Key, modifiers) switch
      {
         (Key.Left, KeyModifiers.None) when !extendsSelection && !editor.Selection.IsEmpty => () => editor.MoveTo(editor.Rendered.PositionAt(editor.Selection.Start)),
         (Key.Right, KeyModifiers.None) when !extendsSelection && !editor.Selection.IsEmpty => () => editor.MoveTo(editor.Rendered.PositionAt(editor.Selection.End)),
         (Key.Left, KeyModifiers.None) => Move(CaretMovement.ToTheCharacterBefore),
         (Key.Right, KeyModifiers.None) => Move(CaretMovement.ToTheCharacterAfter),
         (Key.Left, _) when isForWords => Move(CaretMovement.ToTheWordBefore),
         (Key.Right, _) when isForWords => Move(CaretMovement.ToTheWordAfter),
         (Key.Home, KeyModifiers.None) => MoveTo(line => line.Start),
         (Key.End, KeyModifiers.None) => MoveTo(line => line.End),
         (Key.Home, _) when modifiers == Hotkeys.CommandModifiers => Move(CaretMovement.ToTheStart),
         (Key.End, _) when modifiers == Hotkeys.CommandModifiers => Move(CaretMovement.ToTheEnd),
         (Key.Up, KeyModifiers.None) => () => _verticalMovement.Up(editor, extendsSelection),
         (Key.Down, KeyModifiers.None) => () => _verticalMovement.Down(editor, extendsSelection),
         (Key.A, _) when Is(Hotkeys.SelectAll, e) => editor.SelectAll,
         _ => null
      };

      Action Move(CaretMovement movement) => () => editor.Move(movement, extendsSelection);
      Action MoveTo(Func<LineLayout, RenderedPosition> positionOn) => () => editor.MoveTo(positionOn(_view.Layout.LineOf(editor.CaretPosition)), extendsSelection);
   }

   Action? ClipboardActionOf(KeyEventArgs e, RenderedEditor editor)
   {
      if(Is(Hotkeys.Copy, e)) return () => _ = Copy(editor);
      if(Is(Hotkeys.Cut, e)) return () => _ = Cut(editor);
      if(Is(Hotkeys.Paste, e)) return () => _ = Paste(editor);
      return null;
   }

   async Task Copy(RenderedEditor editor)
   {
      if(editor.SelectedMarkdown is { Length: > 0 } markdown) await TopLevel.Clipboard!.SetTextAsync(markdown);
   }

   async Task Cut(RenderedEditor editor)
   {
      if(editor.Selection.IsEmpty) return;

      var copying = Copy(editor);
      editor.DeleteCharacterBefore();
      await copying;
   }

   async Task Paste(RenderedEditor editor)
   {
      if(await TopLevel.Clipboard!.TryGetTextAsync() is not { Length: > 0 } text) return;

      editor.Paste(text);
      _view.BringTheCaretIntoView();
   }

   static bool Is(IEnumerable<KeyGesture> gestures, KeyEventArgs e) => gestures.Any(it => it.Matches(e));
}
