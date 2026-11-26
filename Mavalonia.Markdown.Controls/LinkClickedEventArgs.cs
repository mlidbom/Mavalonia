using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Mavalonia.Markdown.Controls;

///<summary>Tells that a person has clicked a link.</summary>
public class LinkClickedEventArgs : RoutedEventArgs
{
   readonly Visual _clickedIn;
   readonly Point _clickedAt;

   ///<param name="target">Where the link leads.</param>
   ///<param name="keyModifiers">The modifier keys that were pressed.</param>
   ///<param name="clickedIn">What was clicked.</param>
   ///<param name="clickedAt">Where it was clicked, in <paramref name="clickedIn"/>.</param>
   internal LinkClickedEventArgs(string target, KeyModifiers keyModifiers, Visual clickedIn, Point clickedAt) : base(MarkdownView.LinkClickedEvent)
   {
      Target = target;
      KeyModifiers = keyModifiers;
      _clickedIn = clickedIn;
      _clickedAt = clickedAt;
   }

   ///<summary>The same click, to be told of anew, such as by the control that what was clicked is a part of.</summary>
   internal LinkClickedEventArgs(LinkClickedEventArgs click) : this(click.Target, click.KeyModifiers, click._clickedIn, click._clickedAt) {}

   ///<summary>Where the link leads, as the markdown says it, or as <see cref="MarkdownView.Links"/> does. It may be anything, such as a path, or something that isn't safe to open.</summary>
   public string Target { get; }

   ///<summary>The modifier keys that were pressed, such as Ctrl.</summary>
   public KeyModifiers KeyModifiers { get; }

   ///<summary>Where it was clicked, relative to <paramref name="relativeTo"/>, such as to show a menu there.</summary>
   ///<exception cref="ArgumentException"><paramref name="relativeTo"/> isn't in the window that was clicked.</exception>
   public Point GetPosition(Visual relativeTo) =>
      _clickedIn.TranslatePoint(_clickedAt, relativeTo) ?? throw new ArgumentException("It isn't in the window that was clicked.", nameof(relativeTo));
}
