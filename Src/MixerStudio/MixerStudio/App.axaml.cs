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
        _ = Init()
            .NewLogger()
            .UseDefaultLoc()
            .LogName("MixerStudio", "Appearance")
            .BuildAndStore();
        _ = Init()
            .NewLogger()
            .UseDefaultLoc()
            .LogName("MixerStudio", "Music")
            .BuildAndStore();
        _ = Init()
            .NewLogger()
            .UseDefaultLoc()
            .LogName("MixerStudio", "Export")
            .BuildAndStore();
        _ = Init()
            .NewLogger()
            .UseDefaultLoc()
            .LogName("MixerStudio", "Misc")
            .BuildAndStore();
        
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

        base.OnFrameworkInitializationCompleted();
    }
}
