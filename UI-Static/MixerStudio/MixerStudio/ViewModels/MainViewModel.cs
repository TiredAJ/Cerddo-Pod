using Avalonia.Controls;
using MixerStudio.Views;
using ReactiveUI;
using ReactiveUI.Fody;
using ReactiveUI.Fody.Helpers;
using System.Reflection.Emit;

namespace MixerStudio.ViewModels;

public class MainViewModel : ViewModelBase
{    
    private AppearanceViewModel APView { get; set; }
    private MusicViewModel MView { get; set; }
    private ExportViewModel EXView { get; set; }

    [Reactive]
    public ViewModelBase CenterContent { get; set; }
    
    public MainViewModel()
    {
        MView = new ();
        APView = new ();
        EXView = new ();

        CenterContent = MView;
    }

    public void Command_MusicView()
    { CenterContent = MView; }

    public void Command_AppearanceView()
    { CenterContent = APView; }

    public void Command_ExportView()
    { CenterContent = EXView; }
}