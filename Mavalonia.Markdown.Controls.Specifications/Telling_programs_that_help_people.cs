using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Compze.Must;
using Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.Markdown.Controls.Specifications;

public class Telling_programs_that_help_people
{
   public class what_is_read : Telling_programs_that_help_people
   {
      readonly AutomationPeer _peer = ControlAutomationPeer.CreatePeerForElement(new ViewInAWindow("# a\n\n**b**").View);

      [XFOnUIThread] public void tells_that_it_is_a_document() => _peer.GetAutomationControlType().Must().Be(AutomationControlType.Document);
      [XFOnUIThread] public void tells_its_text() => _peer.GetName().Must().Be("a\nb");
      [XFOnUIThread] public void tells_that_it_cannot_be_changed() => _peer.GetProvider<IValueProvider>()!.IsReadOnly.Must().BeTrue();
   }

   public class what_is_edited : Telling_programs_that_help_people
   {
      readonly EditorInAWindow _editor = new("**b**‸");
      readonly IValueProvider _value;

      public what_is_edited() => _value = ControlAutomationPeer.CreatePeerForElement(_editor.RenderedView).GetProvider<IValueProvider>()!;

      [XFOnUIThread] public void tells_its_markdown() => _value.Value.Must().Be("**b**");

      [XFOnUIThread] public void lets_them_change_it()
      {
         _value.SetValue("c");
         _editor.Editor.Document.Text.Must().Be("c");
      }
   }
}
