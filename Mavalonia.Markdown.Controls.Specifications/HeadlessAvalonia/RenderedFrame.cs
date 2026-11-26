using System.Reflection;
using System.Security.Cryptography;
using Avalonia.Media.Imaging;

namespace Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

///<summary>A picture of what a <see cref="HeadlessWindow"/> shows.</summary>
class RenderedFrame
{
   static readonly DirectoryInfo Directory = new(typeof(RenderedFrame).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(it => it.Key == "RenderedFramesDirectory").Value!);

   readonly byte[] _png;

   public RenderedFrame(Bitmap bitmap)
   {
      using var png = new MemoryStream();
      bitmap.Save(png, new PngBitmapEncoderOptions());
      _png = png.ToArray();
   }

   ///<summary>The same for two frames exactly when they look the same.</summary>
   public string Fingerprint => Convert.ToHexString(SHA256.HashData(_png));

   ///<summary>Saves the frame as <c>artifacts/rendered-frames/<paramref name="name"/>.png</c>, for a human or an agent to look at.</summary>
   public void SaveForInspectionAs(string name)
   {
      Directory.Create();
      File.WriteAllBytes(Path.Combine(Directory.FullName, $"{name}.png"), _png);
   }
}
