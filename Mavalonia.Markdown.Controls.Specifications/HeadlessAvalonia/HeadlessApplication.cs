using Avalonia;
using Avalonia.Headless;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

///<summary>The Avalonia application that specifications run in. It has no desktop: its windows exist only in memory.</summary>
class HeadlessApplication : Application
{
   public override void Initialize()
   {
      Styles.Add(new FluentTheme());
      RequestedThemeVariant = ThemeVariant.Light;
   }

   ///<summary>The headless platform finds this method by its name.</summary>
   // ReSharper disable once UnusedMember.Global
   public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<HeadlessApplication>()
                                                            .UseSkia()
                                                            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                                                            .WithInterFont();
}
