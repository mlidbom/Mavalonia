namespace Mavalonia.Markdown.Rendering.Containers;

///<summary>A quote, whose lines start with <c>&gt;</c>.</summary>
public class Quote : BlockContainer
{
   const int MostSpacesBeforeTheMark = 3;

   internal override string PrefixOfItsFirstLine => "> ";
   internal override string PrefixOfItsOtherLines => "> ";

   internal override int? EndOfItsPrefix(string markdown, int offset)
   {
      var mark = offset;
      while(mark < markdown.Length && markdown[mark] == ' ' && mark - offset < MostSpacesBeforeTheMark) mark++;

      if(mark >= markdown.Length || markdown[mark] != '>') return null;

      var afterTheMark = mark + 1;
      return afterTheMark < markdown.Length && markdown[afterTheMark] == ' ' ? afterTheMark + 1 : afterTheMark;
   }

   internal override bool LooksLike(BlockContainer other) => other is Quote;
}
