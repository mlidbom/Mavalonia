using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace Mavalonia.MarkdownEditing.Controls.Rendered;

partial class RenderedMarkdownView
{
   public static readonly StyledProperty<FontFamily> FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner<RenderedMarkdownView>();
   public static readonly StyledProperty<double> FontSizeProperty = TextElement.FontSizeProperty.AddOwner<RenderedMarkdownView>();
   public static readonly StyledProperty<IBrush?> ForegroundProperty = TextElement.ForegroundProperty.AddOwner<RenderedMarkdownView>();
   public static readonly StyledProperty<FontFamily> MonospaceFontFamilyProperty = AvaloniaProperty.Register<RenderedMarkdownView, FontFamily>(nameof(MonospaceFontFamily), new FontFamily("Cascadia Mono, Consolas, Menlo, monospace"));
   public static readonly StyledProperty<IBrush> FaintForegroundProperty = AvaloniaProperty.Register<RenderedMarkdownView, IBrush>(nameof(FaintForeground), Brushes.Gray);
   public static readonly StyledProperty<IBrush> LinkForegroundProperty = AvaloniaProperty.Register<RenderedMarkdownView, IBrush>(nameof(LinkForeground), Brushes.RoyalBlue);
   public static readonly StyledProperty<IBrush> CodeBackgroundProperty = AvaloniaProperty.Register<RenderedMarkdownView, IBrush>(nameof(CodeBackground), new ImmutableSolidColorBrush(Colors.Gray, 0.15));
   public static readonly StyledProperty<IBrush> LineBrushProperty = AvaloniaProperty.Register<RenderedMarkdownView, IBrush>(nameof(LineBrush), new ImmutableSolidColorBrush(Colors.Gray, 0.5));
   public static readonly StyledProperty<IBrush> SelectionBrushProperty = AvaloniaProperty.Register<RenderedMarkdownView, IBrush>(nameof(SelectionBrush), new ImmutableSolidColorBrush(Colors.DodgerBlue, 0.4));
   public static readonly StyledProperty<Thickness> PaddingProperty = Decorator.PaddingProperty.AddOwner<RenderedMarkdownView>();
   public static readonly StyledProperty<string?> WatermarkProperty = AvaloniaProperty.Register<RenderedMarkdownView, string?>(nameof(Watermark));
   public static readonly StyledProperty<KeyGesture?> StartNewBlockGestureProperty = MarkdownEditor.StartNewBlockGestureProperty.AddOwner<RenderedMarkdownView>();
   public static readonly StyledProperty<KeyGesture?> BreakLineGestureProperty = MarkdownEditor.BreakLineGestureProperty.AddOwner<RenderedMarkdownView>();
   public static readonly StyledProperty<KeyGesture?> PasteTextGestureProperty = MarkdownEditor.PasteTextGestureProperty.AddOwner<RenderedMarkdownView>();
   public static readonly StyledProperty<KeyGesture?> PasteAsQuoteGestureProperty = MarkdownEditor.PasteAsQuoteGestureProperty.AddOwner<RenderedMarkdownView>();

   static readonly AvaloniaProperty[] PropertiesThatTheLayoutDependsOn = [FontFamilyProperty, FontSizeProperty, ForegroundProperty, MonospaceFontFamilyProperty, FaintForegroundProperty, LinkForegroundProperty, CodeBackgroundProperty, LineBrushProperty, SelectionBrushProperty, PaddingProperty];

   public FontFamily FontFamily { get => GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
   public double FontSize { get => GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
   public IBrush? Foreground { get => GetValue(ForegroundProperty); set => SetValue(ForegroundProperty, value); }

   ///<summary>The font of code.</summary>
   public FontFamily MonospaceFontFamily { get => GetValue(MonospaceFontFamilyProperty); set => SetValue(MonospaceFontFamilyProperty, value); }

   ///<summary>What bullets, numbers, markdown shown as source and the <see cref="Watermark"/> are drawn with.</summary>
   public IBrush FaintForeground { get => GetValue(FaintForegroundProperty); set => SetValue(FaintForegroundProperty, value); }

   public IBrush LinkForeground { get => GetValue(LinkForegroundProperty); set => SetValue(LinkForegroundProperty, value); }
   public IBrush CodeBackground { get => GetValue(CodeBackgroundProperty); set => SetValue(CodeBackgroundProperty, value); }

   ///<summary>What the lines of tables, the bars of quotes and thematic breaks are drawn with.</summary>
   public IBrush LineBrush { get => GetValue(LineBrushProperty); set => SetValue(LineBrushProperty, value); }

   public IBrush SelectionBrush { get => GetValue(SelectionBrushProperty); set => SetValue(SelectionBrushProperty, value); }
   public Thickness Padding { get => GetValue(PaddingProperty); set => SetValue(PaddingProperty, value); }

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

   MarkdownAppearance Appearance => new(FontFamily, MonospaceFontFamily, FontSize, Foreground ?? Brushes.Black, FaintForeground, LinkForeground, CodeBackground, LineBrush, SelectionBrush, CaretBrush: Foreground ?? Brushes.Black);
}
