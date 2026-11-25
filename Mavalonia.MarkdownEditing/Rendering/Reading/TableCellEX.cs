using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace Mavalonia.MarkdownEditing.Rendering.Reading;

///<summary>How the cells of a table that Markdig parses are read as GitHub reads them.</summary>
static class TableCellEX
{
   extension(TableCell @this)
   {
      ///<summary>In a cell, <c>\|</c> is a pipe that doesn't end the cell, in code too. Markdig leaves the backslash in the code.</summary>
      internal void TakeTheBackslashOutOfEscapedPipesInCode()
      {
         foreach(var code in @this.Descendants<CodeInline>()) code.Content = code.Content.Replace(@"\|", "|", StringComparison.Ordinal);
      }
   }
}
