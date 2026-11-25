using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Utilities;
using Mavalonia.MarkdownEditing.Controls.Rendered.CodeColours;
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
   readonly ColouredCode? _colours;

   TextLayoutBuilder(RenderedBlock block, MarkdownAppearance appearance, ColouredCode? colours)
   {
      _block = block;
      _appearance = appearance;
      _colours = colours;
   }

   ///<param name="block">The block whose text is laid out.</param>
   ///<param name="appearance">How it's to look.</param>
   ///<param name="width">How wide it may be.</param>
   ///<param name="colours">The colours of the block, when it's code and they are known.</param>
   internal static TextLayout LayOut(RenderedBlock block, MarkdownAppearance appearance, double width, ColouredCode? colours) => new TextLayoutBuilder(block, appearance, colours).LayOut(width);

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
          textStyleOverrides: [..PropertiesOfStretches()]);

   IEnumerable<ValueSpan<TextRunProperties>> PropertiesOfStretches() =>
      _colours is null ? _block.Text.StretchesOfTheSameFormatting.Select(it => new ValueSpan<TextRunProperties>(it.Start, it.Length, PropertiesOf(it.Formatting))) : PropertiesOfColouredStretches(_colours);

   ///<summary>What is between the stretches that have a colour is drawn as code without any.</summary>
   IEnumerable<ValueSpan<TextRunProperties>> PropertiesOfColouredStretches(ColouredCode colours)
   {
      var end = 0;
      foreach(var stretch in colours.Stretches)
      {
         if(stretch.Start > end) yield return Code(end, stretch.Start, _appearance.Foreground);
         yield return Code(stretch.Start, stretch.End, stretch.Foreground);
         end = stretch.End;
      }

      if(_block.Text.Length > end) yield return Code(end, _block.Text.Length, _appearance.Foreground);

      ValueSpan<TextRunProperties> Code(int start, int endOfStretch, IBrush foreground) => new(start, endOfStretch - start, new GenericTextRunProperties(new Typeface(_appearance.MonospaceFontFamily), FontSize * 0.93, foregroundBrush: foreground));
   }

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
