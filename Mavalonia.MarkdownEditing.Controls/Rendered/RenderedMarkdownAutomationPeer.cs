using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;

namespace Mavalonia.MarkdownEditing.Controls.Rendered;

///<summary>Tells programs such as screen readers what text a <see cref="MarkdownView"/> shows.</summary>
class RenderedMarkdownAutomationPeer : ControlAutomationPeer, IValueProvider
{
   internal RenderedMarkdownAutomationPeer(RenderedMarkdownView owner) : base(owner) {}

   protected RenderedMarkdownView View => (RenderedMarkdownView)Owner;

   protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Document;
   protected override string GetNameCore() => base.GetNameCore() is { Length: > 0 } name ? name : Text;
   protected override bool IsContentElementCore() => true;
   protected override bool IsControlElementCore() => true;

   string Text => View.Selectable?.Rendered.PlainText.Value ?? "";

   public virtual bool IsReadOnly => true;
   public virtual string? Value => Text;
   public virtual void SetValue(string? value) => throw new InvalidOperationException("It's only read.");
}
