using Avalonia.Media;

namespace Mavalonia.MarkdownEditing.Controls.Rendered;

///<summary>The fonts and brushes that rendered markdown is drawn with.</summary>
record MarkdownAppearance(
   FontFamily FontFamily,
   FontFamily MonospaceFontFamily,
   double FontSize,
   IBrush Foreground,
   IBrush FaintForeground,
   IBrush LinkForeground,
   IBrush CodeBackground,
   IBrush LineBrush,
   IBrush SelectionBrush,
   IBrush CaretBrush)
{
   ///<summary>A length that is <paramref name="length"/> where text has a font size of 14, and in proportion where it has another.</summary>
   internal double Scaled(double length) => length * FontSize / 14;

   internal double FontSizeOfHeading(int level) => FontSize * level switch
   {
      1 => 1.5,
      2 => 1.3,
      3 => 1.15,
      _ => 1
   };
}
