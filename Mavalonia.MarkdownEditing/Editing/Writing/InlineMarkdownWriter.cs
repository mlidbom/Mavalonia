using System.Text;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Editing.Writing;

///<summary>Writes a <see cref="FormattedText"/> as markdown.</summary>
partial class InlineMarkdownWriter
{
   static readonly TextFormats[] FormatsWithDelimiters = [TextFormats.Bold, TextFormats.Italic, TextFormats.Strikethrough];

   readonly IReadOnlyList<FormattedCharacter> _characters;
   readonly string _italicDelimiter;
   readonly StringBuilder _markdown = new();

   ///<summary>The markup that is open where we are writing, the outermost first.</summary>
   readonly List<OpenMarkup> _open = [];

   InlineMarkdownWriter(FormattedText text, string italicDelimiter)
   {
      _characters = WithFormattingOnWhiteSpaceOnlyBetweenCharactersThatHaveIt(text.Characters);
      _italicDelimiter = italicDelimiter;
   }

   ///<summary>The ways to write <paramref name="text"/>, the preferred first. Next to other markup, one may be read as intended where another isn't.</summary>
   internal static IEnumerable<string> WaysToWrite(FormattedText text) =>
      text.Characters.Any(it => it.Formatting.Has(TextFormats.Italic)) ? [Write(text, "*"), Write(text, "_")] : [Write(text, "*")];

   internal static string PreferredWayToWrite(FormattedText text) => WaysToWrite(text).First();

   static string Write(FormattedText text, string italicDelimiter) => new InlineMarkdownWriter(text, italicDelimiter).Write();

   ///<summary>Markup must start and end at a character that isn't white space: <c>**bold **</c> isn't bold.</summary>
   static IReadOnlyList<FormattedCharacter> WithFormattingOnWhiteSpaceOnlyBetweenCharactersThatHaveIt(IReadOnlyList<FormattedCharacter> characters)
   {
      return [..characters.Select((it, index) => it is { IsWhiteSpace: true, IsCode: false } ? FormattedAsTheCharactersAroundIt(it, index) : it)];

      FormattedCharacter FormattedAsTheCharactersAroundIt(FormattedCharacter whiteSpace, int index) =>
         FormatsWithDelimiters.Aggregate(whiteSpace, (it, format) => IsBetweenCharactersWith(format, index) ? it.With(format) : it.Without(format));

      bool IsBetweenCharactersWith(TextFormats format, int index) =>
         characters.Take(index).LastOrDefault(it => !it.IsWhiteSpace).Formatting.Has(format) && characters.Skip(index).FirstOrDefault(it => !it.IsWhiteSpace).Formatting.Has(format);
   }

   string Write()
   {
      for(var index = 0; index < _characters.Count; index++)
      {
         CloseWhatDoesNotFormat(_characters[index]);
         OpenWhatFormatsFrom(index);
         _markdown.Append(_characters[index].IsCode ? _characters[index].Value : _characters[index].Markdown);
      }

      CloseAllFrom(0);
      return _markdown.ToString();
   }

   void CloseWhatDoesNotFormat(FormattedCharacter character)
   {
      var firstToClose = _open.FindIndex(it => !it.Markup.Formats(character));
      if(firstToClose >= 0) CloseAllFrom(firstToClose);
   }

   void CloseAllFrom(int first)
   {
      for(var index = _open.Count - 1; index >= first; index--) _markdown.Append(_open[index].Closing);
      _open.RemoveRange(first, _open.Count - first);
   }

   void OpenWhatFormatsFrom(int index)
   {
      if(ToOpenAt(index).Count > 0) CloseCode(); // Nothing can be opened inside code.

      foreach(var markup in ToOpenAt(index).OrderBy(it => it.IsCode).ThenByDescending(it => LengthOf(it, index)).ThenByDescending(it => it.Link is not null)) Open(markup, index);
   }

   IReadOnlyList<Markup> ToOpenAt(int index) => [..Markup.Of(_characters[index]).Where(it => _open.All(open => open.Markup != it))];

   void CloseCode()
   {
      var openCode = _open.FindIndex(it => it.Markup.IsCode);
      if(openCode >= 0) CloseAllFrom(openCode);
   }

   void Open(Markup markup, int index)
   {
      var open = markup switch
      {
         { Link: {} link } => new OpenMarkup(markup, link.Opening, link.Closing),
         { Format: TextFormats.Bold } => new OpenMarkup(markup, "**", "**"),
         { Format: TextFormats.Italic } => new OpenMarkup(markup, _italicDelimiter, _italicDelimiter),
         { Format: TextFormats.Strikethrough } => new OpenMarkup(markup, "~~", "~~"),
         _ => OpenMarkup.OfCode(markup, new string([.._characters.Skip(index).Take(LengthOf(markup, index)).Select(it => it.Value)]))
      };

      _markdown.Append(open.Opening);
      _open.Add(open);
   }

   ///<summary>How many characters from <paramref name="index"/> that <paramref name="markup"/> formats.</summary>
   int LengthOf(Markup markup, int index) => _characters.Skip(index).TakeWhile(markup.Formats).Count();
}
