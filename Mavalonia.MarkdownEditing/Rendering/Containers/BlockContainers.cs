namespace Mavalonia.MarkdownEditing.Rendering.Containers;

///<summary>The containers that a block is inside, the outermost first.</summary>
public class BlockContainers
{
   internal static readonly BlockContainers None = new([]);

   BlockContainers(IReadOnlyList<BlockContainer> outermostFirst) => OutermostFirst = outermostFirst;

   public IReadOnlyList<BlockContainer> OutermostFirst { get; }

   internal int Count => OutermostFirst.Count;
   public BlockContainer? Innermost => OutermostFirst.Count > 0 ? OutermostFirst[^1] : null;

   internal BlockContainers Inside(BlockContainer container) => new([..OutermostFirst, container]);

   BlockContainers Outermost(int count) => new([..OutermostFirst.Take(count)]);
   internal BlockContainers WithoutTheInnermost => Outermost(Count - 1);

   ///<summary>The containers that <paramref name="others"/> has too.</summary>
   internal BlockContainers SharedWith(BlockContainers others) => Outermost(OutermostFirst.Zip(others.OutermostFirst).TakeWhile(it => it.First == it.Second).Count());

   internal bool LookLike(BlockContainers others) => Count == others.Count && OutermostFirst.Zip(others.OutermostFirst).All(it => it.First.LooksLike(it.Second));

   ///<summary>What precedes the text on a line inside the containers, other than the first line of any of them.</summary>
   internal string PrefixOfTheirLines => string.Concat(OutermostFirst.Select(it => it.PrefixOfItsOtherLines));

   ///<summary>What precedes the text on a line that is the first of the innermost container, and not of the others.</summary>
   internal string PrefixOfTheFirstLineOfTheInnermost => Innermost is null ? "" : WithoutTheInnermost.PrefixOfTheirLines + Innermost.PrefixOfItsFirstLine;

   ///<summary>The outermost of the containers: those whose prefixes are on the line that starts at <paramref name="startOfLine"/>.</summary>
   internal BlockContainers OfTheLineAt(string markdown, int startOfLine) => ReadPrefixes(markdown, startOfLine).Containers;

   ///<summary>The offset after the prefixes of the containers on the line that starts at <paramref name="startOfLine"/>, as far as they are there.</summary>
   internal int EndOfTheirPrefixes(string markdown, int startOfLine) => ReadPrefixes(markdown, startOfLine).End;

   (BlockContainers Containers, int End) ReadPrefixes(string markdown, int startOfLine)
   {
      var (count, end) = (0, startOfLine);
      while(count < Count && OutermostFirst[count].EndOfItsPrefix(markdown, end) is {} endOfPrefix) (count, end) = (count + 1, endOfPrefix);

      return (Outermost(count), end);
   }
}
