using Avalonia;
using Avalonia.Controls;
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

      var document = new TextDocument(Samples.WhatTheEditorStartsWith);
      document.TextChanged += (_, _) => Preview.Markdown = document.Text;
      Editor.Document = document;
      Preview.Markdown = document.Text;
      Preview.LoadImage = Samples.LoadImage;

      foreach(var sample in Samples.ForTabs) Tabs.Items.Add(TabOf(sample));
      AddHandler(MarkdownView.LinkClickedEvent, TellOfTheClickedLink); // Of the editor and of all the views.
   }

   static TabItem TabOf(SampleDocument sample) => new()
   {
      Header = sample.Name,
      Content = new Border
      {
         Classes = { "Frame" },
         Child = new ScrollViewer { Content = new MarkdownView { Markdown = sample.Markdown, Padding = new Thickness(12, 10), LoadImage = Samples.LoadImage } }
      }
   };

   MarkdownView ViewOfTheTabThatIsShown => (MarkdownView)((ScrollViewer)((Border)Tabs.SelectedContent!).Child!).Content!;

   void QuoteWhatIsSelected(object? sender, RoutedEventArgs e)
   {
      if(!ViewOfTheTabThatIsShown.IsAnythingSelected) return;

      Editor.Quote(ViewOfTheTabThatIsShown.SelectedMarkdown);
      Editor.FocusTheView();
   }

   void ChangeTheTheme(object? sender, RoutedEventArgs e) => Application.Current!.RequestedThemeVariant = IsDark.IsChecked == true ? ThemeVariant.Dark : ThemeVariant.Light;

   void TellOfTheClickedLink(object? sender, LinkClickedEventArgs click) =>
      ClickedLink.Text = $"Clicked a link to {click.Target}{(click.KeyModifiers == default ? "" : $" with {click.KeyModifiers}")}, in {(click.Source is MarkdownView ? "a view" : "the editor")}";
}
