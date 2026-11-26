using Mavalonia.Markdown.Rendering.Containers;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Rendering.Blocks;

///<summary>A block of code: what is between two fences such as <c>```</c>, or, where markdown is only read, lines that are indented.</summary>
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
