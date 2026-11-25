using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Mavalonia.MarkdownEditing.Rendering.Containers;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>Reads the text of one block.</summary>
class TextReader
{
   readonly string _markdown;
   readonly BlockContainers _containers;

   internal TextReader(string markdown, BlockContainers containers)
   {
      _markdown = markdown;
      _containers = containers;
   }

   readonly List<RenderedCharacter> _characters = [];
   readonly List<InlineMarkup> _markups = [];

   ///<summary>The text of <paramref name="inlines"/>, or null when there is none.</summary>
   internal RenderedText? Read(ContainerInline? inlines, bool includeTrailingSpaces = true)
   {
      if(inlines is not null) ReadAll(inlines, TextFormatting.None);
      if(_characters.Count == 0) return null;

      if(includeTrailingSpaces) ReadSpacesBeforeTheEndOfTheLine(EndOfText);
      return Text();
   }

   ///<summary>The _markdown in <paramref name="range"/>, as ordinary text.</summary>
   internal RenderedText ReadAsText(SourceRange range)
   {
      ReadSource(range, TextFormatting.None);
      return Text();
   }

   ///<summary>The _markdown in <paramref name="range"/>, shown as the source it is.</summary>
   internal RenderedText ReadAsSource(SourceRange range)
   {
      ReadSource(range, TextFormatting.None with { IsShownAsSource = true });
      return Text();
   }

   RenderedText Text() => new(_characters, _markups, new SourceRange(StartOfText, EndOfText));

   int StartOfText => _markups.Where(it => it.FirstIndex == 0).Select(it => it.Source.Start).Append(_characters[0].Source.Start).Min();
   int EndOfText => _markups.Where(it => it.EndIndex == _characters.Count).Select(it => it.Source.End).Append(_characters[^1].Source.End).Max();

   static SourceRange RangeOf(MarkdownObject it) => new(it.Span.Start, it.Span.End + 1);

   void ReadAll(ContainerInline inlines, TextFormatting formatting)
   {
      foreach(var inline in inlines) Read(inline, formatting);
   }

   void Read(Inline inline, TextFormatting formatting)
   {
      switch(inline)
      {
         case LiteralInline literal: ReadLiteral(literal, formatting); break;
         case EmphasisInline emphasis: ReadEmphasis(emphasis, formatting); break;
         case CodeInline code: ReadCode(code, formatting); break;
         case LinkInline { IsImage: false } link: ReadLink(link, formatting); break;
         case AutolinkInline link: ReadAutolink(link, formatting); break;
         case HtmlEntityInline entity: ReadEntity(entity, formatting); break;
         case LineBreakInline lineBreak: ReadLineBreak(lineBreak, formatting); break;
         case ContainerInline container and not LinkInline: ReadAll(container, formatting); break;
         default: ReadSource(RangeOf(inline), formatting with { IsShownAsSource = true }); break;
      }
   }

   void ReadLiteral(LiteralInline literal, TextFormatting formatting)
   {
      var range = RangeOf(literal);
      if(literal.IsFirstCharacterEscaped && _markdown[range.Start] != '\\') range = range with { Start = range.Start - 1 };

      ReadCharactersAlong(range, literal.Content.ToString(), formatting);
   }

   void ReadEmphasis(EmphasisInline emphasis, TextFormatting formatting)
   {
      var format = emphasis.DelimiterChar == '~' ? TextFormats.Strikethrough : emphasis.DelimiterCount == 2 ? TextFormats.Bold : TextFormats.Italic;
      ReadMarkup(RangeOf(emphasis), emphasis.DelimiterCount, new TextFormatting(format), () => ReadAll(emphasis, formatting.With(format)));
   }

   void ReadCode(CodeInline code, TextFormatting formatting)
   {
      var range = RangeOf(code);
      var content = new SourceRange(range.Start + code.DelimiterCount, range.End - code.DelimiterCount);
      ReadMarkup(range, code.DelimiterCount, new TextFormatting(TextFormats.Code), () => ReadCharactersAlong(content, code.Content, formatting.With(TextFormats.Code)));
   }

   void ReadLink(LinkInline markdigLink, TextFormatting formatting)
   {
      var range = RangeOf(markdigLink);
      var content = markdigLink.FirstChild is {} first ? new SourceRange(first.Span.Start, markdigLink.LastChild!.Span.End + 1) : SourceRange.EmptyAt(range.Start + 1);
      var link = new Link(markdigLink.GetDynamicUrl?.Invoke() ?? markdigLink.Url ?? "", opening: _markdown[range.Start..content.Start], closing: _markdown[content.End..range.End]);

      ReadMarkup(range, content, new TextFormatting(TextFormats.None, link), () => ReadAll(markdigLink, formatting with { Link = link }));
   }

