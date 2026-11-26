namespace Mavalonia.Markdown.Controls.Rendered.CodeColours;

///<summary>The colours of the code that was asked for latest.</summary>
class KnownColours
{
   const int MostThatAreKept = 400;

   readonly Dictionary<CodeToColour, LinkedListNode<(CodeToColour Code, ColouredCode Colours)>> _byCode = [];

   ///<summary>The one that was asked for latest last.</summary>
   readonly LinkedList<(CodeToColour Code, ColouredCode Colours)> _byWhenAskedFor = [];

   ///<summary>Null when the colours of <paramref name="code"/> aren't known.</summary>
   internal ColouredCode? Of(CodeToColour code)
   {
      if(!_byCode.TryGetValue(code, out var known)) return null;

      _byWhenAskedFor.Remove(known);
      _byWhenAskedFor.AddLast(known);
      return known.Value.Colours;
   }

   ///<summary>The colours of <paramref name="code"/>, as far as it starts as code does whose colours are known. Null when it starts as none does.</summary>
   internal ColouredCode? OfCodeThatStartsAs(CodeToColour code) =>
      _byWhenAskedFor.Select(it => it.Colours).MaxBy(it => it.LengthInCommonWith(code)) is {} similar && similar.LengthInCommonWith(code) > 0 ? similar.AsFarAsInCommonWith(code) : null;

   internal void Keep(CodeToColour code, ColouredCode colours)
   {
      _byCode[code] = _byWhenAskedFor.AddLast((code, colours));
      if(_byCode.Count <= MostThatAreKept) return;

      _byCode.Remove(_byWhenAskedFor.First!.Value.Code);
      _byWhenAskedFor.RemoveFirst();
   }
}
