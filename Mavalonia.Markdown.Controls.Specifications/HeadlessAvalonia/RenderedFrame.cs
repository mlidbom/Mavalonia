using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

///<summary>A picture of what a <see cref="HeadlessWindow"/> shows.</summary>
class RenderedFrame
{
   static readonly DirectoryInfo Directory = new(typeof(RenderedFrame).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().Single(it => it.Key == "RenderedFramesDirectory").Value!);

   const int BytesOfAPixel = 4;

   readonly byte[] _png;

   ///<summary>Each pixel as blue, green, red and alpha, a row after another.</summary>
   readonly byte[] _pixels;

   readonly int _bytesOfARow;

   public RenderedFrame(Bitmap bitmap)
   {
      using var png = new MemoryStream();
      bitmap.Save(png, new PngBitmapEncoderOptions());
      _png = png.ToArray();
      (_pixels, _bytesOfARow) = PixelsOf(bitmap);
   }

   static (byte[] Pixels, int BytesOfARow) PixelsOf(Bitmap bitmap)
   {
      using var copy = new WriteableBitmap(bitmap.PixelSize, bitmap.Dpi, PixelFormat.Bgra8888, AlphaFormat.Premul);
      using var locked = copy.Lock();
      bitmap.CopyPixels(locked);
      var pixels = new byte[locked.RowBytes * locked.Size.Height];
      Marshal.Copy(locked.Address, pixels, 0, pixels.Length);
      return (pixels, locked.RowBytes);
   }

   ///<summary>The same for two frames exactly when they look the same.</summary>
   public string Fingerprint => Convert.ToHexString(SHA256.HashData(_png));

   ///<summary>How many pixels of each row of <paramref name="area"/> differ from those of <paramref name="other"/>, from its top row down: such as to see where something is drawn in a colour that differs between them.</summary>
   public IReadOnlyList<int> PixelsThatDifferFrom(RenderedFrame other, PixelRect area) =>
      [..Enumerable.Range(area.Y, area.Height).Select(row => Enumerable.Range(area.X, area.Width).Count(column => !PixelAt(column, row).SequenceEqual(other.PixelAt(column, row))))];

   ReadOnlySpan<byte> PixelAt(int column, int row) => _pixels.AsSpan(row * _bytesOfARow + column * BytesOfAPixel, BytesOfAPixel);

   ///<summary>Saves the frame as <c>artifacts/rendered-frames/<paramref name="name"/>.png</c>, for a human or an agent to look at.</summary>
   public void SaveForInspectionAs(string name)
   {
      Directory.Create();
      File.WriteAllBytes(Path.Combine(Directory.FullName, $"{name}.png"), _png);
   }
}
