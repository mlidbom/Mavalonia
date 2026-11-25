using Avalonia.Media;
using Avalonia.Media.Immutable;
using TextMateSharp.Grammars;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.CodeColours;

///<summary>Works out the colours of code, as VS Code has them in its default theme, light or dark. Loading the grammar of a language is slow, and none of it may be done by two threads at once.</summary>
class Colourer
{
   static readonly TimeSpan LongestForALine = TimeSpan.FromMilliseconds(200);

   readonly RegistryOptions _options;
   readonly Registry _registry;
   readonly Theme _theme;
   readonly Dictionary<string, IGrammar?> _grammars = [];
   readonly Dictionary<int, IBrush> _brushes = [];

   internal Colourer(bool isDark)
   {
      _options = new RegistryOptions(isDark ? ThemeName.DarkPlus : ThemeName.LightPlus);
      _registry = new Registry(_options);
      _theme = _registry.GetTheme();
   }

   ///<summary>None, when the language isn't known.</summary>
   internal IReadOnlyList<ColouredStretch> StretchesOf(CodeToColour code)
   {
      if(GrammarOf(code.Language) is not {} grammar) return [];

      var stretches = new List<ColouredStretch>();
      IStateStack? state = null;
      var startOfLine = 0;
      foreach(var line in code.Text.Split('\n'))
      {
         var tokens = grammar.TokenizeLine(line, state, LongestForALine);
         state = tokens.RuleStack;

         foreach(var token in tokens.Tokens)
            if(BrushOf(token) is {} brush && Math.Min(token.EndIndex, line.Length) is var end && end > token.StartIndex) stretches.Add(new ColouredStretch(startOfLine + token.StartIndex, end - token.StartIndex, brush));

         startOfLine += line.Length + 1;
      }

      return stretches;
   }

   IGrammar? GrammarOf(string language)
   {
      if(!_grammars.TryGetValue(language, out var grammar)) _grammars[language] = grammar = ScopeOf(language) is {} scope ? _registry.LoadGrammar(scope) : null;
      return grammar;
   }

   ///<summary>Languages are said in many ways: <c>csharp</c>, <c>cs</c> and <c>C#</c> are the same.</summary>
   string? ScopeOf(string language)
   {
      var known = _options.GetAvailableLanguages().FirstOrDefault(it => IsSameText(it.Id, language) || it.Aliases?.Any(alias => IsSameText(alias, language)) == true || it.Extensions?.Contains("." + language, StringComparer.OrdinalIgnoreCase) == true);
      return known is null ? null : _options.GetScopeByLanguageId(known.Id);
   }

   static bool IsSameText(string one, string other) => one.Equals(other, StringComparison.OrdinalIgnoreCase);

   IBrush? BrushOf(IToken token)
   {
      var color = _theme.Match(token.Scopes).Select(it => it.foreground).FirstOrDefault(it => it > 0);
      if(color == 0) return null;

      if(!_brushes.TryGetValue(color, out var brush)) _brushes[color] = brush = new ImmutableSolidColorBrush(Color.Parse(_theme.GetColor(color)));
      return brush;
   }
}
