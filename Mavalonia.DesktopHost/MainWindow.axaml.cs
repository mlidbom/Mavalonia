using Avalonia.Controls;
using AvaloniaEdit.Document;

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
      Editor.Document = new TextDocument(((SampleDocument)SampleDocuments.SelectedItem!).ReadMarkdown());
}
