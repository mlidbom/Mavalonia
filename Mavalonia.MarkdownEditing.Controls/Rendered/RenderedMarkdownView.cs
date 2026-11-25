using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using Mavalonia.MarkdownEditing.Controls.Rendered.Input;
using Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering;

namespace Mavalonia.MarkdownEditing.Controls.Rendered;

///<summary>Shows markdown as it is rendered, and lets a person select in it.</summary>
public abstract partial class RenderedMarkdownView : Control
{
   const double WidthWhenAnyIsAvailable = 600;
   const double HeightDrawnBeyondWhatIsSeen = 1200;

   static readonly RenderedMarkdown NothingRendered = RenderedMarkdown.Of("");

   readonly PointerInput _pointer;
   readonly LatestLayout _latestLayout = new();
   Rect _viewport = new(0, 0, double.MaxValue, double.MaxValue);

   ///<summary>The part of the view that was drawn last. Scrolling inside it needs nothing to be drawn anew.</summary>
   Rect _drawn;

   bool _isGettingTheFocus;

   static RenderedMarkdownView()
   {
      FocusableProperty.OverrideDefaultValue<RenderedMarkdownView>(true);
      ClipToBoundsProperty.OverrideDefaultValue<RenderedMarkdownView>(true);
      AffectsMeasure<RenderedMarkdownView>(PropertiesThatTheLayoutDependsOn);
      AffectsRender<RenderedMarkdownView>([..PropertiesThatTheLayoutDependsOn, ColoursCodeProperty]);
      GotFocusEvent.AddClassHandler<RenderedMarkdownView>((view, _) => view._isGettingTheFocus = true);
      GotFocusEvent.AddClassHandler<TopLevel>((_, focus) => HasGotTheFocus(focus.Source), handledEventsToo: true); // The last that is told, after what scrolls.
      RequestBringIntoViewEvent.AddClassHandler<RenderedMarkdownView>((view, request) => view.StayWhereItIsWhenItGetsTheFocus(request));
   }

   private protected RenderedMarkdownView()
   {
      _pointer = new PointerInput(this);
      Cursor = Cursors.OfText;
      EffectiveViewportChanged += (_, change) => ShowWhatIsIn(change.EffectiveViewport);
      ActualThemeVariantChanged += (_, _) => LayOutAnew();
      ResourcesChanged += (_, _) => LayOutAnewWhenItLooksOtherwise();
   }

   ///<summary>Avalonia shows a control as in a mirror where the direction is from right to left, unless it says that its text mustn't be.</summary>
   protected override bool BypassFlowDirectionPolicies => true;

   ///<summary>What is shown. Null while nothing is.</summary>
   internal abstract SelectableMarkdown? Shown { get; }

   internal PlatformHotkeyConfiguration Hotkeys => this.GetPlatformSettings()!.HotkeyConfiguration;

   RenderedMarkdown Rendered => Shown?.Rendered ?? NothingRendered;

   ///<summary>To be done when what is shown, or what is selected in it, has changed.</summary>
   private protected void InvalidateWhatIsShown()
   {
      if(!_latestLayout.IsOf(Rendered, Bounds.Width)) InvalidateMeasure();
      InvalidateVisual();
   }

   void LayOutAnew()
   {
      _latestLayout.ForgetAll();
      InvalidateMeasure();
      InvalidateVisual();
   }

   ///<summary>Resources change for many reasons, most of which change nothing about the view. While it isn't in a window, it has none, and keeps the looks that it had.</summary>
   void LayOutAnewWhenItLooksOtherwise()
   {
      if(((ILogical)this).IsAttachedToLogicalTree && _latestLayout.LooksOtherwiseThan(Appearance)) LayOutAnew();
   }

   ///<summary>To be done when there are other images to show than before.</summary>
   private protected void ShowOtherImages()
   {
      _latestLayout.ForgetAllButTheTexts();
      InvalidateMeasure();
      InvalidateVisual();
   }

   ///<summary>The layout of what is rendered now, also when it has changed since the view was measured.</summary>
   internal DocumentLayout Layout => LayOut(Bounds.Width);

   private protected DocumentLayout LayOut(double width) => _latestLayout.LayOut(Rendered, () => Appearance, width, Padding);

   protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
   {
      base.OnPropertyChanged(change);
      if(PropertiesThatTheLayoutDependsOn.Contains(change.Property)) _latestLayout.ForgetAll();
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
      var seen = viewport.Intersect(new Rect(Bounds.Size));
      if(seen is { Width: > 0, Height: > 0 } && !_drawn.Contains(seen)) InvalidateVisual();
   }

   public sealed override void Render(DrawingContext context)
   {
      context.FillRectangle(Brushes.Transparent, new Rect(Bounds.Size)); // So that the pointer hits the view where nothing is drawn.
      _drawn = _viewport.Inflate(new Thickness(0, HeightDrawnBeyondWhatIsSeen));
      if(Shown is not {} shown) return;

      Layout.Draw(context, shown.Selection.IsEmpty ? null : shown.Selection.Range, HighlightsToDraw, _drawn);
      DrawOver(context);
   }

   ///<summary>What is highlighted, the uppermost last.</summary>
   private protected virtual IReadOnlyList<TextHighlight> HighlightsToDraw => [];

   ///<summary>Draws what is to be over the text.</summary>
   private protected virtual void DrawOver(DrawingContext context) {}

   ///<summary>Where the end of the selection that moves is.</summary>
   internal Rect CaretRectangle => Shown is null ? default : Layout.CaretAt(Shown.CaretPosition);

   internal void BringTheCaretIntoView()
   {
      UpdateLayout();
      this.BringIntoView(CaretRectangle.Inflate(new Thickness(0, 4)));
   }

   ///<summary>A view that gets the focus is to be brought into view as a whole. What is higher than the place it's in would be scrolled to its start by that.</summary>
   void StayWhereItIsWhenItGetsTheFocus(RequestBringIntoViewEventArgs request)
   {
      if(_isGettingTheFocus) request.Handled = true;
   }

   static void HasGotTheFocus(object? focused)
   {
      if(focused is RenderedMarkdownView view) view._isGettingTheFocus = false;
   }

   protected override AutomationPeer OnCreateAutomationPeer() => new RenderedMarkdownAutomationPeer(this);

   protected override void OnPointerPressed(PointerPressedEventArgs e)
   {
      base.OnPointerPressed(e);
      _pointer.Press(e);
   }

   protected override void OnPointerMoved(PointerEventArgs e)
   {
      base.OnPointerMoved(e);
      _pointer.Move(e);
   }

   protected override void OnPointerReleased(PointerReleasedEventArgs e)
   {
      _pointer.Release(e);
      base.OnPointerReleased(e); // Opens the menu.
   }

   protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
   {
      base.OnPointerCaptureLost(e);
      _pointer.EndSelecting();
   }

   ///<summary>The pointer is at <paramref name="point"/>, and isn't selecting.</summary>
   internal virtual void PointAt(Point point, KeyModifiers modifiers) {}

   ///<summary>A person has started to select in the view.</summary>
   internal virtual void SelectingStarts() {}
}
