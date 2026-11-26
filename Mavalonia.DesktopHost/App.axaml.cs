using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Mavalonia.DesktopHost.Checks;

namespace Mavalonia.DesktopHost;

///<summary>The application in which a developer tries out the controls of this solution by hand.</summary>
class App : Application
{
   public override void Initialize() => AvaloniaXamlLoader.Load(this);

   public override void OnFrameworkInitializationCompleted()
   {
      if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
      {
         var window = new MainWindow();
         desktop.MainWindow = window;
         if(desktop.Args?.Contains(TypingCheck.Argument) == true) new TypingCheck(window, desktop).RunWhenTheWindowOpens();
         Screenshot.AskedForBy(desktop.Args ?? [], window, desktop)?.TakeWhenTheWindowOpens();
      }

      base.OnFrameworkInitializationCompleted();
   }
}
