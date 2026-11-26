using Mavalonia.Markdown.Rendering;
using Mavalonia.Markdown.Rendering.Blocks;

namespace Mavalonia.Markdown.Editing.Looks;

///<summary>How what is rendered is to look after a change: some blocks as the change means them to, and all others as before.</summary>
class ExpectedLooks
{
   readonly IReadOnlyList<RenderedBlock> _unchangedBefore;
   readonly IReadOnlyList<BlockLooks> _changed;
   readonly IReadOnlyList<RenderedBlock> _unchangedAfter;

   ExpectedLooks(IReadOnlyList<RenderedBlock> unchangedBefore, IReadOnlyList<BlockLooks> changed, IReadOnlyList<RenderedBlock> unchangedAfter)
   {
      _unchangedBefore = unchangedBefore;
      _changed = changed;
      _unchangedAfter = unchangedAfter;
   }

   ///<summary>As <paramref name="rendered"/> looks, but with <paramref name="replacements"/> instead of the blocks from <paramref name="first"/> to <paramref name="last"/>.</summary>
   internal static ExpectedLooks OfReplacing(RenderedMarkdown rendered, RenderedBlock first, RenderedBlock last, params IReadOnlyList<BlockLooks> replacements) =>
      new([..rendered.Blocks.Take(rendered.NumberOf(first))], replacements, [..rendered.Blocks.Skip(rendered.NumberOf(last) + 1)]);

   internal bool AreThoseOf(RenderedMarkdown rendered)
   {
      var blocks = rendered.Blocks;
      if(blocks.Count != _unchangedBefore.Count + _changed.Count + _unchangedAfter.Count) return false;

      return _changed.Zip(blocks.Skip(_unchangedBefore.Count)).All(it => it.First.AreThoseOf(it.Second))
          && _unchangedBefore.Zip(blocks).All(it => it.First.LooksLike(it.Second))
          && _unchangedAfter.Zip(blocks.Skip(_unchangedBefore.Count + _changed.Count)).All(it => it.First.LooksLike(it.Second));
   }
}
