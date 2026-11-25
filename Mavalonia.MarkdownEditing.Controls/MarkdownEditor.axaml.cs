using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit.Document;
using Mavalonia.MarkdownEditing.Controls.Source;
using Mavalonia.MarkdownEditing.Editing;

namespace Mavalonia.MarkdownEditing.Controls;

///<summary>A control in which a person edits a markdown document, as it is rendered or as its source.</summary>
public partial class MarkdownEditor : UserControl
{
   public static readonly StyledProperty<TextDocument> DocumentProperty = AvaloniaProperty.Register<MarkdownEditor, TextDocument>(nameof(Document));
   public static readonly StyledProperty<bool> ShowsSourceProperty = AvaloniaProperty.Register<MarkdownEditor, bool>(nameof(ShowsSource));
   public static readonly StyledProperty<string?> WatermarkProperty = AvaloniaProperty.Register<MarkdownEditor, string?>(nameof(Watermark));
   public static readonly StyledProperty<KeyGesture?> StartNewBlockGestureProperty = AvaloniaProperty.Register<MarkdownEditor, KeyGesture?>(nameof(StartNewBlockGesture), new KeyGesture(Key.Enter));
   public static readonly StyledProperty<KeyGesture?> BreakLineGestureProperty = AvaloniaProperty.Register<MarkdownEditor, KeyGesture?>(nameof(BreakLineGesture), new KeyGesture(Key.Enter, KeyModifiers.Shift));
   public static readonly StyledProperty<KeyGesture?> PasteTextGestureProperty = AvaloniaProperty.Register<MarkdownEditor, KeyGesture?>(nameof(PasteTextGesture), new KeyGesture(Key.V, KeyModifiers.Control | KeyModifiers.Shift));
   public static readonly StyledProperty<KeyGesture?> PasteAsQuoteGestureProperty = AvaloniaProperty.Register<MarkdownEditor, KeyGesture?>(nameof(PasteAsQuoteGesture), new KeyGesture(Key.Q, KeyModifiers.Control | KeyModifiers.Shift));

   RenderedEditor? _renderedEditor;

   public MarkdownEditor()
   {
      InitializeComponent();
      SourceView.Options.AllowScrollBelowDocument = false;
      _ = new SourceInput(this, SourceView);
      SetCurrentValue(DocumentProperty, new TextDocument());
   }

   ///<summary>The document. Its text is the markdown, and is all that is edited: the two views show the same text.</summary>
   public TextDocument Document { get => GetValue(DocumentProperty); set => SetValue(DocumentProperty, value); }

   ///<summary>Whether the source is shown, rather than what it's rendered as.</summary>
   public bool ShowsSource { get => GetValue(ShowsSourceProperty); set => SetValue(ShowsSourceProperty, value); }

   ///<summary>What is shown while the document is empty.</summary>
   public string? Watermark { get => GetValue(WatermarkProperty); set => SetValue(WatermarkProperty, value); }

   ///<summary>The keys that start a new block, such as a paragraph or a list item, in the rendered view. Null for none.</summary>
   public KeyGesture? StartNewBlockGesture { get => GetValue(StartNewBlockGestureProperty); set => SetValue(StartNewBlockGestureProperty, value); }

   ///<summary>The keys that break the line inside a block, in the rendered view. Null for none.</summary>
   public KeyGesture? BreakLineGesture { get => GetValue(BreakLineGestureProperty); set => SetValue(BreakLineGestureProperty, value); }

   ///<summary>The keys that paste only the text of what is on the clipboard, when that is rich text. Null for none.</summary>
   public KeyGesture? PasteTextGesture { get => GetValue(PasteTextGestureProperty); set => SetValue(PasteTextGestureProperty, value); }

   ///<summary>The keys that paste what is on the clipboard as a quote. Null for none.</summary>
   public KeyGesture? PasteAsQuoteGesture { get => GetValue(PasteAsQuoteGestureProperty); set => SetValue(PasteAsQuoteGestureProperty, value); }

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

   ///<summary>Puts a quote of <paramref name="markdown"/> in place of the selection, and the caret in a paragraph after it.</summary>
   public void Quote(string markdown)
   {
      using var editor = new RenderedEditor(Document, Selection); // Another than that of the rendered view, which there is none of while the control isn't shown.
      editor.Quote(markdown);
      Selection = editor.Selection;
      BringTheCaretIntoView();
   }

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
