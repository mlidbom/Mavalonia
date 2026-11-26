using JetBrains.Annotations;

namespace Mavalonia.Markdown.Rendering.Texts;

///<summary>The box of a task, as the char that it's shown as. It stands for what it's written as: <c>[ ]</c>, or <c>[x]</c> once the task is done.</summary>
[PublicAPI]
public static class BoxOfATask
{
   public const char Empty = '☐';
   public const char Ticked = '☑';
}
