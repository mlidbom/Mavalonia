using Avalonia;
using Avalonia.Input;

namespace Mavalonia.MarkdownEditing.Controls.Rendered;

partial class RenderedEditorView
{
   public static readonly StyledProperty<string?> WatermarkProperty = AvaloniaProperty.Register<RenderedEditorView, string?>(nameof(Watermark));
   public static readonly StyledProperty<KeyGesture?> StartNewBlockGestureProperty = MarkdownEditor.StartNewBlockGestureProperty.AddOwner<RenderedEditorView>();
   public static readonly StyledProperty<KeyGesture?> BreakLineGestureProperty = MarkdownEditor.BreakLineGestureProperty.AddOwner<RenderedEditorView>();
   public static readonly StyledProperty<KeyGesture?> PasteTextGestureProperty = MarkdownEditor.PasteTextGestureProperty.AddOwner<RenderedEditorView>();
   public static readonly StyledProperty<KeyGesture?> PasteAsQuoteGestureProperty = MarkdownEditor.PasteAsQuoteGestureProperty.AddOwner<RenderedEditorView>();

   ///<summary>What is shown while the document is empty.</summary>
   public string? Watermark { get => GetValue(WatermarkProperty); set => SetValue(WatermarkProperty, value); }

   ///<inheritdoc cref="MarkdownEditor.StartNewBlockGesture"/>
   public KeyGesture? StartNewBlockGesture { get => GetValue(StartNewBlockGestureProperty); set => SetValue(StartNewBlockGestureProperty, value); }

   ///<inheritdoc cref="MarkdownEditor.BreakLineGesture"/>
   public KeyGesture? BreakLineGesture { get => GetValue(BreakLineGestureProperty); set => SetValue(BreakLineGestureProperty, value); }

   ///<inheritdoc cref="MarkdownEditor.PasteTextGesture"/>
   public KeyGesture? PasteTextGesture { get => GetValue(PasteTextGestureProperty); set => SetValue(PasteTextGestureProperty, value); }

   ///<inheritdoc cref="MarkdownEditor.PasteAsQuoteGesture"/>
   public KeyGesture? PasteAsQuoteGesture { get => GetValue(PasteAsQuoteGestureProperty); set => SetValue(PasteAsQuoteGestureProperty, value); }
}
