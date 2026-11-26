using System.Globalization;

namespace Mavalonia.Markdown.Rendering.Texts;

///<summary>Where the characters and words of a text start and end, as a person sees them. They see an emoji as one character, however many <see cref="char"/>s it is.</summary>
public partial class TextBoundaries
{
   readonly string _text;

   internal TextBoundaries(string text) => _text = text;

   ///<summary>The index of the first <see cref="char"/> of the character before <paramref name="index"/>.</summary>
   public int StartOfCharacterBefore(int index) => StringInfo.ParseCombiningCharacters(_text[..index]).LastOrDefault();

   ///<summary>The index after the last <see cref="char"/> of the character at <paramref name="index"/>.</summary>
   public int EndOfCharacterAfter(int index) => index + StringInfo.GetNextTextElementLength(_text.AsSpan(index));

   ///<summary>The index that the word starts at that is before <paramref name="index"/>, or, where there is no word, the punctuation.</summary>
   public int StartOfWordBefore(int index)
   {
      var end = index;
      while(end > 0 && char.IsWhiteSpace(_text[end - 1])) end--;

      var start = end;
      while(start > 0 && KindOf(start - 1) == KindOf(end - 1)) start--;
      return start;
   }

   ///<summary>The index after the white space after the word that is at <paramref name="index"/>, or, where there is no word, the punctuation.</summary>
   internal int EndOfWordAfter(int index)
   {
      var end = EndOfWordWithoutSpacesAfter(index);
      while(end < _text.Length && char.IsWhiteSpace(_text[end])) end++;
      return end;
   }

   ///<summary>The index after the word that is at <paramref name="index"/>, or, where there is no word, the punctuation or the white space.</summary>
   public int EndOfWordWithoutSpacesAfter(int index)
   {
      var end = index;
      while(end < _text.Length && KindOf(end) == KindOf(index)) end++;
      return end;
   }

   KindOfCharacter KindOf(int index) => _text[index] switch
   {
      var it when char.IsWhiteSpace(it) => KindOfCharacter.WhiteSpace,
      var it when char.IsLetterOrDigit(it) || char.IsSurrogate(it) || it == '_' => KindOfCharacter.OfAWord,
      _ => KindOfCharacter.Punctuation
   };
}
