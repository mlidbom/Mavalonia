using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit.Document;

namespace Mavalonia.MarkdownEditing.Controls;

partial class MarkdownEditor
{
   ///<summary>The modifier key of commands on the platform. <see cref="Avalonia.Input.Platform.PlatformHotkeyConfiguration.CommandModifiers"/> says it too, but only of a control in a window, and a default value is the same for every control.</summary>
   static readonly KeyModifiers ControlOrCommand = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;

   public static readonly StyledProperty<TextDocument> DocumentProperty = AvaloniaProperty.Register<MarkdownEditor, TextDocument>(nameof(Document));
   public static readonly StyledProperty<bool> ShowsSourceProperty = AvaloniaProperty.Register<MarkdownEditor, bool>(nameof(ShowsSource));
   public static readonly StyledProperty<string?> WatermarkProperty = AvaloniaProperty.Register<MarkdownEditor, string?>(nameof(Watermark));
   public static readonly StyledProperty<KeyGesture?> StartNewBlockGestureProperty = AvaloniaProperty.Register<MarkdownEditor, KeyGesture?>(nameof(StartNewBlockGesture), new KeyGesture(Key.Enter));
   public static readonly StyledProperty<KeyGesture?> BreakLineGestureProperty = AvaloniaProperty.Register<MarkdownEditor, KeyGesture?>(nameof(BreakLineGesture), new KeyGesture(Key.Enter, KeyModifiers.Shift));
   public static readonly StyledProperty<KeyGesture?> ToggleBoldGestureProperty = AvaloniaProperty.Register<MarkdownEditor, KeyGesture?>(nameof(ToggleBoldGesture), new KeyGesture(Key.B, ControlOrCommand));
   public static readonly StyledProperty<KeyGesture?> ToggleItalicGestureProperty = AvaloniaProperty.Register<MarkdownEditor, KeyGesture?>(nameof(ToggleItalicGesture), new KeyGesture(Key.I, ControlOrCommand));
   public static readonly StyledProperty<KeyGesture?> ToggleCodeGestureProperty = AvaloniaProperty.Register<MarkdownEditor, KeyGesture?>(nameof(ToggleCodeGesture), new KeyGesture(Key.E, ControlOrCommand));
   public static readonly StyledProperty<KeyGesture?> ToggleStrikethroughGestureProperty = AvaloniaProperty.Register<MarkdownEditor, KeyGesture?>(nameof(ToggleStrikethroughGesture), new KeyGesture(Key.X, ControlOrCommand | KeyModifiers.Shift));
   public static readonly StyledProperty<KeyGesture?> PasteTextGestureProperty = AvaloniaProperty.Register<MarkdownEditor, KeyGesture?>(nameof(PasteTextGesture), new KeyGesture(Key.V, ControlOrCommand | KeyModifiers.Shift));
   public static readonly StyledProperty<KeyGesture?> PasteAsQuoteGestureProperty = AvaloniaProperty.Register<MarkdownEditor, KeyGesture?>(nameof(PasteAsQuoteGesture), new KeyGesture(Key.Q, ControlOrCommand | KeyModifiers.Shift));

   public static readonly RoutedEvent<LinkClickedEventArgs> LinkClickedEvent = MarkdownView.LinkClickedEvent;

   ///<summary>The document. Its text is the markdown, and is all that is edited: the two views show the same text.</summary>
   public TextDocument Document { get => GetValue(DocumentProperty); set => SetValue(DocumentProperty, value); }

   ///<summary>Whether the source is shown, rather than what it's rendered as.</summary>
   public bool ShowsSource { get => GetValue(ShowsSourceProperty); set => SetValue(ShowsSourceProperty, value); }

   ///<summary>What is shown while the document is empty.</summary>
   public string? Watermark { get => GetValue(WatermarkProperty); set => SetValue(WatermarkProperty, value); }

   ///<summary>A person has clicked a link. The editor itself opens nothing.</summary>
   ///<remarks>
   /// A click puts the caret in the link too, to edit it. So an application may want to open only what is clicked with Ctrl pressed: see <see cref="LinkClickedEventArgs.KeyModifiers"/>.<br/>
   /// Links are those that markdown has, and, in the source view, URLs too, wherever they are.
   /// </remarks>
   public event EventHandler<LinkClickedEventArgs>? LinkClicked { add => AddHandler(LinkClickedEvent, value); remove => RemoveHandler(LinkClickedEvent, value); }

   ///<summary>The keys that start a new block, such as a paragraph or a list item, in the rendered view. Null for none.</summary>
   public KeyGesture? StartNewBlockGesture { get => GetValue(StartNewBlockGestureProperty); set => SetValue(StartNewBlockGestureProperty, value); }

   ///<summary>The keys that break the line inside a block, in the rendered view. Null for none.</summary>
   public KeyGesture? BreakLineGesture { get => GetValue(BreakLineGestureProperty); set => SetValue(BreakLineGestureProperty, value); }

   ///<summary>The keys that make text bold, or no longer bold, in the rendered view. Null for none.</summary>
   public KeyGesture? ToggleBoldGesture { get => GetValue(ToggleBoldGestureProperty); set => SetValue(ToggleBoldGestureProperty, value); }

   ///<summary>The keys that make text italic, or no longer italic, in the rendered view. Null for none.</summary>
   public KeyGesture? ToggleItalicGesture { get => GetValue(ToggleItalicGestureProperty); set => SetValue(ToggleItalicGestureProperty, value); }

   ///<summary>The keys that make text code, or no longer code, in the rendered view. Null for none.</summary>
   public KeyGesture? ToggleCodeGesture { get => GetValue(ToggleCodeGestureProperty); set => SetValue(ToggleCodeGestureProperty, value); }

   ///<summary>The keys that strike text through, or no longer do, in the rendered view. Null for none.</summary>
   public KeyGesture? ToggleStrikethroughGesture { get => GetValue(ToggleStrikethroughGestureProperty); set => SetValue(ToggleStrikethroughGestureProperty, value); }

   ///<summary>The keys that paste only the text of what is on the clipboard, when that is rich text. Null for none.</summary>
   public KeyGesture? PasteTextGesture { get => GetValue(PasteTextGestureProperty); set => SetValue(PasteTextGestureProperty, value); }

   ///<summary>The keys that paste what is on the clipboard as a quote. Null for none.</summary>
   public KeyGesture? PasteAsQuoteGesture { get => GetValue(PasteAsQuoteGestureProperty); set => SetValue(PasteAsQuoteGestureProperty, value); }
}
