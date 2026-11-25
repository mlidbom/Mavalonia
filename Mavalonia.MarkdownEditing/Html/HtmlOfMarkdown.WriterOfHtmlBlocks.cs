using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Mavalonia.MarkdownEditing.Rendering;
using Mavalonia.MarkdownEditing.Rendering.Reading;

namespace Mavalonia.MarkdownEditing.Html;

static partial class HtmlOfMarkdown
{
   ///<summary>Writes blocks of HTML as paragraphs of text, but for comments, which are left out where they are in markdown that is only read.</summary>
   class WriterOfHtmlBlocks : HtmlObjectRenderer<HtmlBlock>
   {
      readonly MarkdownDialect _dialect;

      internal WriterOfHtmlBlocks(MarkdownDialect dialect) => _dialect = dialect;

      protected override void Write(HtmlRenderer renderer, HtmlBlock block)
      {
         if(_dialect == MarkdownDialect.ForReading && block.IsNothingButComments) return;

         renderer.Write("<p>");
         foreach(var (number, line) in block.Lines.Lines.Take(block.Lines.Count).Index())
         {
            if(number > 0) renderer.WriteLine("<br />");
            renderer.WriteEscape(line.Slice.ToString());
         }

         renderer.WriteLine("</p>");
      }
   }
}
