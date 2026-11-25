using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using AvaloniaEdit.Document;
using Mavalonia.MarkdownEditing.Controls;

namespace Mavalonia.DesktopHost;

///<summary>Has an editor, and beside it what is written there as it is to be read.</summary>
partial class MainWindow : Window
{
   public MainWindow()
   {
      InitializeComponent();
      SampleDocuments.ItemsSource = SampleDocument.All();
      SampleDocuments.SelectedIndex = 0;
   }

   void OpenSelectedSampleDocument(object? sender, SelectionChangedEventArgs selectionChange)
   {
      var document = new TextDocument(((SampleDocument)SampleDocuments.SelectedItem!).ReadMarkdown());
      document.TextChanged += (_, _) => View.Markdown = document.Text;

      Editor.Document = document;
      View.Markdown = document.Text;
   }

   void QuoteWhatIsSelected(object? sender, RoutedEventArgs e)
   {
      if(!View.IsAnythingSelected) return;

      Editor.Quote(View.SelectedMarkdown);
      Editor.FocusTheView();
   }

   void ChangeTheTheme(object? sender, RoutedEventArgs e) => Application.Current!.RequestedThemeVariant = IsDark.IsChecked == true ? ThemeVariant.Dark : ThemeVariant.Light;

   void TellOfTheClickedLink(object? sender, LinkClickedEventArgs click) => ClickedLink.Text = click.Target;
}
