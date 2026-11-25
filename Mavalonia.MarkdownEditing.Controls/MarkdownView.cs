using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Mavalonia.MarkdownEditing.Controls.Rendered;
using Mavalonia.MarkdownEditing.Controls.Rendered.Input;
using Mavalonia.MarkdownEditing.Editing;
using Mavalonia.MarkdownEditing.Rendering;
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
   public static readonly RoutedEvent<LinkClickedEventArgs> LinkClickedEvent = RoutedEvent.Register<MarkdownView, LinkClickedEventArgs>(nameof(LinkClicked), RoutingStrategies.Bubble);

   readonly ReadingKeys _keys;
   readonly MarkdownToRead _read = new("", MarkdownDialect.ForReading);

   static MarkdownView() => IsTabStopProperty.OverrideDefaultValue<MarkdownView>(false);

   public MarkdownView()
   {
      _keys = new ReadingKeys(this);
      _read.Changed += (_, _) => InvalidateWhatIsShown();
   }

   public string Markdown { get => GetValue(MarkdownProperty); set => SetValue(MarkdownProperty, value); }

   ///<summary>What is markup in <see cref="Markdown"/>. <see cref="MarkdownDialect.PlainText"/> shows text that isn't markdown.</summary>
   public MarkdownDialect Dialect { get => GetValue(DialectProperty); set => SetValue(DialectProperty, value); }

   ///<summary>A person has clicked a link, or an image. The view itself opens nothing.</summary>
   public event EventHandler<LinkClickedEventArgs>? LinkClicked { add => AddHandler(LinkClickedEvent, value); remove => RemoveHandler(LinkClickedEvent, value); }

   ///<summary>All the text that is shown, such as to search in.</summary>
   public PlainText PlainText => _read.Rendered.PlainText;

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
   }

   protected override void OnKeyDown(KeyEventArgs e) => _keys.Press(e);

   internal override void PointAt(Point point) => Cursor = Layout.LinkAt(point) is null ? Cursors.OfText : Cursors.OfALink;

   internal override void ClickAt(Point point)
   {
      if(Layout.LinkAt(point) is {} link) RaiseEvent(new LinkClickedEventArgs(link.Target));
   }

   internal override void SelectingStarts() => SelectionOfAWindow.IsNowIn(this);
}
