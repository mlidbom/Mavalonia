using Avalonia;

namespace Mavalonia.DesktopHost;

static class Program
{
   [STAThread]
   public static void Main(string[] arguments) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(arguments);

   ///<summary>The XAML previewer finds this method by its name.</summary>
   // ReSharper disable once MemberCanBePrivate.Global
   public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
                                                            .UsePlatformDetect()
                                                            .WithInterFont()
                                                            .LogToTrace();
}
