using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using AvaloniaEdit.Document;

namespace Mavalonia.DesktopHost.Checks;

///<summary>Types into the editor on the real desktop, says whether what comes of it is right, and ends the application.<br/>
/// The exit code is 0 when it is right.</summary>
///<remarks>
/// The specifications run on the headless platform, which lacks what the real one has, such as input methods. That once hid that typing the first character threw.<br/>
/// The window is off the screen and never in front, so the check doesn't disturb whoever is using the computer.
/// </remarks>
class TypingCheck
{
   internal const string Argument = "--check-typing";

   const string Expected = "**bold** text\n\n- item\n- next one";
   static readonly PixelPoint OffTheScreen = new(-30000, -30000);
   static readonly TimeSpan BetweenKeys = TimeSpan.FromMilliseconds(30); // So that the view is laid out and drawn between them, as when a person types.

   readonly MainWindow _window;
   readonly IClassicDesktopStyleApplicationLifetime _application;

   internal TypingCheck(MainWindow window, IClassicDesktopStyleApplicationLifetime application)
   {
      _window = window;
      _application = application;
   }

   internal void RunWhenTheWindowOpens()
   {
      _window.ShowActivated = false;
      _window.ShowInTaskbar = false;
      _window.WindowStartupLocation = WindowStartupLocation.Manual;
      _window.Position = OffTheScreen;
      _window.Opened += async (_, _) => await Run();

      Dispatcher.UIThread.UnhandledException += (_, failure) =>
      {
         failure.Handled = true;
         End(exitCode: 1, $"Typing threw:\n{failure.Exception}");
      };
   }

   async Task Run()
   {
      _window.Editor.Document = new TextDocument();
      var keyboard = new WindowsKeyboard(_window);
      keyboard.FocusTheWindow();

      await InTheRenderedView();
      await Type("**bold** text");
      await Press(KeyboardKey.Enter);
      await Type("- item");
      await Press(KeyboardKey.Enter);
      await Type("nexx");
      await Press(KeyboardKey.Backspace);

      await InTheSourceView();
      await Type("t");

      await InTheRenderedView();
      await Type(" one");

      var markdown = _window.Editor.Document.Text.ReplaceLineEndings("\n");
      if(markdown == Expected) End(exitCode: 0, "Typing works.");
      else End(exitCode: 1, $"Typing gave:\n{markdown}\n\nExpected:\n{Expected}");

      async Task InTheRenderedView() => await In(showsSource: false);
      async Task InTheSourceView() => await In(showsSource: true);

      async Task In(bool showsSource)
      {
         _window.Editor.ShowsSource = showsSource;
         _window.Editor.FocusTheView();
         await Task.Delay(BetweenKeys);
      }

      async Task Type(string text)
      {
         foreach(var typed in text)
         {
            keyboard.Type(typed);
            await Task.Delay(BetweenKeys);
         }
      }

      async Task Press(KeyboardKey key)
      {
         keyboard.Press(key);
         await Task.Delay(BetweenKeys);
      }
   }

   void End(int exitCode, string result)
   {
      Console.WriteLine(result);
      _application.Shutdown(exitCode);
   }
}
