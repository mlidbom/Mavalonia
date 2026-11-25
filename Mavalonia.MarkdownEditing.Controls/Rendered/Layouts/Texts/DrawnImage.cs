using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts.Texts;

///<summary>An image, no wider than the text may be.</summary>
class DrawnImage : DrawableTextRun
{
   readonly IImage _image;

   ///<param name="image">The image.</param>
   ///<param name="length">How many chars its description has.</param>
   ///<param name="widthAvailable">How wide it may be.</param>
   ///<param name="properties">How its description would look.</param>
   internal DrawnImage(IImage image, int length, double widthAvailable, TextRunProperties properties)
   {
      _image = image;
      Length = length;
      Properties = properties;
      Size = image.Size * Math.Min(1, widthAvailable / Math.Max(1, image.Size.Width));
   }

   public override TextRunProperties Properties { get; }
   public override int Length { get; }
   public override Size Size { get; }
   public override double Baseline => Size.Height;

   public override void Draw(DrawingContext drawingContext, Point origin) => drawingContext.DrawImage(_image, new Rect(_image.Size), new Rect(origin, Size));
}
