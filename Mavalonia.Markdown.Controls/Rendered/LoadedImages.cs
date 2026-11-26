using Avalonia.Media;
using Mavalonia.Markdown.Rendering.Texts;

namespace Mavalonia.Markdown.Controls.Rendered;

///<summary>The images that a <see cref="MarkdownView"/> shows, as far as they are loaded. Each is loaded when it's first asked for.</summary>
class LoadedImages
{
   readonly Func<string, Task<IImage?>> _load;
   readonly Action _whenLoaded;

   ///<summary>By where they are. Null for one that isn't loaded yet, or that there isn't.</summary>
   readonly Dictionary<string, IImage?> _images = [];

   ///<param name="load">Loads the image at a place. Null when there is none.</param>
   ///<param name="whenLoaded">Done each time that one has been loaded.</param>
   internal LoadedImages(Func<string, Task<IImage?>> load, Action whenLoaded)
   {
      _load = load;
      _whenLoaded = whenLoaded;
   }

   internal IImage? Of(InlineImage image)
   {
      if(!_images.TryGetValue(image.Source, out var loaded)) Load(image.Source).SurfaceExceptions();

      return loaded;
   }

   async Task Load(string source)
   {
      _images[source] = null;
      await Task.Yield(); // It's asked for while what is shown is being laid out.
      if(await _load(source) is not {} image) return;

      _images[source] = image;
      _whenLoaded();
   }
}
