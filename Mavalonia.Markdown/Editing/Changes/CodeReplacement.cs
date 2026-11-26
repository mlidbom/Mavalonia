using Mavalonia.Markdown.Editing.Writing;
using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Editing.Changes;

///<summary>Replaces a part of the code in a block with other code.</summary>
///<remarks>
/// A blank line of code in a list item usually lacks the indentation that it must have once there is code on it.
/// </remarks>
class CodeReplacement
{
   readonly RenderedMarkdown _rendered;
   readonly CodeBlock _block;
   readonly SourceRange _replaced;
   readonly string _code;

   internal CodeReplacement(RenderedMarkdown rendered, CodeBlock block, SourceRange replaced, string code)
   {
      _rendered = rendered;
      _block = block;
      _replaced = replaced;
      _code = code;
   }

   internal SourceChange Change()
   {
      if(!IsThereCodeOnTheLineAfterwards) return SourceChange.Remove(_rendered, _replaced);
      if(_block.HasNoLine) return SourceChange.Replace(_rendered, _replaced, new LineBreaksInside(_rendered, _block.Containers).NewLine + _code);

      return IsThePrefixOfTheLineMissing
                ? SourceChange.Replace(_rendered, _replaced with { Start = StartOfLine }, Prefix + _code)
                : SourceChange.Replace(_rendered, _replaced, _code);
   }

   string Prefix => _block.Containers.PrefixOfTheirLines;
   int StartOfLine => _rendered.Source.StartOfLineAt(_replaced.Start);

   bool IsThereCodeOnTheLineAfterwards => _code.Length > 0 || _replaced.End < _rendered.Source.EndOfLineAt(_replaced.End);

   bool IsThePrefixOfTheLineMissing => _replaced.Start - StartOfLine < Prefix.Length && _rendered.Source.AsSpan(StartOfLine.._replaced.Start).IsWhiteSpaceWithoutLineBreak();
}
