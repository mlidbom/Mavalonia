using JetBrains.Annotations;

namespace Mavalonia.Markdown.Rendering.Containers;

///<summary>What an <see cref="Alert"/> is for.</summary>
public enum AlertKind
{
   Note,
   Tip,
   Important,
   Warning,
   [PublicAPI]
   Caution
}
