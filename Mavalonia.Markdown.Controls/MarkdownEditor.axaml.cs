using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit.Document;
using JetBrains.Annotations;
using Mavalonia.Markdown.Controls.Clipboards;
using Mavalonia.Markdown.Controls.Menus;
using Mavalonia.Markdown.Controls.Source;
using Mavalonia.Markdown.Editing;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Containers;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Controls;

///<summary>A control in which a person edits a markdown document, as it is rendered or as its source.</summary>
public partial class MarkdownEditor : UserControl
{
   readonly SourceInput _source;
   RenderedEditor? _renderedEditor;

   public MarkdownEditor()
   {
      InitializeComponent();
      SourceView.Options.AllowScrollBelowDocument = false;
      _source = new SourceInput(this, SourceView);
      RenderedView.AddHandler(MarkdownView.LinkClickedEvent, TellOfALinkClickedInTheRenderedView);
      RenderedView.AddHandler(PastedEvent, TellOfAPasteInTheRenderedView);
      SetCurrentValue(ContextFlyoutProperty, new MenuOfAMarkdownEditor(this));
      SetCurrentValue(DocumentProperty, new TextDocument());
   }

   ///<summary>The application is told by the editor, as of a link in the source view: the views are parts of the editor.</summary>
   void TellOfALinkClickedInTheRenderedView(object? sender, LinkClickedEventArgs click)
   {
      click.Handled = true;
      RaiseEvent(new LinkClickedEventArgs(click.Target, click.KeyModifiers));
   }

   ///<summary>The application is told by the editor, as of a paste in the source view: the views are parts of the editor.</summary>
   void TellOfAPasteInTheRenderedView(object? sender, PastedEventArgs paste)
   {
      paste.Handled = true;
      TellOfAPaste(paste.Written);
   }

   internal void TellOfAPaste(SourceRange written) => RaiseEvent(new PastedEventArgs(written));

   ///<summary>Edits what the rendered view shows. Null while the source is shown instead, and while the control isn't in a window.</summary>
   RenderedEditor? RenderedEditorShown => ShowsSource ? null : _renderedEditor;

   ///<summary>What is selected in the view that is shown.</summary>
   public TextSelection Selection
   {
      get => RenderedEditorShown?.Selection ?? SourceView.TextSelection;
      set
      {
         SourceView.TextSelection = value;
         _renderedEditor?.Select(value);
      }
   }

   [PublicAPI]
   public void SelectAll()
   {
      if(RenderedEditorShown is {} rendered) rendered.SelectAll();
      else Selection = new TextSelection(0, Document.TextLength);
   }

   IClipboardInput ClipboardInputOfTheViewShown => ShowsSource ? _source.ClipboardInput : RenderedView.ClipboardInput;

   ///<summary>Puts what is selected on the clipboard: as markdown, and, from the rendered view, as rich text for programs that take that. Does nothing when nothing is selected.</summary>
   [PublicAPI]
   public Task Copy() => ClipboardInputOfTheViewShown.Copy();

   ///<summary>Does what <see cref="Copy"/> does, and deletes what is selected.</summary>
   [PublicAPI]
   public Task Cut() => ClipboardInputOfTheViewShown.Cut();

   ///<summary>Pastes what is on the clipboard in place of the selection: rich text as the markdown for it.</summary>
   [PublicAPI]
   public Task Paste() => ClipboardInputOfTheViewShown.Paste();

   ///<summary>Pastes only the text of what is on the clipboard, in place of the selection.</summary>
   [PublicAPI]
   public Task PasteText() => ClipboardInputOfTheViewShown.PasteText();

   ///<summary>Pastes what is on the clipboard as a quote, in place of the selection.</summary>
   [PublicAPI]
   public Task PasteAsQuote() => ClipboardInputOfTheViewShown.PasteAsQuote();

   ///<summary>Puts a quote of <paramref name="markdown"/> in place of the selection, and the caret in a paragraph after it.</summary>
   ///<returns>The stretch of the markdown of <see cref="Document"/> that the quote wrote.</returns>
   public SourceRange Quote(string markdown) => EditInEitherView(it => it.Quote(markdown));

   ///<summary>Formats the selected text, or takes the format away when all of it has it.</summary>
   ///<remarks>With nothing selected, it's the text typed next in the rendered view that gets or loses the format. In the source view, nothing happens then.</remarks>
   public void ToggleFormat(TextFormats format)
   {
      if(RenderedEditorShown is {} rendered) rendered.Toggle(format);
      else EditInEitherView(it => it.Toggle(format));
   }

   ///<summary>Whether <see cref="ToggleFormat"/> does anything: with nothing selected, it's only the rendered view that the format of what is typed next is for.</summary>
   internal bool CanToggleAFormat => !ShowsSource || !Selection.IsEmpty;

   ///<summary>Whether the text typed next has <paramref name="format"/>, or, with text selected, all of that does: <see cref="ToggleFormat"/> then takes the format away.</summary>
   internal bool HasFormat(TextFormats format) => ReadInEitherView(it => it.Has(format));

