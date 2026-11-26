using Avalonia;
using Avalonia.Headless;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace Mavalonia.Markdown.Controls.Measurements;

///<summary>An application whose windows exist only in memory, and are drawn as they would be on a screen.</summary>
class MeasuredApplication : Application
{
   internal static void Start() => AppBuilder.Configure<MeasuredApplication>()
                                             .UseSkia()
                                             .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                                             .WithInterFont()
                                             .SetupWithoutStarting();

   public override void Initialize()
   {
      Styles.Add(new FluentTheme());
      RequestedThemeVariant = ThemeVariant.Light;
   }
}
