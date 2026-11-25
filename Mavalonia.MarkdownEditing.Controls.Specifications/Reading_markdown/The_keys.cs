using Avalonia.Input;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.Reading_markdown;

public class The_keys
{
   const RawInputModifiers Ctrl = RawInputModifiers.Control;

   [XFOnUIThread] public void that_select_nothing_are_left_to_others()
   {
      var view = new ViewInAWindow("one two");
      var left = new List<Key>();
      view.ScrollViewer.KeyDown += (_, press) => left.Add(press.Key);
      view.Window.Click(view.CenterOf("one"));
      view.Press(PhysicalKey.ArrowDown).Press(PhysicalKey.Escape).Press(PhysicalKey.C, Ctrl);
      left.Must().Satisfy(it => it.Contains(Key.Down) && it.Contains(Key.Escape) && it.Contains(Key.C));
   }
}
