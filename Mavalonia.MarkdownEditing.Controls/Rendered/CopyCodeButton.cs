using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using Mavalonia.MarkdownEditing.Controls.Rendered.Input;
using Mavalonia.MarkdownEditing.Rendering.Blocks;

namespace Mavalonia.MarkdownEditing.Controls.Rendered;

///<summary>Copies the code of the block that the pointer is over. It's at the top right of that block.</summary>
class CopyCodeButton : Button
{
   const double DistanceFromTheEdge = 4;

   readonly DispatcherTimer _untilItSaysCopyAgain = new() { Interval = TimeSpan.FromSeconds(1.5) };
   Rect _boundsOfTheBlock;

   internal CopyCodeButton()
   {
      IsVisible = false;
      Focusable = false;
      Padding = new Thickness(8, 2);
      Cursor = Cursors.OfAButton;
      AutomationProperties.SetName(this, "Copy the code");
      Click += (_, _) => ClipboardInput.Use(CopyTheCode());
      _untilItSaysCopyAgain.Tick += (_, _) => SayCopy();
   }

   protected override Type StyleKeyOverride => typeof(Button);

   ///<summary>The block whose code is copied. Null while the button isn't shown.</summary>
   internal CodeBlock? Block { get; private set; }

   internal void ShowFor(CodeBlock block, Rect bounds)
   {
      var isAnother = block != Block;
      (Block, _boundsOfTheBlock, IsVisible) = (block, bounds, true);
      if(isAnother) SayCopy();
   }

   internal void Hide() => (Block, IsVisible) = (null, false);

   internal void ArrangeAtItsBlock()
   {
      if(IsVisible) Arrange(new Rect(new Point(_boundsOfTheBlock.Right - DesiredSize.Width - DistanceFromTheEdge, _boundsOfTheBlock.Top + DistanceFromTheEdge), DesiredSize));
   }

   void SayCopy()
   {
      _untilItSaysCopyAgain.Stop();
      Content = Block is { Language.Length: > 0 } block ? $"{block.Language} · Copy" : "Copy";
   }

   async Task CopyTheCode()
   {
      await TopLevel.GetTopLevel(this)!.Clipboard!.SetTextAsync(Block!.Text.Value);
      Content = "Copied";
      _untilItSaysCopyAgain.Start();
   }
}
