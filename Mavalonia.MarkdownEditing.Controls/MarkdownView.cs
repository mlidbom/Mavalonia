using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Mavalonia.MarkdownEditing.Controls.Menus;
using Mavalonia.MarkdownEditing.Controls.Rendered;
using Mavalonia.MarkdownEditing.Controls.Rendered.Input;
using Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Controls;

///<summary>A control that shows markdown to be read. A person can select in it, and copies what is selected as the markdown that it is.</summary>
///<remarks>
/// Setting <see cref="Markdown"/> again is quick, such as for text that is still being written. What is selected stays selected, when the text before it is the same.
/// </remarks>
public class MarkdownView : RenderedMarkdownView
{
   public static readonly StyledProperty<string> MarkdownProperty = AvaloniaProperty.Register<MarkdownView, string>(nameof(Markdown), "", coerce: InsteadOfNull(""));
   public static readonly StyledProperty<MarkdownDialect> DialectProperty = AvaloniaProperty.Register<MarkdownView, MarkdownDialect>(nameof(Dialect), MarkdownDialect.ForReading, coerce: InsteadOfNull(MarkdownDialect.ForReading));
   public static readonly StyledProperty<IReadOnlyList<TextHighlight>> HighlightsProperty = AvaloniaProperty.Register<MarkdownView, IReadOnlyList<TextHighlight>>(nameof(Highlights), [], coerce: InsteadOfNull<IReadOnlyList<TextHighlight>>([]));
   public static readonly StyledProperty<Func<string, Task<IImage?>>?> LoadImageProperty = AvaloniaProperty.Register<MarkdownView, Func<string, Task<IImage?>>?>(nameof(LoadImage));
   public static readonly DirectProperty<MarkdownView, bool> IsAnythingSelectedProperty = AvaloniaProperty.RegisterDirect<MarkdownView, bool>(nameof(IsAnythingSelected), it => it.IsAnythingSelected);
   public static readonly RoutedEvent<LinkClickedEventArgs> LinkClickedEvent = RoutedEvent.Register<MarkdownView, LinkClickedEventArgs>(nameof(LinkClicked), RoutingStrategies.Bubble);

   readonly ReadingKeys _keys;
   readonly MarkdownToRead _read = new("", MarkdownDialect.ForReading);

   ///<summary>Null when none are loaded.</summary>
   LoadedImages? _images;

   ///<summary>Null until the pointer is over code for the first time.</summary>
   CopyCodeButton? _copyCode;

   ///<summary>Where the pointer is. Null when it isn't over the view.</summary>
   Point? _pointedAt;

   static MarkdownView()
   {
      IsTabStopProperty.OverrideDefaultValue<MarkdownView>(false);
      AffectsRender<MarkdownView>(HighlightsProperty);
   }

   public MarkdownView()
   {
      _keys = new ReadingKeys(this);
      _read.Changed += (_, _) => ShowTheChange();
      SetCurrentValue(ContextFlyoutProperty, new MenuOfAView(this));
   }

   void ShowTheChange()
   {
      InvalidateWhatIsShown();
      IsAnythingSelected = !_read.Selection.IsEmpty;
   }

   ///<summary>A binding may give null for what can't be null.</summary>
   static Func<AvaloniaObject, TValue, TValue> InsteadOfNull<TValue>(TValue value) where TValue : class => (_, given) => (TValue?)given ?? value;

   public string Markdown { get => GetValue(MarkdownProperty); set => SetValue(MarkdownProperty, value); }

   ///<summary>What is markup in <see cref="Markdown"/>. <see cref="MarkdownDialect.PlainText"/> shows text that isn't markdown.</summary>
   public MarkdownDialect Dialect { get => GetValue(DialectProperty); set => SetValue(DialectProperty, value); }

   ///<summary>Loads the image that the markdown says is at a place, or returns null when there is none to show. Null to load none, so that images are shown as their descriptions.</summary>
   ///<remarks>
   /// What an AI writes can hold an image whose address carries what it knows to somebody else, who gets it as soon as the image is loaded. So load only what can't do that, such as files, or images from places that are known.<br/>
   /// An exception in it surfaces on the dispatcher.
   /// </remarks>
   public Func<string, Task<IImage?>>? LoadImage { get => GetValue(LoadImageProperty); set => SetValue(LoadImageProperty, value); }

   ///<summary>A person has clicked a link, or an image. The view itself opens nothing.</summary>
   public event EventHandler<LinkClickedEventArgs>? LinkClicked { add => AddHandler(LinkClickedEvent, value); remove => RemoveHandler(LinkClickedEvent, value); }

