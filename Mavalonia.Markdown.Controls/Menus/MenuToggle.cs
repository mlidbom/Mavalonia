using Avalonia.Controls;
using Avalonia.Input;

namespace Mavalonia.Markdown.Controls.Menus;

///<summary>Something in a menu that is on or off, such as a format of text: the menu shows a check mark beside it when it's on.</summary>
class MenuToggle : MenuCommand
{
   ///<param name="header">What the menu calls it.</param>
   ///<param name="toggle">What choosing it does: turns it on, or off when it is on.</param>
   ///<param name="gesture">The keys that do the same, which the menu tells. Null when none do.</param>
   ///<param name="isOn">Whether it is on when the menu opens.</param>
   internal MenuToggle(string header, Action toggle, KeyGesture? gesture, bool isOn) : base(header, toggle, gesture)
   {
      ToggleType = MenuItemToggleType.CheckBox;
      IsChecked = isOn;
   }
}