   void ReadAutolink(AutolinkInline autolink, TextFormatting formatting)
   {
      var range = RangeOf(autolink);
      var content = new SourceRange(range.Start + 1, range.End - 1);
      var target = autolink.IsEmail ? "mailto:" + autolink.Url : autolink.Url;
      var link = new Link(target, opening: "[", closing: $"]({target})"); // Written anew with another label, it has to be an ordinary link.
      ReadMarkup(range, content, new TextFormatting(TextFormats.None, link), () => ReadCharactersAlong(content, autolink.Url, formatting with { Link = link }));
   }

   void ReadMarkup(SourceRange range, int lengthOfEachDelimiter, TextFormatting itsFormatting, Action readContent) =>
      ReadMarkup(range, new SourceRange(range.Start + lengthOfEachDelimiter, range.End - lengthOfEachDelimiter), itsFormatting, readContent);

   void ReadMarkup(SourceRange range, SourceRange content, TextFormatting itsFormatting, Action readContent)
   {
      var firstIndex = _characters.Count;
      readContent();
      _markups.Add(new InlineMarkup(range, content, firstIndex, _characters.Count, itsFormatting));
   }

   void ReadEntity(HtmlEntityInline entity, TextFormatting formatting) =>
      _characters.AddRange(entity.Transcoded.ToString().Select(it => new RenderedCharacter(it, RangeOf(entity), formatting)));

   void ReadLineBreak(LineBreakInline lineBreak, TextFormatting formatting)
   {
      var endOfLine = EndOfTheLineThatEndsAt(lineBreak.Span);

      if(lineBreak.IsHard)
         ReadLineBreak(lineBreak.IsBackslash ? endOfLine - 1 : StartOfTheSpacesBefore(endOfLine), formatting);
      else
      {
         ReadSpacesBeforeTheEndOfTheLine(StartOfTheSpacesBefore(endOfLine));
         ReadLineBreak(endOfLine, formatting);
      }
   }

   int EndOfTheLineThatEndsAt(SourceSpan lineBreak)
   {
      var start = lineBreak.Start;
      return _markdown.EndOfLineAt(_markdown[start] == '\n' && _markdown[start - 1] == '\r' ? start - 1 : start);
   }

   ///<summary>Where the spaces before <paramref name="offset"/> start that haven't been read: Markdig leaves some spaces at the end of a line in the text, and others not.</summary>
   int StartOfTheSpacesBefore(int offset)
   {
      var endOfWhatIsRead = _characters[^1].Source.End;
      while(offset > endOfWhatIsRead && _markdown[offset - 1] is ' ' or '\t') offset--;
      return offset;
   }

   void ReadLineBreak(int start, TextFormatting formatting) => _characters.Add(new RenderedCharacter('\n', new SourceRange(start, StartOfTheTextOnTheLineAfter(start)), formatting));

   int StartOfTheTextOnTheLineAfter(int offset) => _markdown.SkipSpaces(_containers.EndOfTheirPrefixes(_markdown, _markdown.StartOfLineAfter(offset) ?? _markdown.Length));

   ///<summary>Markdown ignores spaces at the end of a line, but a person who has just typed one has to see it.</summary>
   void ReadSpacesBeforeTheEndOfTheLine(int offset)
   {
      var end = _markdown.SkipSpaces(offset);
      if(end == _markdown.EndOfLineAt(end)) ReadSource(new SourceRange(offset, end), TextFormatting.None);
   }

   void ReadSource(SourceRange range, TextFormatting formatting)
   {
      for(var offset = range.Start; offset < range.End; offset = _characters[^1].Source.End)
      {
         if(offset == _markdown.EndOfLineAt(offset))
            ReadLineBreak(offset, formatting);
         else
            _characters.Add(new RenderedCharacter(_markdown[offset], new SourceRange(offset, offset + 1), formatting));
      }
   }

   ///<summary>Finds each character of <paramref name="text"/> in the _markdown in <paramref name="range"/>. What there is between them is the backslashes of escaped characters, and, in code of several lines, the line breaks, which code shows as spaces.</summary>
   void ReadCharactersAlong(SourceRange range, string text, TextFormatting formatting)
   {
      var offset = text.StartsWith(' ') || text.StartsWith('\t') ? range.Start : _markdown.SkipSpaces(range.Start); // Markdig takes the spaces around the text of a cell away, but not from its range.
      foreach(var character in text)
      {
         var end = EndOf(character);
         _characters.Add(new RenderedCharacter(character, new SourceRange(offset, end), formatting));
         offset = end;
      }

      int EndOf(char character)
      {
         if(!formatting.Has(TextFormats.Code)) return _markdown[offset] == character ? offset + 1 : offset + 2;

         var isALineBreak = character == ' ' && offset == _markdown.EndOfLineAt(offset);
         return isALineBreak ? StartOfTheTextOnTheLineAfter(offset) : _markdown.IndexOf(character, offset) + 1;
      }
   }
}
