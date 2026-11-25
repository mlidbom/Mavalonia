using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace Mavalonia.MarkdownEditing.Controls;

///<summary>A control in which a person edits a markdown document.</summary>
public partial class MarkdownEditor : UserControl
{
   //wip: a plain text box stands in for the editor until how it holds the document is decided.
   public static readonly StyledProperty<string> MarkdownProperty =
      AvaloniaProperty.Register<MarkdownEditor, string>(nameof(Markdown), defaultValue: "", defaultBindingMode: BindingMode.TwoWay);

   public MarkdownEditor() => InitializeComponent();

   ///<summary>The markdown text of the document being edited.</summary>
   public string Markdown
   {
      get => GetValue(MarkdownProperty);
      set => SetValue(MarkdownProperty, value);
   }
}
