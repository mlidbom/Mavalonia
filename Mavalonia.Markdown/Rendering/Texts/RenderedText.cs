using JetBrains.Annotations;

namespace Mavalonia.Markdown.Rendering.Texts;

///<summary>The text that is shown of a block, each char knowing the markdown it stands for.</summary>
public class RenderedText
{
   internal RenderedText(IReadOnlyList<RenderedChar> chars, IReadOnlyList<InlineMarkup> markups, SourceRange source)
   {
      Chars = chars;
      Markups = markups;
      Source = source;
      Value = new string([..chars.Select(it => it.Value)]);
   }

   internal static RenderedText EmptyAt(int offset) => new([], [], SourceRange.EmptyAt(offset));

   public IReadOnlyList<RenderedChar> Chars { get; }

   internal IReadOnlyList<InlineMarkup> Markups { get; }

   ///<summary>The markdown that the text stands for. It starts before the first char's own markup, such as the <c>**</c> of a bold word, and ends after the last one's.</summary>
   public SourceRange Source { get; }

   ///<summary>The chars as a string.</summary>
   public string Value { get; }

   public int Length => Chars.Count;
   [PublicAPI]
   public bool IsEmpty => Length == 0;

   ///<summary>The link that is written at <paramref name="offset"/>: its label, or the markup around that. Null when none is.</summary>
   internal Link? LinkWrittenAt(int offset) => Markups.Where(it => it.Source.Contains(offset)).Select(it => it.Formatting.Link).FirstOrDefault(it => it is not null);

   ///<summary>The box stands for what it's written as, such as <c>[x]</c>.</summary>
   internal bool StartsWithTheBoxOfATask => !IsEmpty && Chars[0] is { Value: BoxOfATask.Empty or BoxOfATask.Ticked, Source.Length: > 1 };

   ///<summary>Where the characters and words that a person sees in the text start and end.</summary>
   public TextBoundaries Boundaries => new(Value);

   ///<summary>The whole text, as stretches that are as long as they can be.</summary>
   public IEnumerable<StretchOfText> StretchesOfTheSameFormatting
   {
      get
      {
         for(var start = 0; start < Length;)
         {
            var formatting = Chars[start].Formatting;
            var length = Chars.Skip(start).TakeWhile(it => it.Formatting == formatting).Count();
            yield return new StretchOfText(start, length, formatting);
            start += length;
         }
      }
   }

   ///<summary>Whether the source of each char is after that of the one before it, and within that of the text.</summary>
   internal bool IsOfCharsInOrder =>
      Source.Start <= Source.End
   && Chars.All(it => Source.Start <= it.Source.Start && it.Source.Start < it.Source.End && it.Source.End <= Source.End)
   && Chars.Zip(Chars.Skip(1)).All(it => it.First.Source == it.Second.Source || it.First.Source.End <= it.Second.Source.Start);

   internal bool LooksLike(RenderedText other) => Value == other.Value && Chars.Zip(other.Chars).All(it => it.First.Formatting.LooksLike(it.Second.Formatting));

   ///<summary>The offset at which text typed before the char at <paramref name="index"/> goes. It continues the char before that one, so after a bold char it is inside the bold markup.</summary>
   public int OffsetAt(int index) => index == 0 ? Source.Start : Chars[index - 1].Source.End;

   ///<summary>
   /// The offsets that a caret moved a character at a time stops at before the char at <paramref name="index"/>, in the order of the markdown:<br/>
   /// <see cref="OffsetAt"/>, and, where inline code starts or ends there, also the offset on the other side of its backticks.
   ///</summary>
   ///<remarks>
   /// Text typed inside code is code and text typed outside it isn't, and the two look different, so a person must be able to put the caret on either side.<br/>
   /// Other markup, such as that of bold, gets no stops of its own: text typed at its edge continues the char before, and the format is toggled to type without it.
   ///</remarks>
   internal IReadOnlyList<int> CaretStopsAt(int index)
   {
      return [..Markups.Where(it => it.Formatting.Has(TextFormats.Code)).SelectMany(OtherSideOfTheEdgeOf).Prepend(OffsetAt(index)).Distinct().Order()];

      IEnumerable<int> OtherSideOfTheEdgeOf(InlineMarkup code)
      {
         if(code.FirstIndex < index && code.EndIndex == index) yield return code.Source.End;
         if(code.FirstIndex == index && code.EndIndex > index) yield return Chars[index].Source.Start;
      }
   }

   ///<summary>The index of the first char that is not before <paramref name="offset"/>.</summary>
   public int IndexAt(int offset) => Chars.TakeWhile(it => it.Source.End <= offset).Count();

   ///<summary>The index of the first of the chars that stand for the same markdown as the char at <paramref name="index"/> does, as those of an image's description do. No caret can be between them.</summary>
   internal int StartOfCharsWithTheSameSourceAs(int index)
   {
      while(index > 0 && index < Length && Chars[index - 1].Source == Chars[index].Source) index--;
      return index;
   }

   ///<summary>The formatting that text typed at <paramref name="offset"/> gets from the markup it is inside.</summary>
   internal TextFormatting FormattingAt(int offset) =>
      Markups.Where(it => it.Content.Touches(offset))
             .Select(it => it.Formatting)
             .Aggregate(TextFormatting.None, (all, it) => new TextFormatting(all.Formats | it.Formats, it.Link ?? all.Link));

   internal int SkipWhiteSpaceBefore(int index)
   {
      while(index > 0 && Chars[index - 1].IsWhiteSpace) index--;
      return index;
   }

   internal int SkipWhiteSpaceAfter(int index)
   {
      while(index < Length && Chars[index].IsWhiteSpace) index++;
      return index;
   }

   internal int SkipLineBreaksBefore(int index)
   {
      while(index > 0 && Chars[index - 1].IsLineBreak) index--;
      return index;
   }

   ///<summary>Where the source of the chars from <paramref name="index"/> starts, with the markup that starts there.</summary>
   internal int StartOfSourceFrom(int index) =>
      index == Length ? Source.End : Markups.Where(it => it.FirstIndex == index && it.EndIndex > index).Select(it => it.Source.Start).Append(Chars[index].Source.Start).Min();

   ///<summary>Where the source of the chars up to <paramref name="index"/> ends, with the markup that ends there.</summary>
   internal int EndOfSourceUpTo(int index) =>
      index == 0 ? Source.Start : Markups.Where(it => it.EndIndex == index && it.FirstIndex < index).Select(it => it.Source.End).Append(Chars[index - 1].Source.End).Max();

   ///<summary>The index that the outermost markup starts at that formats the char before <paramref name="index"/>. That index, when none does.</summary>
   internal int StartOfMarkupThatTouches(int index) => Markups.Where(it => it.FirstIndex < index && it.EndIndex >= index).Select(it => it.FirstIndex).Append(index).Min();

   ///<summary>The index that the outermost markup ends at that formats the char at <paramref name="index"/>. That index, when none does.</summary>
   internal int EndOfMarkupThatTouches(int index) => Markups.Where(it => it.FirstIndex <= index && it.EndIndex > index).Select(it => it.EndIndex).Append(index).Max();
}
