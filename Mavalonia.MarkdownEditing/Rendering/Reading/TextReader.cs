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
   readonly int _shift;
   readonly RenderedTextBuilder _text;

   ///<param name="markdown">All of the markdown that the block is in.</param>
   ///<param name="containers">The containers that the block is inside.</param>
   ///<param name="shift">How much further on everything is than Markdig says.</param>
   internal TextReader(string markdown, BlockContainers containers, int shift = 0)
   {
      _markdown = markdown;
      _containers = containers;
      _shift = shift;
      _text = new RenderedTextBuilder(markdown, containers);
   }

   ///<summary>The text of <paramref name="inlines"/>, or null when there is none.</summary>
   internal RenderedText? Read(ContainerInline? inlines, bool includeTrailingSpaces = true)
   {
      if(inlines is not null) ReadAll(inlines, TextFormatting.None);
      if(_text.IsEmpty) return null;

      if(includeTrailingSpaces) _text.AddSpacesBeforeTheEndOfTheLine(_text.End);
      return _text.Text;
   }

   ///<summary>The markdown in <paramref name="range"/>, as ordinary text.</summary>
   internal RenderedText ReadAsText(SourceRange range)
   {
      _text.AddSource(range, TextFormatting.None);
      return _text.Text;
   }

   ///<summary>The markdown in <paramref name="range"/>, shown as the source it is.</summary>
   internal RenderedText ReadAsSource(SourceRange range)
   {
      _text.AddSource(range, TextFormatting.None with { IsShownAsSource = true });
      return _text.Text;
   }

   SourceRange RangeOf(MarkdownObject it) => new(it.Span.Start + _shift, it.Span.End + 1 + _shift);

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
         case LinkInline { IsImage: false, FirstChild: not null } link: ReadLink(link, formatting); break;
         case AutolinkInline link: ReadAutolink(link, formatting); break;
         case HtmlEntityInline entity: ReadEntity(entity, formatting); break;
         case LineBreakInline lineBreak: ReadLineBreak(lineBreak, formatting); break;
         case DelimiterInline delimiter: ReadDelimiterThatDelimitsNothing(delimiter, formatting); break;
         case ContainerInline container and not LinkInline: ReadAll(container, formatting); break;
         default: _text.AddSource(RangeOf(inline), formatting with { IsShownAsSource = true }); break;
      }
   }

   void ReadLiteral(LiteralInline literal, TextFormatting formatting)
   {
      var range = RangeOf(literal);
      var text = literal.Content.ToString();
      if(literal.IsFirstCharacterEscaped)
      {
         var escaped = range with { End = range.Start + 2 };
         _text.Add(text[0], escaped, formatting);
         (range, text) = (range with { Start = escaped.End }, text[1..]);
      }

      ReadCharactersAlong(range, text, formatting);
   }

   ///<summary>Such as a <c>[</c> that no <c>]</c> follows.</summary>
   void ReadDelimiterThatDelimitsNothing(DelimiterInline delimiter, TextFormatting formatting)
   {
      ReadCharactersAlong(RangeOf(delimiter), delimiter.ToLiteral(), formatting);
      ReadAll(delimiter, formatting);
   }

   void ReadEmphasis(EmphasisInline emphasis, TextFormatting formatting)
   {
      var format = emphasis.DelimiterChar == '~' ? TextFormats.Strikethrough : emphasis.DelimiterCount == 2 ? TextFormats.Bold : TextFormats.Italic;
      var range = RangeOf(emphasis);
      _text.AddMarkup(range, range.BetweenDelimiters(ofLength: emphasis.DelimiterCount), new TextFormatting(format), () => ReadAll(emphasis, formatting.With(format)));
   }

   void ReadCode(CodeInline code, TextFormatting formatting)
   {
      var range = RangeOf(code);
      var content = range.BetweenDelimiters(ofLength: code.DelimiterCount);
      _text.AddMarkup(range, content, new TextFormatting(TextFormats.Code), () => ReadCharactersAlong(content, code.Content, formatting.With(TextFormats.Code)));
   }

   void ReadLink(LinkInline markdigLink, TextFormatting formatting)
   {
      var range = RangeOf(markdigLink);
      var content = new SourceRange(RangeOf(markdigLink.FirstChild!).Start, RangeOf(markdigLink.LastChild!).End);
      var link = new Link(markdigLink.GetDynamicUrl?.Invoke() ?? markdigLink.Url ?? "", opening: _markdown[range.Start..content.Start], closing: _markdown[content.End..range.End]);

      _text.AddMarkup(range, content, new TextFormatting(TextFormats.None, link), () => ReadAll(markdigLink, formatting with { Link = link }));
   }

   void ReadAutolink(AutolinkInline autolink, TextFormatting formatting)
   {
      var range = RangeOf(autolink);
      var content = range.BetweenDelimiters(ofLength: 1);
      var target = autolink.IsEmail ? "mailto:" + autolink.Url : autolink.Url;
      var link = new Link(target, opening: "[", closing: $"]({target})"); // Written anew with another label, it has to be an ordinary link.

      _text.AddMarkup(range, content, new TextFormatting(TextFormats.None, link), () => ReadCharactersAlong(content, autolink.Url, formatting with { Link = link }));
   }

   void ReadEntity(HtmlEntityInline entity, TextFormatting formatting)
   {
      foreach(var character in entity.Transcoded.ToString()) _text.Add(character, RangeOf(entity), formatting);
   }

   void ReadLineBreak(LineBreakInline lineBreak, TextFormatting formatting)
   {
      var endOfLine = _markdown.EndOfLineAt(RangeOf(lineBreak).Start);
      var startOfTheSpaces = _text.StartOfTheSpacesBefore(endOfLine); // Markdig leaves some spaces at the end of a line in the text, and others not.

      if(lineBreak.IsHard)
         _text.AddLineBreak(lineBreak.IsBackslash ? endOfLine - 1 : startOfTheSpaces, formatting);
      else
      {
         _text.AddSpacesBeforeTheEndOfTheLine(startOfTheSpaces);
         _text.AddLineBreak(endOfLine, formatting);
      }
   }

   ///<summary>Finds each character of <paramref name="text"/> in the markdown in <paramref name="range"/>. What there is between them is the backslashes of escaped characters, and, in code of several lines, the line breaks, which code shows as spaces.</summary>
   void ReadCharactersAlong(SourceRange range, string text, TextFormatting formatting)
   {
      var offset = text.StartsWith(' ') || text.StartsWith('\t') ? range.Start : _markdown.SkipSpaces(range.Start); // Markdig takes the spaces around the text of a cell away, but not from its range.
      foreach(var character in text)
      {
         var end = EndOf(character);
         _text.Add(character, new SourceRange(offset, end), formatting);
         offset = end;
      }

      int EndOf(char character)
      {
         if(!formatting.Has(TextFormats.Code)) return _markdown[offset] == character ? offset + 1 : offset + 2;

         var isALineBreak = character == ' ' && offset == _markdown.EndOfLineAt(offset);
         return isALineBreak ? _containers.StartOfTextOnTheLineAfter(_markdown, offset) : _markdown.IndexOf(character, offset) + 1;
      }
   }
}
