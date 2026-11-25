namespace Mavalonia.MarkdownEditing.Rendering;

///<summary>What we need to find in a markdown text, by its characters alone.</summary>
static class MarkdownTextEX
{
   extension(ReadOnlySpan<char> @this)
   {
      internal bool IsWhiteSpaceWithoutLineBreak() => @this.IndexOfAnyExcept(' ', '\t') < 0;
   }

   extension(string @this)
   {
      ///<summary>The offset of the first character of the line that <paramref name="offset"/> is on.</summary>
      internal int StartOfLineAt(int offset) => offset == 0 ? 0 : @this.LastIndexOf('\n', offset - 1) + 1;

      ///<summary>The offset after the last character of the line that <paramref name="offset"/> is on: before its line break, if it has one.</summary>
      internal int EndOfLineAt(int offset)
      {
         var lineBreak = @this.IndexOf('\n', offset);
         if(lineBreak < 0) return @this.Length;
         return lineBreak > offset && @this[lineBreak - 1] == '\r' ? lineBreak - 1 : lineBreak;
      }

      ///<summary>The offset of the first character of the line after the one that <paramref name="offset"/> is on. Null when that is the last line.</summary>
      internal int? StartOfLineAfter(int offset) => @this.IndexOf('\n', offset) is var lineBreak and >= 0 ? lineBreak + 1 : null;

      ///<summary>The offset of the first character from <paramref name="offset"/> that isn't a space or a tab.</summary>
      internal int SkipSpaces(int offset)
      {
         while(offset < @this.Length && @this[offset] is ' ' or '\t') offset++;
         return offset;
      }

      ///<summary>The line break that the text uses.</summary>
      internal string LineBreak => @this.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
   }
}
