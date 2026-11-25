using Avalonia;
using Avalonia.Controls;
using Mavalonia.MarkdownEditing.Controls;

namespace Mavalonia.DesktopHost;

///<summary>A tab that shows markdown to be read, and to select and copy from.</summary>
class TabWithAMarkdownView : TabItem
{
   internal TabWithAMarkdownView(string header, string markdown)
   {
      Header = header;
      View = new MarkdownView { Markdown = markdown, Padding = new Thickness(12, 10), LoadImage = Samples.LoadImage };
      Content = new Border { Classes = { "Frame" }, Child = new ScrollViewer { Content = View } };
   }

   internal MarkdownView View { get; }

   protected override Type StyleKeyOverride => typeof(TabItem);
}
