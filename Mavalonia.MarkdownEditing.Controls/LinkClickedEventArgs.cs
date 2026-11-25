using Avalonia.Interactivity;

namespace Mavalonia.MarkdownEditing.Controls;

///<summary>Tells that a person has clicked a link.</summary>
public class LinkClickedEventArgs : RoutedEventArgs
{
   internal LinkClickedEventArgs(string target) : base(MarkdownView.LinkClickedEvent) => Target = target;

   ///<summary>Where the link leads, as the markdown says it. It may be anything, such as a path, or something that isn't safe to open.</summary>
   public string Target { get; }
}
