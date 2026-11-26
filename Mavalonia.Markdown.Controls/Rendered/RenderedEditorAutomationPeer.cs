using Avalonia.Automation.Peers;

namespace Mavalonia.Markdown.Controls.Rendered;

///<summary>Tells programs such as screen readers what is edited in a <see cref="RenderedEditorView"/>, and lets those that write for a person, such as to dictate, do that. What they read and write is the markdown.</summary>
class RenderedEditorAutomationPeer : RenderedMarkdownAutomationPeer
{
   internal RenderedEditorAutomationPeer(RenderedEditorView owner) : base(owner) {}

   RenderedEditorView EditorView => (RenderedEditorView)View;

   protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Edit;
   protected override string GetNameCore() => EditorView.Watermark ?? "";

   public override bool IsReadOnly => EditorView.Editor is null;
   public override string? Value => EditorView.Editor?.Document.Text;

   public override void SetValue(string? value)
   {
      if(EditorView.Editor is {} editor) editor.Document.Text = value ?? "";
   }
}
