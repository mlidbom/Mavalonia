using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Rendering.Blocks;

///<summary>The first line of an <see cref="Alert"/>, which says what kind it is.</summary>
public class AlertTitle : RenderedBlock
{
   internal AlertTitle(BlockContainers containers, RenderedText text, SourceRange source, Alert alert) : base(containers, text, source) => Alert = alert;

   public Alert Alert { get; }

   internal override bool IsTheSameKindAs(RenderedBlock other) => other is AlertTitle title && title.Alert.Kind == Alert.Kind;
}
