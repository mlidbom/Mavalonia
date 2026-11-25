using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts;

///<summary>Lays out the text of a block in lines.</summary>
class TextLayoutBuilder
{
   ///<summary>Shows the empty line after a line break at the end of a text, without being seen itself.</summary>
   const string ZeroWidthSpace = "​";

   readonly RenderedBlock _block;
   readonly MarkdownAppearance _appearance;

   TextLayoutBuilder(RenderedBlock block, MarkdownAppearance appearance)
   {
      _block = block;
      _appearance = appearance;
   }

   internal static TextLayout LayOut(RenderedBlock block, MarkdownAppearance appearance, double width) => new TextLayoutBuilder(block, appearance).LayOut(width);

   double FontSize => _block is Heading heading ? _appearance.FontSizeOfHeading(heading.Level) : _appearance.FontSize;
   FontFamily FontFamily => _block is CodeBlock ? _appearance.MonospaceFontFamily : _appearance.FontFamily;
   bool IsAllBold => _block is Heading or AlertTitle or TableCell { IsInTheHeader: true };

   TextLayout LayOut(double width) =>
      new(_block.Text.Value.EndsWith('\n') ? _block.Text.Value + ZeroWidthSpace : _block.Text.Value,
          new Typeface(FontFamily),
          FontSize,
          _appearance.Foreground,
          textWrapping: TextWrapping.Wrap,
          maxWidth: width,
          textStyleOverrides: [.._block.Text.StretchesOfTheSameFormatting.Select(it => new ValueSpan<TextRunProperties>(it.Start, it.Length, PropertiesOf(it.Formatting)))]);

   TextRunProperties PropertiesOf(TextFormatting formatting)
   {
      var isMonospace = formatting.Has(TextFormats.Code) || formatting.IsShownAsSource;
      var typeface = new Typeface(isMonospace ? _appearance.MonospaceFontFamily : _appearance.FontFamily,
                                  formatting.Has(TextFormats.Italic) ? FontStyle.Italic : FontStyle.Normal,
                                  IsAllBold || formatting.Has(TextFormats.Bold) ? FontWeight.Bold : FontWeight.Normal);

      return new GenericTextRunProperties(typeface,
                                          isMonospace ? FontSize * 0.93 : FontSize,
                                          DecorationsOf(formatting),
                                          ForegroundOf(formatting),
                                          backgroundBrush: formatting.Has(TextFormats.Code) && _block is not CodeBlock ? _appearance.CodeBackground : null);
   }

   IBrush ForegroundOf(TextFormatting formatting) => formatting switch
   {
      { Link: not null } => _appearance.LinkForeground,
      { IsShownAsSource: true } => _appearance.FaintForeground,
      _ => _block is AlertTitle title ? _appearance.BrushOf(title.Alert) : _appearance.Foreground
   };

   static TextDecorationCollection? DecorationsOf(TextFormatting formatting) => (formatting.Link, formatting.Has(TextFormats.Strikethrough)) switch
   {
      (not null, true) => [..TextDecorations.Underline, ..TextDecorations.Strikethrough],
      (not null, false) => TextDecorations.Underline,
      (null, true) => TextDecorations.Strikethrough,
      (null, false) => null
   };
}
