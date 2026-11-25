using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace Mavalonia.DesktopHost.Checks;

///<summary>Sends a window what Windows sends it when a person uses the keyboard.</summary>
///<remarks>
/// What is sent goes the whole way that a person's typing does, the input method included, which what the headless platform sends doesn't.
/// </remarks>
class WindowsKeyboard
{
   const uint KeyDown = 0x0100;
   const uint KeyUp = 0x0101;
   const uint Char = 0x0102;

   readonly WindowHandle _window;

   internal WindowsKeyboard(Window window) => _window = new WindowHandle(window.TryGetPlatformHandle()!.Handle);

   ///<summary>Gives the window the focus among those of its thread. The window in front, that a person is using, stays in front.</summary>
   internal void FocusTheWindow() => SetFocus(_window.Value);

   internal void Type(char typed) => Send(Char, typed, 0);

   internal void Press(KeyboardKey key)
   {
      const int isDown = 1;
      const int isReleased = 1 | (1 << 30) | (1 << 31);

      Send(KeyDown, (int)key, isDown);
      Send(KeyUp, (int)key, isReleased);
   }

   void Send(uint message, int what, int details)
   {
      if(!PostMessage(_window.Value, message, what, details)) throw new System.ComponentModel.Win32Exception();
   }

   [DllImport("user32.dll", EntryPoint = "PostMessageW", SetLastError = true)]
   [return: MarshalAs(UnmanagedType.Bool)]
   static extern bool PostMessage(nint window, uint message, nint what, nint details);

   [DllImport("user32.dll")]
   static extern nint SetFocus(nint window);

   readonly record struct WindowHandle(nint Value);
}
