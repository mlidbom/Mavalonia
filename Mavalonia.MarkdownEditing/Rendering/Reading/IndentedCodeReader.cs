using Mavalonia.MarkdownEditing.Rendering.Texts;
using CodeBlock = Mavalonia.MarkdownEditing.Rendering.Blocks.CodeBlock;
using MarkdigCodeBlock = Markdig.Syntax.CodeBlock;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Reads a block of code that is code since its lines are indented.</summary>
class IndentedCodeReader
{
   const int ColumnsOfIndentation = 4;

   static readonly TextFormatting Formatting = new(TextFormats.Code);

   readonly string _markdown;
   readonly PlaceOfBlocks _place;

   internal IndentedCodeReader(string markdown, PlaceOfBlocks place)
   {
      _markdown = markdown;
      _place = place;
   }

   internal CodeBlock Read(MarkdigCodeBlock code)
   {
      var startsOfLines = _markdown.StartsOfLinesFrom(_markdown.StartOfLineAt(code.Span.Start)).Take(code.Lines.Count).ToList();
      var lines = startsOfLines.Select(CodeOnTheLineAt).ToList();
      var source = new SourceRange(StartOfIndentation(startsOfLines[0]), lines[^1].End);

      return new CodeBlock(_place.Containers, TextOfLines.Read(_markdown, lines, Formatting), source, language: "", hasNoLine: false);
   }

   SourceRange CodeOnTheLineAt(int startOfLine)
   {
      var endOfLine = _markdown.EndOfLineAt(startOfLine);
      return new SourceRange(Math.Min(EndOfIndentationFrom(StartOfIndentation(startOfLine)), endOfLine), endOfLine);
   }

   ///<summary>The prefixes of the containers aren't on a line that is the first of one of them, nor on a blank line.</summary>
   int StartOfIndentation(int startOfLine)
   {
      var areAllPrefixesThere = _place.Containers.OfTheLineAt(_markdown, startOfLine).Count == _place.Containers.Count;
      return areAllPrefixesThere ? _place.Containers.EndOfTheirPrefixes(_markdown, startOfLine) : Math.Min(startOfLine + _place.Column, _markdown.EndOfLineAt(startOfLine));
   }

   int EndOfIndentationFrom(int start)
   {
      var (end, column) = (start, 0);
      while(end < _markdown.Length && _markdown[end] is ' ' or '\t' && column < ColumnsOfIndentation)
      {
         column = _markdown[end] == '\t' ? ColumnsOfIndentation : column + 1;
         end++;
      }

      return end;
   }
}
