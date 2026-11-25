using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;

///<summary>Lays out the texts of blocks in lines. Laying out text is slow, so the layout of a text that looks the same as before is used again.</summary>
class TextLayouts : IDisposable
{
   ///<summary>Shows the empty line after a line break at the end of a text, without being seen itself.</summary>
   const string ZeroWidthSpace = "​";

   readonly MarkdownAppearance _appearance;
   Dictionary<Looks, TextLayout> _used = [];
   Dictionary<Looks, TextLayout> _usedBefore = [];
   readonly Dictionary<string, TextLayout> _listMarkers = [];

   internal TextLayouts(MarkdownAppearance appearance) => _appearance = appearance;

   ///<summary>Does <paramref name="layOut"/>, and then forgets the layouts that it didn't use.</summary>
   internal TResult Use<TResult>(Func<TResult> layOut)
   {
      (_usedBefore, _used) = (_used, []);
      var result = layOut();
      foreach(var (looks, layout) in _usedBefore)
         if(!_used.ContainsKey(looks)) layout.Dispose();

      _usedBefore = [];
      return result;
   }

   internal TextLayout Of(RenderedBlock block, double width)
   {
      var looks = Looks.Of(block, width);
      if(_used.TryGetValue(looks, out var layout)) return layout;

      return _used[looks] = _usedBefore.TryGetValue(looks, out var before) ? before : LayOut(block, width);
   }

   internal TextLayout OfListMarker(string marker)
   {
      if(!_listMarkers.TryGetValue(marker, out var layout)) _listMarkers[marker] = layout = new TextLayout(marker, new Typeface(_appearance.FontFamily), _appearance.FontSize, _appearance.FaintForeground);
      return layout;
   }

   public void Dispose()
   {
      foreach(var layout in _used.Values.Concat(_listMarkers.Values)) layout.Dispose();
   }

   TextLayout LayOut(RenderedBlock block, double width)
   {
      var fontSize = block is Heading heading ? _appearance.FontSizeOfHeading(heading.Level) : _appearance.FontSize;
      var isAllBold = block is Heading or TableCell { IsInTheHeader: true };
      var text = block.Text.Value;

      return new TextLayout(text.EndsWith('\n') ? text + ZeroWidthSpace : text,
                            new Typeface(block is CodeBlock ? _appearance.MonospaceFontFamily : _appearance.FontFamily),
                            fontSize,
                            _appearance.Foreground,
                            textWrapping: TextWrapping.Wrap,
                            maxWidth: width,
                            textStyleOverrides: [..StretchesOfTheSameFormatting(block.Text).Select(it => new ValueSpan<TextRunProperties>(it.Start, it.Length, PropertiesOf(it.Formatting)))]);

      TextRunProperties PropertiesOf(TextFormatting formatting)
      {
         var isMonospace = formatting.Has(TextFormats.Code) || formatting.IsShownAsSource;
         var typeface = new Typeface(isMonospace ? _appearance.MonospaceFontFamily : _appearance.FontFamily,
                                     formatting.Has(TextFormats.Italic) ? FontStyle.Italic : FontStyle.Normal,
                                     isAllBold || formatting.Has(TextFormats.Bold) ? FontWeight.Bold : FontWeight.Normal);

         return new GenericTextRunProperties(typeface,
                                             isMonospace ? fontSize * 0.93 : fontSize,
                                             DecorationsOf(formatting),
                                             formatting.Link is not null ? _appearance.LinkForeground : formatting.IsShownAsSource ? _appearance.FaintForeground : _appearance.Foreground,
                                             backgroundBrush: formatting.Has(TextFormats.Code) && block is not CodeBlock ? _appearance.CodeBackground : null);
      }
   }

   static TextDecorationCollection? DecorationsOf(TextFormatting formatting) => (formatting.Link, formatting.Has(TextFormats.Strikethrough)) switch
   {
      (not null, true) => [..TextDecorations.Underline, ..TextDecorations.Strikethrough],
      (not null, false) => TextDecorations.Underline,
      (null, true) => TextDecorations.Strikethrough,
      (null, false) => null
   };

   static IEnumerable<(int Start, int Length, TextFormatting Formatting)> StretchesOfTheSameFormatting(RenderedText text)
   {
      for(var start = 0; start < text.Length;)
      {
         var formatting = text.Characters[start].Formatting;
         var length = text.Characters.Skip(start).TakeWhile(it => it.Formatting == formatting).Count();
         yield return (start, length, formatting);
         start += length;
      }
   }

   ///<summary>All that decides how a text is laid out.</summary>
   // ReSharper disable NotAccessedPositionalProperty.Local : they are what makes two equal.
   readonly record struct Looks(string Text, string Formatting, string KindOfBlock, double Width)
   {
      internal static Looks Of(RenderedBlock block, double width) => new(block.Text.Value, new string([..block.Text.Characters.Select(it => InOneChar(it.Formatting))]), KindOf(block), width);

      static char InOneChar(TextFormatting formatting) => (char)((int)formatting.Formats + (formatting.Link is null ? 0 : 16) + (formatting.IsShownAsSource ? 32 : 0));

      static string KindOf(RenderedBlock block) => block switch
      {
         Heading heading => $"heading {heading.Level}",
         TableCell { IsInTheHeader: true } => "header",
         CodeBlock => "code",
         _ => "text"
      };
   }
}