   ///<summary>All the text that is shown, such as to search in.</summary>
   public PlainText PlainText => _read.Rendered.PlainText;

   ///<summary>What is highlighted. Where highlights overlap, the last is uppermost. They are drawn of the text that is there now: when that changes, set them anew.</summary>
   public IReadOnlyList<TextHighlight> Highlights { get => GetValue(HighlightsProperty); set => SetValue(HighlightsProperty, value); }

   private protected override IReadOnlyList<TextHighlight> HighlightsToDraw => Highlights;

   ///<summary>Where the text of <paramref name="range"/> is shown: a rectangle for each line of it.</summary>
   public IReadOnlyList<Rect> BoundsOf(PlainTextRange range) => [..PlainText.PartsOf(range).SelectMany(Layout.BoundsOf)];

   ///<summary>Where the link at <paramref name="point"/> of the view leads, as the markdown says it, or else the image there. Null when none is there.</summary>
   public string? TargetOfTheLinkAt(Point point) => Layout.TargetAt(point);

   ///<summary>Tells when it changes, such as for a command that needs something to be selected.</summary>
   public bool IsAnythingSelected { get; private set => SetAndRaise(IsAnythingSelectedProperty, ref field, value); }

   ///<summary>The markdown of what is selected. Empty when nothing is.</summary>
   public string SelectedMarkdown => _read.SelectedMarkdown;

   public void SelectAll()
   {
      SelectingStarts();
      _read.SelectAll();
   }

   public void SelectNothing() => _read.SelectNothing();

   ///<summary>Puts what is selected on the clipboard: as markdown, and as rich text for programs that take that. Does nothing when nothing is selected.</summary>
   public Task CopySelection() => IsAnythingSelected ? new CopiedSelection(_read).PutOn(TopLevel.GetTopLevel(this)!.Clipboard!) : Task.CompletedTask;

   internal override SelectableMarkdown Shown => _read;

   protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
   {
      base.OnPropertyChanged(change);
      if(change.Property == MarkdownProperty || change.Property == DialectProperty) _read.Show(Markdown, Dialect);
      else if(change.Property == LoadImageProperty) LoadAllImagesAnew();
   }

   void LoadAllImagesAnew()
   {
      _images = LoadImage is {} load ? new LoadedImages(load, whenLoaded: ShowOtherImages) : null;
      ShowOtherImages();
   }

   private protected override IImage? ImageOf(InlineImage image) => _images?.Of(image);

   protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
   {
      base.OnAttachedToVisualTree(e);
      if(IsAnythingSelected) SelectionOfAWindow.IsNowIn(this);
   }

   protected override Size MeasureOverride(Size availableSize)
   {
      _copyCode?.Measure(Size.Infinity);
      return base.MeasureOverride(availableSize);
   }

   protected override Size ArrangeOverride(Size finalSize)
   {
      if(_pointedAt is {} point) ShowWhatIsAt(point, LayOut(finalSize.Width));
      _copyCode?.ArrangeAtItsBlock();
      return base.ArrangeOverride(finalSize);
   }

   protected override void OnKeyDown(KeyEventArgs e) => _keys.Press(e);

   internal override void PointAt(Point point, KeyModifiers modifiers)
   {
      _pointedAt = point;
      ShowWhatIsAt(point, Layout);
      if(_copyCode is { IsVisible: true }) InvalidateArrange();
   }

   ///<summary>Shows what can be done with what is at <paramref name="point"/>: that a link can be clicked, and the button to copy code.</summary>
   void ShowWhatIsAt(Point point, DocumentLayout layout)
   {
      Cursor = layout.TargetAt(point) is null ? Cursors.OfText : Cursors.OfALink;

      if(layout.CodeAt(point) is {} code) ShowTheCopyButton(code.Block, code.Bounds);
      else _copyCode?.Hide();
   }

   void ShowTheCopyButton(CodeBlock block, Rect bounds)
   {
      if(_copyCode is null)
      {
         _copyCode = new CopyCodeButton();
         LogicalChildren.Add(_copyCode);
         VisualChildren.Add(_copyCode);
      }

      _copyCode.ShowFor(block, bounds);
   }

   protected override void OnPointerExited(PointerEventArgs e)
   {
      base.OnPointerExited(e);
      _pointedAt = null;
      _copyCode?.Hide();
   }

   internal override void ClickAt(Point point, KeyModifiers modifiers)
   {
      if(Layout.TargetAt(point) is {} target) RaiseEvent(new LinkClickedEventArgs(target, modifiers));
   }

   internal override void SelectingStarts() => SelectionOfAWindow.IsNowIn(this);
}
