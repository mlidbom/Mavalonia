using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Mavalonia.MarkdownEditing.Controls.Rendered;
using Mavalonia.MarkdownEditing.Controls.Rendered.Input;
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
   public static readonly StyledProperty<string> MarkdownProperty = AvaloniaProperty.Register<MarkdownView, string>(nameof(Markdown), "");
   public static readonly StyledProperty<MarkdownDialect> DialectProperty = AvaloniaProperty.Register<MarkdownView, MarkdownDialect>(nameof(Dialect), MarkdownDialect.ForReading);
   public static readonly StyledProperty<IReadOnlyList<TextHighlight>> HighlightsProperty = AvaloniaProperty.Register<MarkdownView, IReadOnlyList<TextHighlight>>(nameof(Highlights), []);
   public static readonly StyledProperty<Func<string, Task<IImage?>>?> LoadImageProperty = AvaloniaProperty.Register<MarkdownView, Func<string, Task<IImage?>>?>(nameof(LoadImage));
   public static readonly RoutedEvent<LinkClickedEventArgs> LinkClickedEvent = RoutedEvent.Register<MarkdownView, LinkClickedEventArgs>(nameof(LinkClicked), RoutingStrategies.Bubble);

   ///<summary>The images that have been asked for, by where they are. Null for one that isn't loaded, or that there isn't.</summary>
   readonly Dictionary<string, IImage?> _images = [];

   readonly ReadingKeys _keys;
   readonly MarkdownToRead _read = new("", MarkdownDialect.ForReading);

   ///<summary>Null until the pointer is over code for the first time.</summary>
   CopyCodeButton? _copyCode;

   static MarkdownView()
   {
      IsTabStopProperty.OverrideDefaultValue<MarkdownView>(false);
      AffectsRender<MarkdownView>(HighlightsProperty);
   }

   public MarkdownView()
   {
      _keys = new ReadingKeys(this);
      _read.Changed += (_, _) => InvalidateWhatIsShown();
   }

   public string Markdown { get => GetValue(MarkdownProperty); set => SetValue(MarkdownProperty, value); }

   ///<summary>What is markup in <see cref="Markdown"/>. <see cref="MarkdownDialect.PlainText"/> shows text that isn't markdown.</summary>
   public MarkdownDialect Dialect { get => GetValue(DialectProperty); set => SetValue(DialectProperty, value); }

   ///<summary>Loads the image that the markdown says is at a place, or returns null when there is none to show. Null to load none, so that images are shown as their descriptions.</summary>
   ///<remarks>
   /// What an AI writes can hold an image whose address carries what it knows to somebody else, who gets it as soon as the image is loaded. So load only what can't do that, such as files, or images from places that are known.
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

   public bool IsAnythingSelected => !_read.Selection.IsEmpty;

   ///<summary>The markdown of what is selected. Empty when nothing is.</summary>
   public string SelectedMarkdown => _read.SelectedMarkdown;

   public void SelectAll()
   {
      SelectingStarts();
      _read.SelectAll();
   }

   public void SelectNothing() => _read.SelectNothing();

   ///<summary>Puts what is selected on the clipboard: as markdown, and as rich text for programs that take that. Does nothing when nothing is selected.</summary>
   public Task CopySelection() => IsAnythingSelected ? ClipboardContent.Write(_read, TopLevel.GetTopLevel(this)!.Clipboard!) : Task.CompletedTask;

   private protected override SelectableMarkdown Shown => _read;

   protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
   {
      base.OnPropertyChanged(change);
      if(change.Property == MarkdownProperty || change.Property == DialectProperty) _read.Show(Markdown, Dialect);
      else if(change.Property == LoadImageProperty) LoadAllImagesAnew();
   }

   void LoadAllImagesAnew()
   {
      _images.Clear();
      ShowAnImageThatHasBecomeKnown();
   }

   private protected override IImage? ImageOf(InlineImage image)
   {
      if(LoadImage is null) return null;
      if(!_images.TryGetValue(image.Source, out var loaded)) Load(image.Source, LoadImage);

      return loaded;
   }

   // ReSharper disable once AsyncVoidMethod An exception in it surfaces where all of the application's do.
   async void Load(string source, Func<string, Task<IImage?>> load)
   {
      _images[source] = null;
      await Task.Yield(); // It's asked for while what is shown is being laid out.
      if(await load(source) is not {} image) return;

      _images[source] = image;
      ShowAnImageThatHasBecomeKnown();
   }

   protected override Size MeasureOverride(Size availableSize)
   {
      _copyCode?.Measure(Size.Infinity);
      return base.MeasureOverride(availableSize);
   }

   protected override Size ArrangeOverride(Size finalSize)
   {
      _copyCode?.ArrangeAtItsBlock();
      return base.ArrangeOverride(finalSize);
   }

   protected override void OnKeyDown(KeyEventArgs e) => _keys.Press(e);

   internal override void PointAt(Point point)
   {
      Cursor = Layout.TargetAt(point) is null ? Cursors.OfText : Cursors.OfALink;

      if(Layout.CodeAt(point) is {} code) ShowTheCopyButton(code.Block, code.Bounds);
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
      InvalidateArrange();
   }

   protected override void OnPointerExited(PointerEventArgs e)
   {
      base.OnPointerExited(e);
      _copyCode?.Hide();
   }

   internal override void ClickAt(Point point)
   {
      if(Layout.TargetAt(point) is {} target) RaiseEvent(new LinkClickedEventArgs(target));
   }

   internal override void SelectingStarts() => SelectionOfAWindow.IsNowIn(this);
}
