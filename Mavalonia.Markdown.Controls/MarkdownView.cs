using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using JetBrains.Annotations;
using Mavalonia.Markdown.Controls.Clipboards;
using Mavalonia.Markdown.Controls.Menus;
using Mavalonia.Markdown.Controls.Rendered;
using Mavalonia.Markdown.Controls.Rendered.Input;
using Mavalonia.Markdown.Controls.Rendered.Layouts;
using Mavalonia.Markdown.Editing;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Controls;

///<summary>A control that shows markdown to be read. A person can select in it, and copies what is selected as the markdown that it is.</summary>
///<remarks>
/// Setting <see cref="Markdown"/> again is quick, such as for text that is still being written. What is selected stays selected, when the text before it is the same.
/// </remarks>
public class MarkdownView : RenderedMarkdownView
{
   [PublicAPI]
   public static readonly StyledProperty<string> MarkdownProperty = AvaloniaProperty.Register<MarkdownView, string>(nameof(Markdown), "", coerce: InsteadOfNull(""));
   [PublicAPI]
   public static readonly StyledProperty<MarkdownDialect> DialectProperty = AvaloniaProperty.Register<MarkdownView, MarkdownDialect>(nameof(Dialect), MarkdownDialect.ForReading, coerce: InsteadOfNull(MarkdownDialect.ForReading));
   [PublicAPI]
   public static readonly StyledProperty<IReadOnlyList<TextHighlight>> HighlightsProperty = AvaloniaProperty.Register<MarkdownView, IReadOnlyList<TextHighlight>>(nameof(Highlights), [], coerce: InsteadOfNull<IReadOnlyList<TextHighlight>>([]));
   [PublicAPI]
   public static readonly StyledProperty<IReadOnlyList<TextLink>> LinksProperty = AvaloniaProperty.Register<MarkdownView, IReadOnlyList<TextLink>>(nameof(Links), [], coerce: InsteadOfNull<IReadOnlyList<TextLink>>([]));
   [PublicAPI]
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
      AffectsRender<MarkdownView>(HighlightsProperty, LinksProperty);
   }

   public MarkdownView()
   {
      _keys = new ReadingKeys(this);
      _read.Changed += (_, _) => ShowTheChange();
      SetCurrentValue(ContextFlyoutProperty, new MenuOfAMarkdownView(this));
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

   ///<summary>A person has clicked a link, of the markdown or of <see cref="Links"/>, or an image. The view itself opens nothing.</summary>
   public event EventHandler<LinkClickedEventArgs>? LinkClicked { add => AddHandler(LinkClickedEvent, value); remove => RemoveHandler(LinkClickedEvent, value); }

   ///<summary>All the text that is shown, such as to search in.</summary>
   public PlainText PlainText => _read.Rendered.PlainText;

   ///<summary>What is highlighted. Where highlights overlap, the last is uppermost. They are drawn of the text that is there now: when that changes, set them anew.</summary>
   public IReadOnlyList<TextHighlight> Highlights { get => GetValue(HighlightsProperty); set => SetValue(HighlightsProperty, value); }

   private protected override IReadOnlyList<TextHighlight> HighlightsToDraw => Highlights;

   ///<summary>
   /// Text that is shown as links where the markdown has none, such as names that the application knows. They are drawn as the markdown's links are, in
   /// <see cref="RenderedMarkdownView.LinkForeground"/> and underlined, in code too, and are clicked as they are: see <see cref="LinkClicked"/>.
   ///</summary>
   ///<remarks>
   /// The markdown stays as it is, as does what is copied of it. Where one overlaps a link of the markdown, that of the markdown is clicked. Where they overlap
   /// one another, the last is. They are of the text that is there now: when that changes, set them anew.<br/>
   /// Setting them lays out nothing anew: they change no sizes, nor where lines end.
   ///</remarks>
   public IReadOnlyList<TextLink> Links { get => GetValue(LinksProperty); set => SetValue(LinksProperty, value); }

   private protected override IReadOnlyList<TextLink> TextLinks => Links;

   ///<summary>No more than what it shows needs, as a <see cref="Avalonia.Controls.TextBlock"/> asks for: aligned to a side rather than stretched, such as in the bubble of a message, it's no wider than its widest line.</summary>
   ///<remarks>Laid out again at that width, its lines end where they did, since each of them fits in it and the next word didn't fit in the width that it was offered.</remarks>
   private protected override double WidthToAskFor(DocumentLayout layout) => layout.WidthOfWhatItShows;

   ///<summary>Where the text of <paramref name="range"/> is shown: a rectangle for each line of it.</summary>
   public IReadOnlyList<Rect> BoundsOf(PlainTextRange range) => [..PlainText.PartsOf(range).SelectMany(Layout.BoundsOf)];

   ///<summary>Where the link at <paramref name="point"/> of the view leads, as the markdown says it, or else the image there, or else the last of <see cref="Links"/> there. Null when none is there.</summary>
   public string? TargetOfTheLinkAt(Point point) => TargetAt(point);

   ///<summary>Tells when it changes, such as for a command that needs something to be selected.</summary>
   public bool IsAnythingSelected { get; private set => SetAndRaise(IsAnythingSelectedProperty, ref field, value); }

   ///<summary>The markdown of what is selected. Empty when nothing is.</summary>
   public string SelectedMarkdown => _read.SelectedMarkdown;

   public void SelectAll()
   {
      SelectingStarts();
      _read.SelectAll();
   }

   [PublicAPI]
   public void SelectNothing() => _read.SelectNothing();

   ///<summary>Puts what is selected on the clipboard: as markdown, and as rich text for programs that take that. Does nothing when nothing is selected.</summary>
   [PublicAPI]
   public Task Copy() => IsAnythingSelected ? new CopiedSelection(_read).PutOn(this.Clipboard) : Task.CompletedTask;

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
      Cursor = layout.TargetAt(point, Links) is null ? Cursors.OfText : Cursors.OfALink;

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

   internal override void SelectingStarts() => SelectionOfAWindow.IsNowIn(this);
}
