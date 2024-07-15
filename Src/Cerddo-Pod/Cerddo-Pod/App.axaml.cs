using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Cerddo_Pod.ViewModels;
using Cerddo_Pod.Views;
using static Utilities.Logging.LoggerBuilder;

namespace Cerddo_Pod;

public partial class App : Application
{
    public override void Initialize()
    {
        Init()
            .UseDefaultLoc()
            .LogName("CerddoPod", "SAPlayer")
            .Store();
        Init()
            .UseDefaultLoc()
            .LogName("CerddoPod", "MPData")
            .Store();
        Init()
            .UseDefaultLoc()
            .LogName("CerddoPod", "UI")
            .Store();
        Init()
            .UseDefaultLoc()
            .LogName("CerddoPod", "Misc")
            .Store();
        
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}
