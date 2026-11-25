using System.Globalization;

namespace Mavalonia.MarkdownEditing.Rendering.Texts;

///<summary>The text that is shown of a block, each character knowing the markdown it stands for.</summary>
public class RenderedText
{
   internal RenderedText(IReadOnlyList<RenderedCharacter> characters, IReadOnlyList<InlineMarkup> markups, SourceRange source)
   {
      Characters = characters;
      Markups = markups;
      Source = source;
      Value = new string([..characters.Select(it => it.Value)]);
   }

   internal static RenderedText EmptyAt(int offset) => new([], [], SourceRange.EmptyAt(offset));

   public IReadOnlyList<RenderedCharacter> Characters { get; }

   internal IReadOnlyList<InlineMarkup> Markups { get; }

   ///<summary>The markdown that the text stands for. It starts before the first character's own markup, such as the <c>**</c> of a bold word, and ends after the last one's.</summary>
   public SourceRange Source { get; }

   ///<summary>The characters as a string.</summary>
   public string Value { get; }

   public int Length => Characters.Count;
   public bool IsEmpty => Length == 0;

   ///<summary>The offset at which text typed before the character at <paramref name="index"/> goes. It continues the character before that one, so after a bold character it is inside the bold markup.</summary>
   public int OffsetAt(int index) => index == 0 ? Source.Start : Characters[index - 1].Source.End;

   ///<summary>The index of the first character that is not before <paramref name="offset"/>.</summary>
   public int IndexAt(int offset) => Characters.TakeWhile(it => it.Source.End <= offset).Count();

   ///<summary>The formatting that text typed at <paramref name="offset"/> gets from the markup it is inside.</summary>
   public TextFormatting FormattingAt(int offset) =>
      Markups.Where(it => it.Content.Touches(offset))
             .Select(it => it.Formatting)
             .Aggregate(TextFormatting.None, (all, it) => new TextFormatting(all.Formats | it.Formats, it.Link ?? all.Link));

   ///<summary>The index of the first <see cref="char"/> of the character before <paramref name="index"/>. A person sees an emoji as one character, however many chars it is.</summary>
   public int StartOfCharacterBefore(int index) => StringInfo.ParseCombiningCharacters(Value[..index]).LastOrDefault();

   ///<summary>The index after the last <see cref="char"/> of the character at <paramref name="index"/>.</summary>
   public int EndOfCharacterAfter(int index) => index + StringInfo.GetNextTextElementLength(Value.AsSpan(index));

   ///<summary>The index that the word starts at that is before <paramref name="index"/>, or, where there is no word, the punctuation.</summary>
   public int StartOfWordBefore(int index)
   {
      var end = SkipWhiteSpaceBefore(index);
      var start = end;
      while(start > 0 && KindOf(start - 1) == KindOf(end - 1)) start--;
      return start;
   }

   ///<summary>The index after the white space after the word that is at <paramref name="index"/>, or, where there is no word, the punctuation.</summary>
   public int EndOfWordAfter(int index)
   {
      var end = index;
      while(end < Length && KindOf(end) == KindOf(index)) end++;
      return SkipWhiteSpaceAfter(end);
   }

   KindOfCharacter KindOf(int index) => Value[index] switch
   {
      var it when char.IsWhiteSpace(it) => KindOfCharacter.WhiteSpace,
      var it when char.IsLetterOrDigit(it) || char.IsSurrogate(it) || it == '_' => KindOfCharacter.OfAWord,
      _ => KindOfCharacter.Punctuation
   };

   enum KindOfCharacter
   {
      WhiteSpace = 1,
      OfAWord = 2,
      Punctuation = 3
   }

   internal int SkipWhiteSpaceBefore(int index)
   {
      while(index > 0 && Characters[index - 1].IsWhiteSpace) index--;
      return index;
   }

   internal int SkipWhiteSpaceAfter(int index)
   {
      while(index < Length && Characters[index].IsWhiteSpace) index++;
      return index;
   }

   internal int SkipLineBreaksBefore(int index)
   {
      while(index > 0 && Characters[index - 1].IsLineBreak) index--;
      return index;
   }

   ///<summary>Where the source of the characters from <paramref name="index"/> starts, with the markup that starts there.</summary>
   internal int StartOfSourceFrom(int index) =>
      index == Length ? Source.End : Markups.Where(it => it.FirstIndex == index && it.EndIndex > index).Select(it => it.Source.Start).Append(Characters[index].Source.Start).Min();

   ///<summary>Where the source of the characters up to <paramref name="index"/> ends, with the markup that ends there.</summary>
   internal int EndOfSourceUpTo(int index) =>
      index == 0 ? Source.Start : Markups.Where(it => it.EndIndex == index && it.FirstIndex < index).Select(it => it.Source.End).Append(Characters[index - 1].Source.End).Max();

   ///<summary>The index that the outermost markup starts at that formats the character before <paramref name="index"/>. That index, when none does.</summary>
   internal int StartOfMarkupThatTouches(int index) => Markups.Where(it => it.FirstIndex < index && it.EndIndex >= index).Select(it => it.FirstIndex).Append(index).Min();

   ///<summary>The index that the outermost markup ends at that formats the character at <paramref name="index"/>. That index, when none does.</summary>
   internal int EndOfMarkupThatTouches(int index) => Markups.Where(it => it.FirstIndex <= index && it.EndIndex > index).Select(it => it.EndIndex).Append(index).Max();
}