   ///<summary>
   /// Makes each paragraph and heading that is selected, or that the caret is in, an item of a list of <paramref name="kind"/>.
   /// When each of them is one already, it takes them out of their lists instead.
   ///</summary>
   ///<remarks>An item of a list of the other kind turns its whole list into one of <paramref name="kind"/>.</remarks>
   public void ToggleList(ListKind kind)
   {
      if(RenderedEditorShown is {} rendered) rendered.ToggleList(kind);
      else EditInEitherView(it => it.ToggleList(kind));
   }

   ///<summary>Whether a paragraph or heading is selected, or the caret is in one, for <see cref="ToggleList"/> to make an item.</summary>
   internal bool CanToggleAList => ReadInEitherView(it => it.CanToggleAList);

   ///<summary>Whether each paragraph and heading that is selected, or that the caret is in, is an item of a list of <paramref name="kind"/>: <see cref="ToggleList"/> then takes them out of their lists.</summary>
   internal bool IsInAList(ListKind kind) => ReadInEitherView(it => it.IsInAList(kind));

   ///<summary>What <paramref name="read"/> reads of the editor of the rendered view, or, while the source is shown or the control isn't, of another of the same selection.</summary>
   TResult ReadInEitherView<TResult>(Func<RenderedEditor, TResult> read)
   {
      if(RenderedEditorShown is {} rendered) return read(rendered);

      using var editor = new RenderedEditor(Document, Selection);
      return read(editor);
   }

   ///<summary>Edits also while the source is shown, and while the control isn't.</summary>
   void EditInEitherView(Action<RenderedEditor> edit)
   {
      using var editor = new RenderedEditor(Document, Selection); // Another than that of the rendered view, which there is none of while the control isn't shown.
      edit(editor);
      Selection = editor.Selection;
      BringTheCaretIntoView();
   }

   ///<summary>Edits also while the source is shown, and while the control isn't.</summary>
   ///<returns>What <paramref name="edit"/> returns.</returns>
   TResult EditInEitherView<TResult>(Func<RenderedEditor, TResult> edit)
   {
      using var editor = new RenderedEditor(Document, Selection); // Another than that of the rendered view, which there is none of while the control isn't shown.
      var result = edit(editor);
      Selection = editor.Selection;
      BringTheCaretIntoView();
      return result;
   }

   ///<summary>The link that the markdown has at <paramref name="offset"/>. Null when it has none there. Only while the control is in a window.</summary>
   internal Link? LinkWrittenAt(int offset) => _renderedEditor!.Rendered.LinkWrittenAt(offset);

   ///<summary>Just below the caret of the view that is shown, from the corner of the control.</summary>
   internal Point PlaceBelowTheCaret => ShowsSource
                                           ? SourceView.TextArea.TextView.TranslatePoint(SourceView.PlaceBelowTheCaret, this)!.Value
                                           : RenderedView.TranslatePoint(RenderedView.CaretRectangle.BottomLeft, this)!.Value;

   void BringTheCaretIntoView()
   {
      UpdateLayout(); // A view that has just been made visible has no place for the caret before it is laid out.
      if(RenderedEditorShown is not null) RenderedView.BringTheCaretIntoView();
      else if(ShowsSource) SourceView.TextArea.Caret.BringCaretToView();
   }

   ///<summary>Puts the keyboard focus in the view that is shown.</summary>
   public void FocusTheView()
   {
      UpdateLayout(); // A view that has just been made visible can't have the focus before it is laid out.
      if(ShowsSource) SourceView.TextArea.Focus();
      else RenderedView.Focus();
   }

   protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
   {
      base.OnPropertyChanged(change);
      if(change.Property == DocumentProperty) Edit(change.GetNewValue<TextDocument>());
      else if(change.Property == ShowsSourceProperty) Show(source: change.GetNewValue<bool>());
      else if(change.Property == WatermarkProperty) SourceView.Watermark = change.GetNewValue<string?>();
   }

   protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
   {
      base.OnAttachedToVisualTree(e);
      if(_renderedEditor is null) EditAsRendered(Document, SourceView.TextSelection);
   }

   ///<summary>The document may outlive the control, and must not keep it alive.</summary>
   protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
   {
      base.OnDetachedFromVisualTree(e);
      SourceView.TextSelection = Selection;
      StopEditingAsRendered();
   }

   void Edit(TextDocument document)
   {
      SourceView.Document = document;
      SourceView.CaretOffset = document.TextLength;
      EditAsRendered(document, TextSelection.CaretAt(document.TextLength));
   }

   void EditAsRendered(TextDocument document, TextSelection selection)
   {
      StopEditingAsRendered();
      RenderedView.Editor = _renderedEditor = new RenderedEditor(document, selection);
   }

   void StopEditingAsRendered()
   {
      RenderedView.Editor = null;
      _renderedEditor?.Dispose();
      _renderedEditor = null;
   }

   void Show(bool source)
   {
      var hadFocus = IsKeyboardFocusWithin;

      if(source && _renderedEditor is not null) SourceView.TextSelection = _renderedEditor.Selection;
      else _renderedEditor?.Select(SourceView.TextSelection);

      SourceView.IsVisible = source;
      ScrollOfTheRenderedView.IsVisible = !source;
      if(hadFocus) FocusTheView();
      BringTheCaretIntoView();
   }
}
