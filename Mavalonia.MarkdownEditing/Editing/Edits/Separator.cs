using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Editing.Edits;

///<summary>What separates blocks, inside containers.</summary>
class Separator
{
   readonly string _lineBreak;
   readonly string _prefix;

   internal Separator(RenderedMarkdown rendered, BlockContainers containers)
   {
      _lineBreak = rendered.Source.LineBreak;
      _prefix = containers.PrefixOfTheirLines;
   }

   ///<summary>A line break, with the prefix that the line after it must have to be inside the containers.</summary>
   internal string NewLine => _lineBreak + _prefix;

   ///<summary>A line break, and nothing on the line after it but what is needed for it to be inside the containers.</summary>
   internal string BlankLine => _lineBreak + _prefix.TrimEnd();

   internal string OfParagraphs => BlankLine + NewLine;
}
