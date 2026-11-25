using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;

///<summary>The layouts of the texts of blocks. Laying out text is slow, so the layout of a text that looks the same as before is used again.</summary>
partial class TextLayouts : IDisposable
{
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

      return _used[looks] = _usedBefore.TryGetValue(looks, out var before) ? before : TextLayoutBuilder.LayOut(block, _appearance, width);
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
}
