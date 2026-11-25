using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using Mavalonia.MarkdownEditing.Controls.Rendered.Input;
using Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Controls.Rendered;

///<summary>Shows a markdown document as it is rendered, and lets a person edit it there.</summary>
public class RenderedMarkdownView : Control
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

   static readonly AvaloniaProperty[] PropertiesThatTextIsLaidOutWith = [FontFamilyProperty, FontSizeProperty, ForegroundProperty, MonospaceFontFamilyProperty, FaintForegroundProperty, LinkForegroundProperty, CodeBackgroundProperty, LineBrushProperty, SelectionBrushProperty, PaddingProperty];
   static readonly RenderedMarkdown NothingRendered = RenderedMarkdown.Of("");

   readonly BlinkingCaret _blinkingCaret;
   readonly KeyboardInput _keyboard;
   readonly PointerInput _pointer;
   readonly InputMethodClient _inputMethod;

   TextLayouts? _texts;
   DocumentLayout? _layout;
   Rect _viewport = new(0, 0, double.MaxValue, double.MaxValue);

   static RenderedMarkdownView()
   {
      FocusableProperty.OverrideDefaultValue<RenderedMarkdownView>(true);
      ClipToBoundsProperty.OverrideDefaultValue<RenderedMarkdownView>(true);
      AffectsMeasure<RenderedMarkdownView>(PropertiesThatTextIsLaidOutWith);
      AffectsRender<RenderedMarkdownView>([..PropertiesThatTextIsLaidOutWith, WatermarkProperty]);
      TextInputMethodClientRequestedEvent.AddClassHandler<RenderedMarkdownView>((view, request) => request.Client = view._inputMethod);
   }

   public RenderedMarkdownView()
   {
      _blinkingCaret = new BlinkingCaret(InvalidateVisual);
      _keyboard = new KeyboardInput(this);
      _pointer = new PointerInput(this);
      _inputMethod = new InputMethodClient(this);
      Cursor = new Cursor(StandardCursorType.Ibeam);
      EffectiveViewportChanged += (_, change) => ShowWhatIsIn(change.EffectiveViewport);
   }

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

   ///<summary>Edits the document that is shown. Null while none is.</summary>
   public RenderedEditor? Editor
   {
      get;
      set
      {
         if(field is not null) field.Changed -= ShowTheChange;
         field = value;
         if(field is not null) field.Changed += ShowTheChange;
         LayOutAnew();
      }
   }

   void ShowTheChange(object? sender, EventArgs e)
   {
      LayOutAnew();
      if(IsFocused) _blinkingCaret.Show();
      _inputMethod.CaretHasMoved();
   }

   void LayOutAnew()
   {
      if(_layout?.IsOf(Editor?.Rendered ?? NothingRendered, Bounds.Width) != true) InvalidateMeasure();
      InvalidateVisual();
   }

   internal DocumentLayout Layout => _layout ?? LayOut(Bounds.Width);

   DocumentLayout LayOut(double width)
   {
      var rendered = Editor?.Rendered ?? NothingRendered;
      if(_layout?.IsOf(rendered, width) == true) return _layout;

      _texts ??= new TextLayouts(Appearance);
      return _layout = _texts.Use(() => DocumentLayoutBuilder.LayOut(rendered, Appearance, _texts, width, Padding));
   }

   MarkdownAppearance Appearance => new(FontFamily, MonospaceFontFamily, FontSize, Foreground ?? Brushes.Black, FaintForeground, LinkForeground, CodeBackground, LineBrush, SelectionBrush, CaretBrush: Foreground ?? Brushes.Black);

   protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
   {
      base.OnPropertyChanged(change);
      if(PropertiesThatTextIsLaidOutWith.Contains(change.Property)) ForgetHowTextIsLaidOut();
   }

   void ForgetHowTextIsLaidOut()
   {
      _texts?.Dispose();
      _texts = null;
      _layout = null;
   }

   protected override Size MeasureOverride(Size availableSize)
   {
      var width = double.IsInfinity(availableSize.Width) ? 600 : availableSize.Width;
      return LayOut(width).Size;
   }

   protected override Size ArrangeOverride(Size finalSize)
   {
      LayOut(finalSize.Width);
      return finalSize;
   }

   void ShowWhatIsIn(Rect viewport)
   {
      _viewport = viewport;
      InvalidateVisual();
   }

   public override void Render(DrawingContext context)
   {
      context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size)); // So that the pointer hits the view where nothing is drawn.
      if(Editor is null) return;

      if(Watermark is not null && Editor.Document.TextLength == 0) DrawTheWatermark(context, Watermark);
      Layout.Draw(context, Editor.Selection.IsEmpty ? null : new SourceRange(Editor.Selection.Start, Editor.Selection.End), _viewport);
      if(IsFocused && _blinkingCaret.IsSeen) context.FillRectangle(Appearance.CaretBrush, CaretRectangle);
   }

   void DrawTheWatermark(DrawingContext context, string watermark)
   {
      using var text = new TextLayout(watermark, new Typeface(FontFamily), FontSize, FaintForeground);
      text.Draw(context, Layout.StartOfTheFirstLine);
   }

   ///<summary>Where the caret is, whether or not it is seen right now.</summary>
   internal Rect CaretRectangle => Editor is null ? default : Layout.CaretAt(Editor.CaretPosition);

   internal void BringTheCaretIntoView()
   {
      UpdateLayout();
      this.BringIntoView(CaretRectangle.Inflate(new Thickness(0, 4)));
   }

   protected override void OnGotFocus(FocusChangedEventArgs e)
   {
      base.OnGotFocus(e);
      _blinkingCaret.Show();
      InvalidateVisual();
   }

   protected override void OnLostFocus(FocusChangedEventArgs e)
   {
      base.OnLostFocus(e);
      _blinkingCaret.Hide();
      InvalidateVisual();
   }

   protected override void OnTextInput(TextInputEventArgs e) => _keyboard.Type(e);
   protected override void OnKeyDown(KeyEventArgs e) => _keyboard.Press(e);

   protected override void OnPointerPressed(PointerPressedEventArgs e) => _pointer.Press(e);
   protected override void OnPointerMoved(PointerEventArgs e) => _pointer.Move(e);
   protected override void OnPointerReleased(PointerReleasedEventArgs e) => PointerInput.Release(e);

   protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
   {
      base.OnDetachedFromVisualTree(e);
      _blinkingCaret.Hide();
   }
}
