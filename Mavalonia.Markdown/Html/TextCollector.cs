using Mavalonia.Markdown.Editing.Writing;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Html;

///<summary>Collects text as HTML shows it: any amount of white space as one space, and none at the start and the end of a line.</summary>
class TextCollector
{
   const char NoBreakSpace = '\u00A0';

   readonly List<FormattedChar> _chars = [];
   FormattedChar? _pendingSpace;

   bool IsAtTheStartOfALine => _chars.Count == 0 || _chars[^1].Value == '\n';

   internal void Add(string text, TextFormatting formatting)
   {
      foreach(var value in text)
         if(value is ' ' or '\t' or '\n' or '\r' or '\f')
            _pendingSpace ??= FormattedChar.Typed(' ', formatting);
         else if(!IsInvisible(value))
            Add(FormattedChar.Typed(value == NoBreakSpace ? ' ' : value, formatting));
   }

   void Add(FormattedChar formattedChar)
   {
      if(_pendingSpace is {} space && !IsAtTheStartOfALine) _chars.Add(space);
      _pendingSpace = null;

      if(formattedChar.Value != ' ' || !IsAtTheStartOfALine) _chars.Add(formattedChar);
   }

   ///<summary>Such as a soft hyphen, or a zero-width space. An emoji may have a zero-width joiner in it, which is a part of it.</summary>
   static bool IsInvisible(char value) => value is '\u00AD' or '\u200B' or '\uFEFF' || char.IsControl(value);

   ///<summary>Whether the line that is being collected has no text, so that a line break there leaves a blank line.</summary>
   internal bool IsOnABlankLine => IsAtTheStartOfALine && _chars.Count > 0;

   internal void BreakLine()
   {
      EndTheLine();
      if(!IsAtTheStartOfALine) _chars.Add(new FormattedChar('\n', TextFormatting.None, "\n"));
   }

   ///<summary>Takes what has been collected out.</summary>
   internal FormattedText Take()
   {
      EndTheLine();
      if(IsOnABlankLine) _chars.RemoveAt(_chars.Count - 1);

      var text = new FormattedText([.._chars]);
      _chars.Clear();
      return text;
   }

   void EndTheLine()
   {
      _pendingSpace = null;
      while(_chars.Count > 0 && _chars[^1].Value == ' ') _chars.RemoveAt(_chars.Count - 1);
   }
}
