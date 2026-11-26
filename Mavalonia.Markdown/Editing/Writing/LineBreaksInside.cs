using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Containers;

namespace Mavalonia.Markdown.Editing.Writing;

///<summary>The line breaks to write inside containers: after each comes what the line after it must start with to be inside them.</summary>
class LineBreaksInside
{
   readonly string _lineBreak;
   readonly string _prefix;

   internal LineBreaksInside(RenderedMarkdown rendered, BlockContainers containers)
   {
      _lineBreak = rendered.Source.LineBreak;
      _prefix = containers.PrefixOfTheirLines;
   }

   ///<summary>A line break, with a line after it that text can be written on.</summary>
   internal string NewLine => _lineBreak + _prefix;

   ///<summary>A line break, with a line after it that stays blank.</summary>
   internal string BlankLine => _lineBreak + _prefix.TrimEnd();

   ///<summary>The lines, with what each after the first must start with to be inside the containers.</summary>
   internal string Between(IEnumerable<string> lines) => string.Concat(lines.Select((line, number) => number == 0 ? line : line.Length == 0 ? BlankLine : NewLine + line));

   ///<summary>What separates a paragraph from one that is written after it.</summary>
   internal string BetweenParagraphs => BlankLine + NewLine;
}
