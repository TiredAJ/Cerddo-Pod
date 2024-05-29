using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using ReactiveUI.Fody.Helpers;
using Utilities.Logging;

namespace MixerStudio.ViewModels;

public class AppearanceViewModel : ViewModelBase
{
    private Dictionary<string, ControlSpecs> IntControls = new();

    [Reactive]
    public (Control, ControlSpecs) Current { get; set; }
    
    public void InitControls(Panel _Parent)
    {
        ControlSpecs CS = new();
        
        foreach (var C in _Parent.Children)
        {
            CS = new();
            
            if (C.Name is string Name)
            {
                if (!IntControls.TryAdd(Name, new ControlSpecs()))
                { Logger.Log($"Duplicate control name found: [{C.Name}]"); }
                else
                {
                    if (C.GetType().GetProperty("Background") is ISolidColorBrush ISCB_bg)
                    { CS.Background = ISCB_bg.Color; }

                    if (C.GetType().GetProperty("Foreground") is ISolidColorBrush ISCB_fg)
                    { CS.Foreground = ISCB_fg.Color; }
                }
            }
            else
            { Logger.Log($"Control [{C.GetType().FullName}] doesn't have a name!"); }
        }
    }

    public void GetControlSpecs(Control _Control)
    {
        if (_Control.Name is not null)
        { Current = (_Control, IntControls[_Control.Name]); }        
    }
}

public struct ControlSpecs
{
    public Maybe<FontFamily> Font;
    public Maybe<float> FontSize;
    public Maybe<Color> Foreground;
    public Maybe<Color> Background;
}