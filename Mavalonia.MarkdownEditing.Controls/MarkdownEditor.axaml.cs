using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit.Document;
using Mavalonia.MarkdownEditing.Controls.Menus;
using Mavalonia.MarkdownEditing.Controls.Source;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Controls;

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
      SetCurrentValue(ContextFlyoutProperty, new MenuOfAnEditor(this));
      SetCurrentValue(DocumentProperty, new TextDocument());
   }

   ///<summary>What is selected in the view that is shown.</summary>
   public TextSelection Selection
   {
      get => ShowsSource || _renderedEditor is null ? SourceView.TextSelection : _renderedEditor.Selection;
      set
      {
         SourceView.TextSelection = value;
         _renderedEditor?.Select(value);
      }
   }

   public void SelectAll()
   {
      if(ShowsSource || _renderedEditor is null) Selection = new TextSelection(0, Document.TextLength);
      else _renderedEditor.SelectAll();
   }

   IClipboardInput Clipboard => ShowsSource ? _source.Clipboard : RenderedView.Clipboard;

   ///<summary>Puts what is selected on the clipboard: as markdown, and, from the rendered view, as rich text for programs that take that. Does nothing when nothing is selected.</summary>
   public Task Copy() => Clipboard.Copy();

   ///<summary>Does what <see cref="Copy"/> does, and deletes what is selected.</summary>
   public Task Cut() => Clipboard.Cut();

   ///<summary>Pastes what is on the clipboard in place of the selection: rich text as the markdown for it.</summary>
   public Task Paste() => Clipboard.Paste();

   ///<summary>Pastes only the text of what is on the clipboard, in place of the selection.</summary>
   public Task PasteText() => Clipboard.PasteText();

   ///<summary>Pastes what is on the clipboard as a quote, in place of the selection.</summary>
   public Task PasteAsQuote() => Clipboard.PasteAsQuote();

   ///<summary>Puts a quote of <paramref name="markdown"/> in place of the selection, and the caret in a paragraph after it.</summary>
   public void Quote(string markdown) => EditInEitherView(it => it.Quote(markdown));

   ///<summary>Formats the selected text, or takes the format away when all of it has it.</summary>
   ///<remarks>With nothing selected, it's the text typed next in the rendered view that gets or loses the format. In the source view, nothing happens then.</remarks>
   public void ToggleFormat(TextFormats format)
   {
      if(ShowsSource || _renderedEditor is null) EditInEitherView(it => it.Toggle(format));
      else _renderedEditor.Toggle(format);
   }

   ///<summary>Edits also while the source is shown, and while the control isn't.</summary>
   void EditInEitherView(Action<RenderedEditor> edit)
   {
      using var editor = new RenderedEditor(Document, Selection); // Another than that of the rendered view, which there is none of while the control isn't shown.
      edit(editor);
      Selection = editor.Selection;
      BringTheCaretIntoView();
   }

   ///<summary>Just below the caret of the view that is shown, from the corner of the control.</summary>
   internal Point PlaceBelowTheCaret => ShowsSource
                                           ? SourceView.TextArea.TextView.TranslatePoint(SourceView.PlaceBelowTheCaret, this)!.Value
                                           : RenderedView.TranslatePoint(RenderedView.CaretRectangle.BottomLeft, this)!.Value;

   void BringTheCaretIntoView()
   {
      UpdateLayout(); // A view that has just been made visible has no place for the caret before it is laid out.
      if(ShowsSource) SourceView.TextArea.Caret.BringCaretToView();
      else if(_renderedEditor is not null) RenderedView.BringTheCaretIntoView();
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
