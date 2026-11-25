using Avalonia.Controls;
using Avalonia.Input;

namespace Mavalonia.MarkdownEditing.Controls.Menus;

///<summary>Something that a person can choose in a menu.</summary>
class MenuCommand : MenuItem
{
   ///<param name="header">What the menu calls it.</param>
   ///<param name="action">What choosing it does.</param>
   ///<param name="gesture">The keys that do the same, which the menu tells. Null when none do.</param>
   internal MenuCommand(string header, Func<Task> action, KeyGesture? gesture = null)
   {
      Header = header;
      InputGesture = gesture;
      Click += (_, _) => action().SurfaceExceptions();
   }

   protected override Type StyleKeyOverride => typeof(MenuItem);
}
