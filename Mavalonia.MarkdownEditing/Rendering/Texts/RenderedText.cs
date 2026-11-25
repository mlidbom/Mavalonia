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
}
