using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Mavalonia.MarkdownEditing.Controls.Rendered.CodeColours;
using Mavalonia.MarkdownEditing.Rendering.Blocks;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Controls.Rendered.Layouts.Texts;

///<summary>The runs that the text of a block is drawn as.</summary>
class RunsOfABlock
{
   ///<summary>Shows the empty line after a line break at the end of a text, without being seen itself.</summary>
   const string ZeroWidthSpace = "\u200B";

   const double SizeOfMonospace = 0.93;

   readonly RenderedBlock _block;
   readonly MarkdownAppearance _appearance;
   readonly ColouredCode? _colours;
   readonly double _widthAvailable;

   ///<param name="block">The block.</param>
   ///<param name="appearance">How it's to look.</param>
   ///<param name="colours">The colours of the block, when it's code and they are known.</param>
   ///<param name="widthAvailable">How wide an image may be.</param>
   internal RunsOfABlock(RenderedBlock block, MarkdownAppearance appearance, ColouredCode? colours, double widthAvailable)
   {
      _block = block;
      _appearance = appearance;
      _colours = colours;
      _widthAvailable = widthAvailable;
   }

   double FontSize => _block is Heading heading ? _appearance.FontSizeOfHeading(heading.Level) : _appearance.FontSize;
   FontFamily FontFamily => _block is CodeBlock ? _appearance.MonospaceFontFamily : _appearance.FontFamily;
   bool IsAllBold => _block is Heading or AlertTitle or TableCell { IsInTheHeader: true };

   internal TextRunProperties PropertiesOfTheParagraph => new GenericTextRunProperties(new Typeface(FontFamily), FontSize, foregroundBrush: _appearance.Foreground);

   internal TextOfABlock Text
   {
      get
      {
         var text = _block.Text.Value;
         var runs = (_colours is null ? FormattedRuns() : ColouredRuns(_colours)).SelectMany(WithWhatIsDrawn).ToList();
         if(text.Length > 0 && !text.EndsWith('\n')) return new TextOfABlock(text, runs);

         return new TextOfABlock(text + ZeroWidthSpace, [..runs, new TextOfABlock.Run(text.Length, ZeroWidthSpace.Length, PropertiesOfTheParagraph)]);
      }
   }

   IEnumerable<TextOfABlock.Run> FormattedRuns() =>
      _block.Text.StretchesOfTheSameFormatting.Select(it => it.Formatting.Image is {} image && _appearance.ImageOf(image) is {} loaded
                                                               ? new TextOfABlock.Run(it.Start, it.Length, PropertiesOf(it.Formatting), new DrawnImage(loaded, it.Length, _widthAvailable, PropertiesOf(it.Formatting)))
                                                               : new TextOfABlock.Run(it.Start, it.Length, PropertiesOf(it.Formatting)));

   ///<summary>What is between the stretches that have a colour is drawn as code without any.</summary>
   IEnumerable<TextOfABlock.Run> ColouredRuns(ColouredCode colours)
   {
      var end = 0;
      foreach(var stretch in colours.Stretches)
      {
         if(stretch.Start > end) yield return Code(end, stretch.Start, _appearance.Foreground);
         yield return Code(stretch.Start, stretch.End, stretch.Foreground);
         end = stretch.End;
      }

      if(_block.Text.Length > end) yield return Code(end, _block.Text.Length, _appearance.Foreground);

      TextOfABlock.Run Code(int start, int endOfRun, IBrush foreground) =>
         new(start, endOfRun - start, new GenericTextRunProperties(new Typeface(_appearance.MonospaceFontFamily), FontSize * SizeOfMonospace, foregroundBrush: foreground));
   }

   ///<summary>The run, cut at the boxes of tasks in it.</summary>
   IEnumerable<TextOfABlock.Run> WithWhatIsDrawn(TextOfABlock.Run run)
   {
      if(run.Drawn is not null || _block is CodeBlock)
      {
         yield return run;
         yield break;
      }

      var start = run.Start;
      for(var index = run.Start; index < run.End; index++)
      {
         if(_block.Text.Value[index] is not (DrawnBox.Empty or DrawnBox.Ticked)) continue;

         if(index > start) yield return run with { Start = start, Length = index - start };
         yield return new TextOfABlock.Run(index, 1, run.Properties, new DrawnBox(_block.Text.Value[index], run.Properties));
         start = index + 1;
      }

      if(run.End > start) yield return run with { Start = start, Length = run.End - start };
   }

   TextRunProperties PropertiesOf(TextFormatting formatting)
   {
      var isMonospace = formatting.Has(TextFormats.Code) || formatting.IsShownAsSource;
      var typeface = new Typeface(isMonospace ? _appearance.MonospaceFontFamily : _appearance.FontFamily,
                                  formatting.Has(TextFormats.Italic) ? FontStyle.Italic : FontStyle.Normal,
                                  IsAllBold || formatting.Has(TextFormats.Bold) ? FontWeight.Bold : FontWeight.Normal);

      return new GenericTextRunProperties(typeface,
                                          isMonospace ? FontSize * SizeOfMonospace : FontSize,
                                          DecorationsOf(formatting),
                                          ForegroundOf(formatting),
                                          backgroundBrush: formatting.Has(TextFormats.Code) && _block is not CodeBlock ? _appearance.CodeBackground : null);
   }

   ///<summary>An image that isn't shown, since there's none to show, can be clicked as a link can.</summary>
   IBrush ForegroundOf(TextFormatting formatting) => formatting switch
   {
      { Link: not null } or { Image: not null } => _appearance.LinkForeground,
      { IsShownAsSource: true } => _appearance.FaintForeground,
      _ => _block is AlertTitle title ? _appearance.BrushOf(title.Alert) : _appearance.Foreground
   };

   static TextDecorationCollection? DecorationsOf(TextFormatting formatting) => (formatting.Link ?? (object?)formatting.Image, formatting.Has(TextFormats.Strikethrough)) switch
   {
      (not null, true) => [..TextDecorations.Underline, ..TextDecorations.Strikethrough],
      (not null, false) => TextDecorations.Underline,
      (null, true) => TextDecorations.Strikethrough,
      (null, false) => null
   };
}
