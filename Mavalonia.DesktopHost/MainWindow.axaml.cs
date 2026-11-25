using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using AvaloniaEdit.Document;
using Mavalonia.MarkdownEditing.Controls;

namespace Mavalonia.DesktopHost;

///<summary>Has an editor, and beside it what is written there as it's to be read, and tabs of sample documents to select and copy from.</summary>
partial class MainWindow : Window
{
   public MainWindow()
   {
      InitializeComponent();

      var document = new TextDocument(Samples.ReadWhatTheEditorStartsWith());
      var preview = new TabWithAMarkdownView("Preview", document.Text);
      document.TextChanged += (_, _) => preview.View.Markdown = document.Text;
      Editor.Document = document;

      Tabs.Items.Add(preview);
      foreach(var sample in Samples.ReadTheDocumentsForTabs()) Tabs.Items.Add(new TabWithAMarkdownView(sample.Name, sample.Markdown));
      AddHandler(MarkdownView.LinkClickedEvent, TellOfTheClickedLink); // Of the editor and of all the views.
   }

   MarkdownView ViewOfTheTabThatIsShown => ((TabWithAMarkdownView)Tabs.SelectedItem!).View;

   void QuoteWhatIsSelected(object? sender, RoutedEventArgs e)
   {
      if(!ViewOfTheTabThatIsShown.IsAnythingSelected) return;

      Editor.Quote(ViewOfTheTabThatIsShown.SelectedMarkdown);
      Editor.FocusTheView();
   }

   void ChangeTheTheme(object? sender, RoutedEventArgs e) => Application.Current!.RequestedThemeVariant = IsDark.IsChecked == true ? ThemeVariant.Dark : ThemeVariant.Light;

   void TellOfTheClickedLink(object? sender, LinkClickedEventArgs click)
   {
      ClickedLink.Text = $"Clicked a link to {click.Target}{WithWhatKeys()}, in {InWhat()}";

      string WithWhatKeys() => click.KeyModifiers == KeyModifiers.None ? "" : $" with {click.KeyModifiers}";
      string InWhat() => click.Source is MarkdownView ? "a view" : "the editor";
   }
}
