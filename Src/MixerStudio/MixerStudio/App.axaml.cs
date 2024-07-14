using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MixerStudio.ViewModels;
using MixerStudio.Views;
using System.Reflection.Metadata;
using static Utilities.Logging.LoggerBuilder;

namespace MixerStudio;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow { DataContext = new MainViewModel() };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MusicView { DataContext = new MainViewModel() };
        }

        _ = Init()
            .UseDefaultLoc()
            .LogName("MixerStudio", "Appearance")
            .BuildAndStore();
        _ = Init()
            .UseDefaultLoc()
            .LogName("MixerStudio", "Music")
            .BuildAndStore();
        _ = Init()
            .UseDefaultLoc()
            .LogName("MixerStudio", "Export")
            .BuildAndStore();
        _ = Init()
            .UseDefaultLoc()
            .LogName("MixerStudio", "Misc")
            .BuildAndStore();

        base.OnFrameworkInitializationCompleted();
    }
}