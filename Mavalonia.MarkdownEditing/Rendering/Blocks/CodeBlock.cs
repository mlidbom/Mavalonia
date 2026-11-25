using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Rendering.Blocks;

///<summary>A block of code, between two fences such as <c>```</c>.</summary>
public class CodeBlock : RenderedBlock
{
   ///<summary>The fence that we write. A person may write another.</summary>
   internal const string Fence = "```";

   internal CodeBlock(BlockContainers containers, RenderedText text, SourceRange source, string language, bool hasNoLine) : base(containers, text, source)
   {
      Language = language;
      HasNoLine = hasNoLine;
   }

   ///<summary>The language that the opening fence says the code is in. Empty when it says none.</summary>
   public string Language { get; }

   ///<summary>Whether there is no line between the fences, not even an empty one, so that the first line of code has to be added before any can be written.</summary>
   internal bool HasNoLine { get; }

   internal override bool IsTheSameKindAs(RenderedBlock other) => other is CodeBlock;
}
