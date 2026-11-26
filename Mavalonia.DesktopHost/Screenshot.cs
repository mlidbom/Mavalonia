using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Mavalonia.DesktopHost;

///<summary>Draws the window, as it opens with its samples, to a PNG file, and ends the application:
/// the picture of the controls in the README.<br/>
/// <c>dotnet run --project Mavalonia.DesktopHost -- --screenshot images/desktop-host.png</c></summary>
///<remarks>
/// The window is off the screen and never in front, as for the <see cref="Checks.TypingCheck"/>, so making the picture
/// doesn't disturb whoever is using the computer. What is drawn is the window's content: the title bar is Windows'.<br/>
/// It is drawn at twice the window's size, so that it is sharp on screens that scale.
/// </remarks>
class Screenshot
{
   internal const string Argument = "--screenshot";

   const double Scale = 2;
   static readonly PixelPoint OffTheScreen = new(-30000, -30000);
   static readonly TimeSpan ForTheImagesToLoad = TimeSpan.FromSeconds(1);

   readonly MainWindow _window;
   readonly IClassicDesktopStyleApplicationLifetime _application;
   readonly string _pngFile;

   Screenshot(MainWindow window, IClassicDesktopStyleApplicationLifetime application, string pngFile)
   {
      _window = window;
      _application = application;
      _pngFile = pngFile;
   }

   ///<summary>The screenshot the command line asks for, if it asks for one: <see cref="Argument"/> followed by the PNG file to
   /// write.</summary>
   internal static Screenshot? AskedForBy(IReadOnlyList<string> arguments, MainWindow window, IClassicDesktopStyleApplicationLifetime application)
   {
      var at = arguments.ToList().IndexOf(Argument);
      if(at < 0) return null;
      if(at + 1 >= arguments.Count) throw new ArgumentException($"{Argument} needs the PNG file to write after it.");

      return new Screenshot(window, application, Path.GetFullPath(arguments[at + 1]));
   }

   internal void TakeWhenTheWindowOpens()
   {
      _window.ShowActivated = false;
      _window.ShowInTaskbar = false;
      _window.WindowStartupLocation = WindowStartupLocation.Manual;
      _window.Position = OffTheScreen;
      _window.Opened += async (_, _) => await Take();
   }

   async Task Take()
   {
      await Task.Delay(ForTheImagesToLoad);
      await Dispatcher.UIThread.InvokeAsync(() => {}, DispatcherPriority.Render);

      var size = _window.ClientSize;
      using var bitmap = new RenderTargetBitmap(new PixelSize((int)(size.Width * Scale), (int)(size.Height * Scale)), new Vector(96 * Scale, 96 * Scale));
      bitmap.Render(_window);
      Directory.CreateDirectory(Path.GetDirectoryName(_pngFile)!);
      bitmap.Save(_pngFile, PngBitmapEncoderOptions.Default);

      Console.WriteLine($"Wrote {_pngFile}");
      _application.Shutdown();
   }
}
