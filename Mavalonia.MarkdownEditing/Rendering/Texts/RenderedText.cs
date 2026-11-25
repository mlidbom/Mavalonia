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

   ///<summary>Where the characters and words that a person sees in the text start and end.</summary>
   public TextBoundaries Boundaries => new(Value);

   ///<summary>The whole text, as stretches that are as long as they can be.</summary>
   public IEnumerable<StretchOfText> StretchesOfTheSameFormatting
   {
      get
      {
         for(var start = 0; start < Length;)
         {
            var formatting = Characters[start].Formatting;
            var length = Characters.Skip(start).TakeWhile(it => it.Formatting == formatting).Count();
            yield return new StretchOfText(start, length, formatting);
            start += length;
         }
      }
   }

   ///<summary>Whether the source of each character is after that of the one before it, and within that of the text.</summary>
   internal bool IsOfCharactersInOrder =>
      Source.Start <= Source.End
   && Characters.All(it => Source.Start <= it.Source.Start && it.Source.Start < it.Source.End && it.Source.End <= Source.End)
   && Characters.Zip(Characters.Skip(1)).All(it => it.First.Source == it.Second.Source || it.First.Source.End <= it.Second.Source.Start);

   internal bool LooksLike(RenderedText other) => Value == other.Value && Characters.Zip(other.Characters).All(it => it.First.Formatting.LooksLike(it.Second.Formatting));

   ///<summary>The offset at which text typed before the character at <paramref name="index"/> goes. It continues the character before that one, so after a bold character it is inside the bold markup.</summary>
   public int OffsetAt(int index) => index == 0 ? Source.Start : Characters[index - 1].Source.End;

   ///<summary>The index of the first character that is not before <paramref name="offset"/>.</summary>
   public int IndexAt(int offset) => Characters.TakeWhile(it => it.Source.End <= offset).Count();

   ///<summary>The formatting that text typed at <paramref name="offset"/> gets from the markup it is inside.</summary>
   internal TextFormatting FormattingAt(int offset) =>
      Markups.Where(it => it.Content.Touches(offset))
             .Select(it => it.Formatting)
             .Aggregate(TextFormatting.None, (all, it) => new TextFormatting(all.Formats | it.Formats, it.Link ?? all.Link));

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
