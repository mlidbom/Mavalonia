using Avalonia.Media.TextFormatting;

namespace Mavalonia.Markdown.Controls.Rendered.Layouts.Texts;

///<summary>The text of a block, for Avalonia to lay out: runs of characters that look the same, and what is drawn among them.</summary>
class TextOfABlock : ITextSource
{
   readonly string _text;
   readonly int _length;
   readonly IReadOnlyList<Run> _runs;

   ///<param name="text">The text.</param>
   ///<param name="runs">All of the text, from its start to its end.</param>
   internal TextOfABlock(string text, IReadOnlyList<Run> runs) : this(text, text.Length, runs) {}

   TextOfABlock(string text, int length, IReadOnlyList<Run> runs)
   {
      _text = text;
      _length = length;
      _runs = runs;
   }

   ///<summary>The text as far as the char at <paramref name="end"/>.</summary>
   internal TextOfABlock UpTo(int end) => new(_text, Math.Min(end, _length), _runs);

   public TextRun? GetTextRun(int textSourceIndex)
   {
      if(textSourceIndex >= _length) return null;

      var run = RunAt(textSourceIndex);
      return run.Drawn is {} drawn ? drawn : new TextCharacters(_text.AsMemory(textSourceIndex..Math.Min(run.End, _length)), run.Properties);
   }

   Run RunAt(int index)
   {
      var (first, last) = (0, _runs.Count - 1);
      while(first < last)
      {
         var middle = (first + last + 1) / 2;
         if(_runs[middle].Start <= index) first = middle;
         else last = middle - 1;
      }

      return _runs[first];
   }

   ///<param name="Start">The index of its first char.</param>
   ///<param name="Length">How many chars it has.</param>
   ///<param name="Properties">How its chars look.</param>
   ///<param name="Drawn">What is drawn instead of the chars. Null when the chars are.</param>
   internal readonly record struct Run(int Start, int Length, TextRunProperties Properties, DrawableTextRun? Drawn = null)
   {
      internal int End => Start + Length;
   }
}
