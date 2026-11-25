using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Mavalonia.MarkdownEditing.Controls.Rendered.Input;
using Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Controls.Rendered;

///<summary>Shows a markdown document as it is rendered, and lets a person edit it there.</summary>
public partial class RenderedMarkdownView : Control
{
   const double WidthWhenAnyIsAvailable = 600;

   static readonly RenderedMarkdown NothingRendered = RenderedMarkdown.Of("");

   readonly BlinkingCaret _blinkingCaret;
   readonly KeyboardInput _keyboard;
   readonly PointerInput _pointer;
   readonly InputMethodClient _inputMethod;
   readonly LatestLayout _latestLayout = new();
   Rect _viewport = new(0, 0, double.MaxValue, double.MaxValue);

   static RenderedMarkdownView()
   {
      FocusableProperty.OverrideDefaultValue<RenderedMarkdownView>(true);
      ClipToBoundsProperty.OverrideDefaultValue<RenderedMarkdownView>(true);
      AffectsMeasure<RenderedMarkdownView>(PropertiesThatTheLayoutDependsOn);
      AffectsRender<RenderedMarkdownView>([..PropertiesThatTheLayoutDependsOn, WatermarkProperty]);
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

   ///<summary>Edits the document that is shown. Null while none is.</summary>
   public RenderedEditor? Editor
   {
      get;
      set
      {
         if(field is not null) field.Changed -= ShowTheChange;
         field = value;
         if(field is not null) field.Changed += ShowTheChange;
         InvalidateWhatIsShown();
      }
   }

   void ShowTheChange(object? sender, EventArgs e)
   {
      InvalidateWhatIsShown();
      if(IsFocused) _blinkingCaret.Show();
      _inputMethod.CaretHasMoved();
   }

   RenderedMarkdown Rendered => Editor?.Rendered ?? NothingRendered;

   void InvalidateWhatIsShown()
   {
      if(!_latestLayout.IsOf(Rendered, Bounds.Width)) InvalidateMeasure();
      InvalidateVisual();
   }

   ///<summary>The layout of what is rendered now, also when it has changed since the view was measured.</summary>
   internal DocumentLayout Layout => LayOut(Bounds.Width);

   DocumentLayout LayOut(double width) => _latestLayout.LayOut(Rendered, Appearance, width, Padding);

   protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
   {
      base.OnPropertyChanged(change);
      if(PropertiesThatTheLayoutDependsOn.Contains(change.Property)) _latestLayout.Forget();
   }

   protected override Size MeasureOverride(Size availableSize)
   {
      var width = double.IsInfinity(availableSize.Width) ? WidthWhenAnyIsAvailable : availableSize.Width;
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
      Layout.Draw(context, Editor.Selection.IsEmpty ? null : Editor.Selection.Range, _viewport);
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
