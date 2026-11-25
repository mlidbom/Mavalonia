using System.Runtime.CompilerServices;
using Avalonia;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.CodeColours;

///<summary>The colours of all code that an application shows. They are worked out off the UI thread, and kept for the code that was shown latest. Used on the UI thread only.</summary>
class ColoursOfCode
{
   const int MostThatIsKept = 400;
   const int LongestToColour = 60_000;

   static readonly ConditionalWeakTable<Application, ColoursOfCode> OfApplications = [];

   internal static ColoursOfCode OfTheApplication => OfApplications.GetOrCreateValue(Application.Current!);

   readonly Lazy<Colourer> _light = new(() => new Colourer(isDark: false));
   readonly Lazy<Colourer> _dark = new(() => new Colourer(isDark: true));

   ///<summary>The code whose colours are known, the one that was asked for latest last.</summary>
   readonly Dictionary<CodeToColour, LinkedListNode<(CodeToColour Code, ColouredCode Colours)>> _known = [];
   readonly LinkedList<(CodeToColour Code, ColouredCode Colours)> _byWhenAskedFor = [];

   ///<summary>The code whose colours aren't known yet, with what to do when they are.</summary>
   readonly Dictionary<CodeToColour, HashSet<Action>> _waiting = [];

   CodeToColour? _beingWorkedOut;

   ///<summary>The work that was started latest. Each piece of it waits for the one before.</summary>
   Task _work = Task.CompletedTask;

   ///<summary>The colours of <paramref name="block"/>, as far as they are known. When more become known, <paramref name="whenKnown"/> is done. Null when there are none.</summary>
   internal ColouredCode? Of(CodeBlock block, bool isDark, Action whenKnown)
   {
      if(block.Language.Length == 0 || block.Text.Length > LongestToColour) return null;

      var code = new CodeToColour(block.Text.Value, LanguageIn(block.Language), isDark);
      if(_known.TryGetValue(code, out var known)) return AskedForNow(known);

      WorkOut(code, whenKnown);
      return OfCodeThatStartsTheSame(code);
   }

   ///<summary>What follows the language, as in <c>cs title="x"</c>, says something else.</summary>
   static string LanguageIn(string info) => info.Split([' ', ',', '{'], 2)[0].ToLowerInvariant();

   ColouredCode AskedForNow(LinkedListNode<(CodeToColour Code, ColouredCode Colours)> known)
   {
      _byWhenAskedFor.Remove(known);
      _byWhenAskedFor.AddLast(known);
      return known.Value.Colours;
   }

   ///<summary>While code is written, its colours would go away at each change otherwise, until they are worked out anew.</summary>
   ColouredCode? OfCodeThatStartsTheSame(CodeToColour code) =>
      _byWhenAskedFor.Select(it => it.Colours).MaxBy(it => it.LengthInCommonWith(code)) is {} similar && similar.LengthInCommonWith(code) > 0 ? similar.AsFarAsInCommonWith(code) : null;

   void WorkOut(CodeToColour code, Action whenKnown)
   {
      if(_waiting.TryGetValue(code, out var waiting))
      {
         waiting.Add(whenKnown);
         return;
      }

      _waiting[code] = [..WhatWaitsForTheStartOf(code), whenKnown];
      _work = WorkOutAfter(_work, code);
      _work.SurfaceExceptions();
   }

   ///<summary>While code is written, its colours aren't worked out as it was before. What waits for those waits for the colours of what it is now.</summary>
   IReadOnlyList<Action> WhatWaitsForTheStartOf(CodeToColour code)
   {
      var starts = _waiting.Where(it => it.Key != _beingWorkedOut && code.GoesOnFrom(it.Key)).ToList();
      foreach(var start in starts) _waiting.Remove(start.Key);
      return [..starts.SelectMany(it => it.Value)];
   }

   async Task WorkOutAfter(Task workBefore, CodeToColour code)
   {
      await Task.WhenAny(workBefore); // Whether or not that failed, which has surfaced.
      if(!_waiting.ContainsKey(code)) return;

      _beingWorkedOut = code;
      var colourer = code.IsDark ? _dark : _light;
      Keep(code, new ColouredCode(code, await Task.Run(() => colourer.Value.StretchesOf(code))));

      _waiting.Remove(code, out var waiting);
      foreach(var whenKnown in waiting!) whenKnown();
   }

   void Keep(CodeToColour code, ColouredCode colours)
   {
      _known[code] = _byWhenAskedFor.AddLast((code, colours));
      if(_known.Count <= MostThatIsKept) return;

      _known.Remove(_byWhenAskedFor.First!.Value.Code);
      _byWhenAskedFor.RemoveFirst();
   }
}
