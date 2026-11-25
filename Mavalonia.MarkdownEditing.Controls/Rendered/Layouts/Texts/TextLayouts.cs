using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts.Texts;

///<summary>The layouts of the texts of blocks. Laying out text is slow, so the layout of a text that looks the same as before is used again.</summary>
partial class TextLayouts : IDisposable
{
   readonly MarkdownAppearance _appearance;
   Dictionary<Looks, LayoutsOfAText> _used = [];
   Dictionary<Looks, LayoutsOfAText> _usedBefore = [];
   readonly Dictionary<string, TextLayout> _listMarkers = [];

   internal TextLayouts(MarkdownAppearance appearance) => _appearance = appearance;

   ///<summary>Does <paramref name="layOut"/>, and then forgets the layouts of the texts that it didn't use.</summary>
   internal TResult Use<TResult>(Func<TResult> layOut)
   {
      (_usedBefore, _used) = (_used, []);
      var result = layOut();
      foreach(var (looks, text) in _usedBefore)
         if(!_used.ContainsKey(looks)) text.Dispose();

      _usedBefore = [];
      return result;
   }

   internal LaidOutText Of(RenderedBlock block, double width)
   {
      var looks = Looks.Of(block, _appearance);
      if(!_used.TryGetValue(looks, out var text)) _used[looks] = text = UsedBefore(looks) ?? new LayoutsOfAText(it => new RunsOfABlock(block, _appearance, it), looks.DependsOnTheWidth);

      return text.At(width);
   }

   LayoutsOfAText? UsedBefore(Looks looks)
   {
      if(!_usedBefore.TryGetValue(looks, out var text)) return null;

      text.StartAnew();
      return text;
   }

   internal TextLayout OfListMarker(string marker)
   {
      if(!_listMarkers.TryGetValue(marker, out var layout)) _listMarkers[marker] = layout = new TextLayout(marker, new Typeface(_appearance.FontFamily), _appearance.FontSize, _appearance.FaintForeground);
      return layout;
   }

   public void Dispose()
   {
      foreach(var text in _used.Values) text.Dispose();
      foreach(var layout in _listMarkers.Values) layout.Dispose();
   }
}
