using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Puts the text of a block together, from its first character to its last.</summary>
class RenderedTextBuilder
{
   readonly string _markdown;
   readonly BlockContainers _containers;
   readonly List<RenderedCharacter> _characters = [];
   readonly List<InlineMarkup> _markups = [];

   internal RenderedTextBuilder(string markdown, BlockContainers containers)
   {
      _markdown = markdown;
      _containers = containers;
   }

   internal bool IsEmpty => _characters.Count == 0;

   ///<summary>The text. There must be at least one character in it.</summary>
   internal RenderedText Text => new(_characters, _markups, new SourceRange(Start, End));

   int Start => _markups.Where(it => it.FirstIndex == 0).Select(it => it.Source.Start).Append(_characters[0].Source.Start).Min();

   ///<summary>Where the source of the text ends, with the markup that ends there.</summary>
   internal int End => _markups.Where(it => it.EndIndex == _characters.Count).Select(it => it.Source.End).Append(_characters[^1].Source.End).Max();

   internal void Add(char character, SourceRange source, TextFormatting formatting) => _characters.Add(new RenderedCharacter(character, source, formatting));

   ///<summary>Adds the line break that is at <paramref name="start"/>. What precedes the text on the next line belongs to it.</summary>
   internal void AddLineBreak(int start, TextFormatting formatting) => Add('\n', new SourceRange(start, _containers.StartOfTextOnTheLineAfter(_markdown, start)), formatting);

   ///<summary>Adds the markdown in <paramref name="range"/> as it is written.</summary>
   internal void AddSource(SourceRange range, TextFormatting formatting)
   {
      for(var offset = range.Start; offset < range.End; offset = _characters[^1].Source.End)
      {
         if(offset == _markdown.EndOfLineAt(offset))
            AddLineBreak(offset, formatting);
         else
            Add(_markdown[offset], new SourceRange(offset, offset + 1), formatting);
      }
   }

   ///<summary>Adds the spaces from <paramref name="offset"/> to the end of the line, when there is nothing else. Markdown ignores them, but a person who has just typed one has to see it.</summary>
   internal void AddSpacesBeforeTheEndOfTheLine(int offset)
   {
      var end = _markdown.SkipSpaces(offset);
      if(end == _markdown.EndOfLineAt(end)) AddSource(new SourceRange(offset, end), TextFormatting.None);
   }

   ///<summary>Where the spaces before <paramref name="offset"/> start that haven't been added.</summary>
   internal int StartOfTheSpacesBefore(int offset)
   {
      var endOfWhatIsAdded = IsEmpty ? offset : _characters[^1].Source.End;
      while(offset > endOfWhatIsAdded && _markdown[offset - 1] is ' ' or '\t') offset--;
      return offset;
   }

   ///<summary>Adds the characters that <paramref name="addContent"/> does, as formatted by the markup in <paramref name="range"/>.</summary>
   internal void AddMarkup(SourceRange range, SourceRange content, TextFormatting itsFormatting, Action addContent)
   {
      var firstIndex = _characters.Count;
      addContent();
      _markups.Add(new InlineMarkup(range, content, firstIndex, _characters.Count, itsFormatting));
   }
}
