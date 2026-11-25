namespace Mavalonia.MarkdownEditing.Editing.Writing;

partial class InlineMarkdownWriter
{
   ///<summary>Markup that has been opened, and is closed after the last character that it formats.</summary>
   readonly record struct OpenMarkup(Markup Markup, string Opening, string Closing)
   {
      internal static OpenMarkup OfCode(Markup markup, string code)
      {
         var backticks = new string('`', LengthOfTheLongestRunOfBackticks() + 1);
         var padding = code.StartsWith('`') || code.EndsWith('`') ? " " : "";
         return new OpenMarkup(markup, backticks + padding, padding + backticks);

         int LengthOfTheLongestRunOfBackticks() => code.Split([..code.Where(it => it != '`').Distinct()]).Max(it => it.Length);
      }
   }
}
