using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System.Collections.Generic;
using ReactiveUI.Fody.Helpers;
using System.Threading.Tasks;
using Utilities.Logging;
using Common.Appearance;
using MixerStudio.Utils;
using System.Diagnostics;

namespace MixerStudio.ViewModels;

public class AppearanceViewModel : ViewModelBase
{
    private Dictionary<string, ControlSpecs> IntControls = new();
    private Logger Log;

    [Reactive]
    public (Control, ControlSpecs) Current { get; set; }
    
    public AppearanceViewModel()
    { Log = LoggerBuilder.Loggers["MixerStudio/Appearance"]; }

    public void Command_InitControls(Panel _Parent)
    { _ = InitControls(_Parent); }

    private async Task InitControls(Panel _Parent)
    {
        Debug.WriteLine($"Panel name: {_Parent.Name}");
        
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await _InitControls(_Parent); 
            Debug.WriteLine($"{IntControls.Count} Total controls found");
        });
    }

    private async Task _InitControls(Panel _Panel)
    {
        ControlSpecs CS;
        
        foreach (var C in _Panel.Children)
        {
            CS = new();

            Debug.WriteLine($"{_Panel.Children.Count} Controls found");
            
            if (C is Panel P)
            { await _InitControls(P); }
            
            if (C.Name is { } Name)
            {
                if (!IntControls.TryAdd(Name, new ControlSpecs()))
                { Log.Error($"Duplicate control name found: [{C.Name}]"); }
                else
                {
                    if (C.GetType().GetProperty("Background") is ISolidColorBrush ISCB_bg)
                    { CS.Background = ISCB_bg.Color.AvColToCol(); }

                    if (C.GetType().GetProperty("Foreground") is ISolidColorBrush ISCB_fg)
                    { CS.Foreground = ISCB_fg.Color.AvColToCol(); }

                    IntControls[Name] = CS;
                }
            }
            else
            { Log.Error($"Control [{C.GetType().FullName}] doesn't have a name!"); }
        }
    }

    public void GetControlSpecs(Control _Control)
    {
        if (_Control.Name is not null)
        { Current = (_Control, IntControls[_Control.Name]); }        
    }
}

