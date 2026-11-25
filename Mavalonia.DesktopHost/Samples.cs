using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Mavalonia.DesktopHost;

///<summary>The markdown that the application comes with, to try the controls on: what the editor starts with, the documents shown in the tabs, and the images that they show.</summary>
static class Samples
{
   static readonly DirectoryInfo Folder = new(Path.Combine(AppContext.BaseDirectory, "Samples"));

   internal static string WhatTheEditorStartsWith => File.ReadAllText(Path.Combine(Folder.FullName, "What the editor starts with.md"));

   ///<summary>Each markdown file in the <c>Samples/Tabs</c> folder.</summary>
   internal static IReadOnlyList<SampleDocument> ForTabs =>
      [..Folder.GetDirectories("Tabs").Single()
               .GetFiles("*.md")
               .OrderBy(it => it.Name, StringComparer.Ordinal)
               .Select(it => new SampleDocument(Path.GetFileNameWithoutExtension(it.Name), File.ReadAllText(it.FullName)))];

   ///<summary>Loads only the images in the <c>Samples</c> folder, as an application should load only images that it trusts.</summary>
   internal static Task<IImage?> LoadImage(string source)
   {
      var path = Path.GetFullPath(Path.Combine(Folder.FullName, source));
      var isASample = path.StartsWith(Folder.FullName + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && File.Exists(path);
      return Task.FromResult<IImage?>(isASample ? new Bitmap(path) : null);
   }
}
