namespace Mavalonia.Markdown.Rendering.Containers;

///<summary>A quote that stands out as a note, a warning or the like. Its first line is such as <c>&gt; [!NOTE]</c>.</summary>
public class Alert : Quote
{
   internal Alert(AlertKind kind) => Kind = kind;

   public AlertKind Kind { get; }

   internal override bool LooksLike(BlockContainer other) => other is Alert alert && alert.Kind == Kind;
}
