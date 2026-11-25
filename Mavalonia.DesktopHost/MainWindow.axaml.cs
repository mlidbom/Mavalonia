using Avalonia.Controls;

namespace Mavalonia.DesktopHost;

partial class MainWindow : Window
{
   public MainWindow()
   {
      InitializeComponent();
      SampleDocuments.ItemsSource = SampleDocument.All();
      SampleDocuments.SelectedIndex = 0;
   }

   void OpenSelectedSampleDocument(object? sender, SelectionChangedEventArgs selectionChange) =>
      Editor.Markdown = ((SampleDocument)SampleDocuments.SelectedItem!).ReadMarkdown();
}
