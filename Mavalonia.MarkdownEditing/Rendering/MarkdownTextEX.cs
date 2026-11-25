namespace Mavalonia.MarkdownEditing.Rendering;

///<summary>What we need to find in a markdown text, by its characters alone.</summary>
static class MarkdownTextEX
{
   static readonly char[] CharactersOfLineBreaks = ['\r', '\n'];

   extension(ReadOnlySpan<char> @this)
   {
      internal bool IsWhiteSpaceWithoutLineBreak() => @this.IndexOfAnyExcept(' ', '\t') < 0;
   }

   extension(string @this)
   {
      ///<summary>The offset of the first character of the line that <paramref name="offset"/> is on.</summary>
      internal int StartOfLineAt(int offset)
      {
         var searchFrom = @this.IsInsideALineBreak(offset) ? offset - 2 : offset - 1;
         return searchFrom < 0 ? 0 : @this.LastIndexOfAny(CharactersOfLineBreaks, searchFrom) + 1;
      }

      ///<summary>The offset after the last character of the line that <paramref name="offset"/> is on: before its line break, if it has one.</summary>
      internal int EndOfLineAt(int offset)
      {
         if(@this.IsInsideALineBreak(offset)) return offset - 1;
         return @this.IndexOfAny(CharactersOfLineBreaks, offset) is var lineBreak and >= 0 ? lineBreak : @this.Length;
      }

      ///<summary>The offset of the first character of the line after the one that <paramref name="offset"/> is on. Null when that is the last line.</summary>
      internal int? StartOfLineAfter(int offset)
      {
         var endOfLine = @this.EndOfLineAt(offset);
         if(endOfLine == @this.Length) return null;

         return @this.IsInsideALineBreak(endOfLine + 1) ? endOfLine + 2 : endOfLine + 1;
      }

      ///<summary>Whether <paramref name="offset"/> is between the two characters of a line break of Windows.</summary>
      internal bool IsInsideALineBreak(int offset) => offset > 0 && offset < @this.Length && @this[offset - 1] == '\r' && @this[offset] == '\n';

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
