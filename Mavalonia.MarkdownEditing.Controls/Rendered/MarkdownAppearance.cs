using Avalonia.Media;
using Avalonia.Media.Immutable;
using Mavalonia.MarkdownEditing.Controls.Rendered.CodeColours;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Containers;

namespace Mavalonia.MarkdownEditing.Controls.Rendered;

///<summary>The fonts and brushes that rendered markdown is drawn with, and the colours of code, as far as they are known.</summary>
record MarkdownAppearance(
   FontFamily FontFamily,
   FontFamily MonospaceFontFamily,
   double FontSize,
   IReadOnlyList<double>? HeadingFontSizes,
   IBrush Foreground,
   IBrush FaintForeground,
   IBrush LinkForeground,
   IBrush CodeBackground,
   IBrush LineBrush,
   IBrush SelectionBrush,
   bool IsDark,
   Func<CodeBlock, ColouredCode?> ColoursOf)
{
   ///<summary>A length that is <paramref name="length"/> where text has a font size of 14, and in proportion where it has another.</summary>
   internal double Scaled(double length) => length * FontSize / 14;

   internal double FontSizeOfHeading(int level) => HeadingFontSizes is { Count: > 0 } sizes ? sizes[Math.Min(level, sizes.Count) - 1] : FontSize * InProportion(level);

   static double InProportion(int level) => level switch
   {
      1 => 1.5,
      2 => 1.3,
      3 => 1.15,
      _ => 1
   };

   ///<summary>The colours that GitHub has for them.</summary>
   internal IBrush BrushOf(Alert alert) => new ImmutableSolidColorBrush(Color.Parse((alert.Kind, IsDark) switch
   {
      (AlertKind.Note, false) => "#0969DA",
      (AlertKind.Note, true) => "#4493F8",
      (AlertKind.Tip, false) => "#1A7F37",
      (AlertKind.Tip, true) => "#3FB950",
      (AlertKind.Important, false) => "#8250DF",
      (AlertKind.Important, true) => "#AB7DF8",
      (AlertKind.Warning, false) => "#9A6700",
      (AlertKind.Warning, true) => "#D29922",
      (_, false) => "#D1242F",
      (_, true) => "#F85149"
   }));
}
