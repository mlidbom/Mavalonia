using Avalonia;
using Avalonia.Input;
using JetBrains.Annotations;

namespace Mavalonia.Markdown.Controls.Rendered;

partial class RenderedEditorView
{
   public static readonly StyledProperty<string?> WatermarkProperty = AvaloniaProperty.Register<RenderedEditorView, string?>(nameof(Watermark));
   public static readonly StyledProperty<KeyGesture?> StartNewBlockGestureProperty = MarkdownEditor.StartNewBlockGestureProperty.AddOwner<RenderedEditorView>();
   public static readonly StyledProperty<KeyGesture?> BreakLineGestureProperty = MarkdownEditor.BreakLineGestureProperty.AddOwner<RenderedEditorView>();
   public static readonly StyledProperty<KeyGesture?> ToggleBoldGestureProperty = MarkdownEditor.ToggleBoldGestureProperty.AddOwner<RenderedEditorView>();
   public static readonly StyledProperty<KeyGesture?> ToggleItalicGestureProperty = MarkdownEditor.ToggleItalicGestureProperty.AddOwner<RenderedEditorView>();
   public static readonly StyledProperty<KeyGesture?> ToggleCodeGestureProperty = MarkdownEditor.ToggleCodeGestureProperty.AddOwner<RenderedEditorView>();
   public static readonly StyledProperty<KeyGesture?> ToggleStrikethroughGestureProperty = MarkdownEditor.ToggleStrikethroughGestureProperty.AddOwner<RenderedEditorView>();
   public static readonly StyledProperty<KeyGesture?> ToggleBulletedListGestureProperty = MarkdownEditor.ToggleBulletedListGestureProperty.AddOwner<RenderedEditorView>();
   public static readonly StyledProperty<KeyGesture?> ToggleNumberedListGestureProperty = MarkdownEditor.ToggleNumberedListGestureProperty.AddOwner<RenderedEditorView>();
   public static readonly StyledProperty<KeyGesture?> PasteTextGestureProperty = MarkdownEditor.PasteTextGestureProperty.AddOwner<RenderedEditorView>();
   public static readonly StyledProperty<KeyGesture?> PasteAsQuoteGestureProperty = MarkdownEditor.PasteAsQuoteGestureProperty.AddOwner<RenderedEditorView>();

   ///<summary>What is shown while the document is empty.</summary>
   public string? Watermark { get => GetValue(WatermarkProperty); set => SetValue(WatermarkProperty, value); }

   ///<inheritdoc cref="MarkdownEditor.StartNewBlockGesture"/>
   [PublicAPI]
   public KeyGesture? StartNewBlockGesture { get => GetValue(StartNewBlockGestureProperty); set => SetValue(StartNewBlockGestureProperty, value); }

   ///<inheritdoc cref="MarkdownEditor.BreakLineGesture"/>
   [PublicAPI]
   public KeyGesture? BreakLineGesture { get => GetValue(BreakLineGestureProperty); set => SetValue(BreakLineGestureProperty, value); }

   ///<inheritdoc cref="MarkdownEditor.ToggleBoldGesture"/>
   [PublicAPI]
   public KeyGesture? ToggleBoldGesture { get => GetValue(ToggleBoldGestureProperty); set => SetValue(ToggleBoldGestureProperty, value); }

   ///<inheritdoc cref="MarkdownEditor.ToggleItalicGesture"/>
   [PublicAPI]
   public KeyGesture? ToggleItalicGesture { get => GetValue(ToggleItalicGestureProperty); set => SetValue(ToggleItalicGestureProperty, value); }

   ///<inheritdoc cref="MarkdownEditor.ToggleCodeGesture"/>
   [PublicAPI]
   public KeyGesture? ToggleCodeGesture { get => GetValue(ToggleCodeGestureProperty); set => SetValue(ToggleCodeGestureProperty, value); }

   ///<inheritdoc cref="MarkdownEditor.ToggleStrikethroughGesture"/>
   [PublicAPI]
   public KeyGesture? ToggleStrikethroughGesture { get => GetValue(ToggleStrikethroughGestureProperty); set => SetValue(ToggleStrikethroughGestureProperty, value); }

   ///<inheritdoc cref="MarkdownEditor.ToggleBulletedListGesture"/>
   [PublicAPI]
   public KeyGesture? ToggleBulletedListGesture { get => GetValue(ToggleBulletedListGestureProperty); set => SetValue(ToggleBulletedListGestureProperty, value); }

   ///<inheritdoc cref="MarkdownEditor.ToggleNumberedListGesture"/>
   [PublicAPI]
   public KeyGesture? ToggleNumberedListGesture { get => GetValue(ToggleNumberedListGestureProperty); set => SetValue(ToggleNumberedListGestureProperty, value); }

   ///<inheritdoc cref="MarkdownEditor.PasteTextGesture"/>
   [PublicAPI]
   public KeyGesture? PasteTextGesture { get => GetValue(PasteTextGestureProperty); set => SetValue(PasteTextGestureProperty, value); }

   ///<inheritdoc cref="MarkdownEditor.PasteAsQuoteGesture"/>
   [PublicAPI]
   public KeyGesture? PasteAsQuoteGesture { get => GetValue(PasteAsQuoteGestureProperty); set => SetValue(PasteAsQuoteGestureProperty, value); }
}
