using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;

namespace Mavalonia.MarkdownEditing.Controls.Rendered;

partial class RenderedMarkdownView
{
   public static readonly StyledProperty<FontFamily> FontFamilyProperty = TextElement.FontFamilyProperty.AddOwner<RenderedMarkdownView>();
   public static readonly StyledProperty<double> FontSizeProperty = TextElement.FontSizeProperty.AddOwner<RenderedMarkdownView>();
   public static readonly StyledProperty<IBrush?> ForegroundProperty = TextElement.ForegroundProperty.AddOwner<RenderedMarkdownView>();
   public static readonly StyledProperty<FontFamily> MonospaceFontFamilyProperty = AvaloniaProperty.Register<RenderedMarkdownView, FontFamily>(nameof(MonospaceFontFamily), new FontFamily("Cascadia Mono, Consolas, Menlo, monospace"));
   public static readonly StyledProperty<IReadOnlyList<double>?> HeadingFontSizesProperty = AvaloniaProperty.Register<RenderedMarkdownView, IReadOnlyList<double>?>(nameof(HeadingFontSizes));
   public static readonly StyledProperty<IBrush?> FaintForegroundProperty = AvaloniaProperty.Register<RenderedMarkdownView, IBrush?>(nameof(FaintForeground));
   public static readonly StyledProperty<IBrush?> LinkForegroundProperty = AvaloniaProperty.Register<RenderedMarkdownView, IBrush?>(nameof(LinkForeground));
   public static readonly StyledProperty<IBrush> CodeBackgroundProperty = AvaloniaProperty.Register<RenderedMarkdownView, IBrush>(nameof(CodeBackground), new ImmutableSolidColorBrush(Colors.Gray, 0.15));
   public static readonly StyledProperty<IBrush> LineBrushProperty = AvaloniaProperty.Register<RenderedMarkdownView, IBrush>(nameof(LineBrush), new ImmutableSolidColorBrush(Colors.Gray, 0.5));
   public static readonly StyledProperty<IBrush> SelectionBrushProperty = AvaloniaProperty.Register<RenderedMarkdownView, IBrush>(nameof(SelectionBrush), new ImmutableSolidColorBrush(Colors.DodgerBlue, 0.4));
   public static readonly StyledProperty<Thickness> PaddingProperty = Decorator.PaddingProperty.AddOwner<RenderedMarkdownView>();

   static readonly AvaloniaProperty[] PropertiesThatTheLayoutDependsOn = [FontFamilyProperty, FontSizeProperty, ForegroundProperty, MonospaceFontFamilyProperty, HeadingFontSizesProperty, FaintForegroundProperty, LinkForegroundProperty, CodeBackgroundProperty, LineBrushProperty, SelectionBrushProperty, PaddingProperty];

   public FontFamily FontFamily { get => GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
   public double FontSize { get => GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
   public IBrush? Foreground { get => GetValue(ForegroundProperty); set => SetValue(ForegroundProperty, value); }

   ///<summary>The font of code.</summary>
   public FontFamily MonospaceFontFamily { get => GetValue(MonospaceFontFamilyProperty); set => SetValue(MonospaceFontFamilyProperty, value); }

   ///<summary>The font sizes of headings, from the first level on. Levels that it has none for get the last. Null for sizes in proportion to <see cref="FontSize"/>.</summary>
   public IReadOnlyList<double>? HeadingFontSizes { get => GetValue(HeadingFontSizesProperty); set => SetValue(HeadingFontSizesProperty, value); }

   ///<summary>What bullets, numbers and markdown shown as source are drawn with. Null for what the theme has.</summary>
   public IBrush? FaintForeground { get => GetValue(FaintForegroundProperty); set => SetValue(FaintForegroundProperty, value); }

   ///<summary>What links are drawn with. Null for what the theme has.</summary>
   public IBrush? LinkForeground { get => GetValue(LinkForegroundProperty); set => SetValue(LinkForegroundProperty, value); }

   public IBrush CodeBackground { get => GetValue(CodeBackgroundProperty); set => SetValue(CodeBackgroundProperty, value); }

   ///<summary>What the lines of tables, the bars of quotes and thematic breaks are drawn with.</summary>
   public IBrush LineBrush { get => GetValue(LineBrushProperty); set => SetValue(LineBrushProperty, value); }

   public IBrush SelectionBrush { get => GetValue(SelectionBrushProperty); set => SetValue(SelectionBrushProperty, value); }
   public Thickness Padding { get => GetValue(PaddingProperty); set => SetValue(PaddingProperty, value); }

   private protected MarkdownAppearance Appearance =>
      new(FontFamily,
          MonospaceFontFamily,
          FontSize,
          HeadingFontSizes,
          Foreground ?? FromTheTheme("TextControlForeground", Brushes.Black),
          FaintForeground ?? FromTheTheme("TextControlPlaceholderForeground", Brushes.Gray),
          LinkForeground ?? FromTheTheme(IsDark ? "SystemAccentColorLight2" : "SystemAccentColor", Brushes.RoyalBlue),
          CodeBackground,
          LineBrush,
          SelectionBrush,
          IsDark);

   private protected bool IsDark => ActualThemeVariant == ThemeVariant.Dark;

   IBrush FromTheTheme(string key, IBrush otherwise) => this.TryFindResource(key, ActualThemeVariant, out var found)
                                                           ? found switch
                                                           {
                                                              IBrush brush => brush,
                                                              Color color => new ImmutableSolidColorBrush(color),
                                                              _ => otherwise
                                                           }
                                                           : otherwise;
}
