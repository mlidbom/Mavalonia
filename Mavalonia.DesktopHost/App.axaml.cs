using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Mavalonia.DesktopHost;

///<summary>The application in which a developer tries out the controls of this solution by hand.</summary>
class App : Application
{
   public override void Initialize() => AvaloniaXamlLoader.Load(this);

   public override void OnFrameworkInitializationCompleted()
   {
      if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
         desktop.MainWindow = new MainWindow();

      base.OnFrameworkInitializationCompleted();
   }
}
