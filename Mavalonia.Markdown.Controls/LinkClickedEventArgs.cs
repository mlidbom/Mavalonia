using Avalonia.Input;
using Avalonia.Interactivity;

namespace Mavalonia.Markdown.Controls;

///<summary>Tells that a person has clicked a link.</summary>
public class LinkClickedEventArgs : RoutedEventArgs
{
   internal LinkClickedEventArgs(string target, KeyModifiers keyModifiers) : base(MarkdownView.LinkClickedEvent)
   {
      Target = target;
      KeyModifiers = keyModifiers;
   }

   ///<summary>Where the link leads, as the markdown says it, or as <see cref="MarkdownView.Links"/> does. It may be anything, such as a path, or something that isn't safe to open.</summary>
   public string Target { get; }

   ///<summary>The modifier keys that were pressed, such as Ctrl.</summary>
   public KeyModifiers KeyModifiers { get; }
}
