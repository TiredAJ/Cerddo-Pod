using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Player;
using ReactiveUI;
using System.Linq;
using Utilities;

namespace Cerddo_Pod.ViewModels;

public class MainViewModel : ViewModelBase
{
    private static FolderPickerOpenOptions FPOO = new FolderPickerOpenOptions()
    { Title = "Mix folder to open", AllowMultiple = false };
    
    public SAPlayer SAP { get; private set; } = new();

    public MainViewModel()
    { }

    public MainViewModel(string _Loc)
    { SAP.LoadMix(_Loc); }
    
    public void Command_PlayPause()
    { SAP.TogglePause(); }

    public void Command_Rewind()
    { SAP.Rewind(); }

    public void Command_Skip()
    { SAP.Skip(); }

    public async void Command_OpenFolder(object? _Sender)
    {
        if (_Sender is null || _Sender is not Avalonia.Controls.Control)
        { return; }

        var ISP = TopLevel.GetTopLevel(_Sender as Button)?.StorageProvider;

        if (ISP is null)
        { Logger.Log("ISP returned null!"); }
        
        FPOO.SuggestedStartLocation = await ISP.TryGetWellKnownFolderAsync(WellKnownFolder.Downloads);
        
        if (!ISP.CanOpen)
        { Logger.Log("Not allowed to open folders!"); return; }

        var FolderLoc = await ISP.OpenFolderPickerAsync(FPOO);

        if (FolderLoc.Count > 0 &&  FolderLoc.First().TryGetLocalPath() is { } LP)
        { SAP.LoadMix(LP); }
        else
        { Logger.Log("FolderLoc count error!"); }        
    }

    public void Closing()
    { SAP.Closing(); }
}
