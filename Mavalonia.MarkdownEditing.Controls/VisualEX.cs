using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;

namespace Mavalonia.MarkdownEditing.Controls;

///<summary>What a control has of the window that it is in, and of the platform that that is on. Only while it is in one.</summary>
static class VisualEX
{
   extension(Visual @this)
   {
      ///<summary>The clipboard of the window.</summary>
      internal IClipboard Clipboard => TopLevel.GetTopLevel(@this)!.Clipboard!;

      ///<summary>The keys that the platform has for what any control does, such as copying, or moving by words.</summary>
      internal PlatformHotkeyConfiguration Hotkeys => @this.GetPlatformSettings()!.HotkeyConfiguration;
   }
}
