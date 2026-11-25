using Markdig.Syntax.Inlines;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>How the inlines that Markdig parses are inside one another.</summary>
static class ContainerInlineEX
{
   extension(ContainerInline @this)
   {
      ///<summary>Markdig has what follows a delimiter that delimits nothing, such as a <c>[</c> that no <c>]</c> follows, inside that delimiter, so that thousands of them are inside one another. Puts it after the delimiter instead.</summary>
      internal void TakeWhatFollowsDelimitersOutOfThem()
      {
         var containers = new Stack<ContainerInline>([@this]);
         while(containers.TryPop(out var container))
            for(var inline = container.FirstChild; inline is not null; inline = inline.NextSibling)
               if(inline is DelimiterInline delimiter) delimiter.MoveChildrenAfter(delimiter);
               else if(inline is ContainerInline inner) containers.Push(inner);
      }

      internal bool AreInsideOneAnotherDeeperThan(int depth) => depth < 0 || @this.OfType<ContainerInline>().Any(it => it.AreInsideOneAnotherDeeperThan(depth - 1));
   }
}
