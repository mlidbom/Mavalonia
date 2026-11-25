using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Editing.Looks;

///<summary>How what is rendered is to look after a change: some blocks as the change means them to, and all others as before.</summary>
class ExpectedLooks
{
   readonly IReadOnlyList<BlockLooks> _blocks;

   ExpectedLooks(IReadOnlyList<BlockLooks> blocks) => _blocks = blocks;

   ///<summary>As <paramref name="rendered"/> looks, but with <paramref name="replacements"/> instead of the blocks from <paramref name="first"/> to <paramref name="last"/>.</summary>
   internal static ExpectedLooks OfReplacing(RenderedMarkdown rendered, RenderedBlock first, RenderedBlock last, params IReadOnlyList<BlockLooks> replacements)
   {
      var looks = rendered.Blocks.Select(it => BlockLooks.Of(rendered, it)).ToList();
      return new ExpectedLooks([..looks.Take(rendered.NumberOf(first)), ..replacements, ..looks.Skip(rendered.NumberOf(last) + 1)]);
   }

   internal bool AreThoseAfter(SourceChange change) => change.After.Blocks.Count == _blocks.Count && _blocks.Zip(change.After.Blocks).All(it => it.First.AreThoseOf(it.Second));
}
