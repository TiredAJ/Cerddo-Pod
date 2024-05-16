using Player;
using ReactiveUI;

namespace Cerddo_Pod.ViewModels;

public class MainViewModel : ViewModelBase
{
    public SAPlayer SAP { get; private set; } = new();

    public MainViewModel()
    {
#if DEBUG
        _ = SAP.LoadFiles($"/run/media/tiredaj/AJStore/GitHub/Cerddo-Pod/Testing/AudioFiles");
#endif
    }

    public MainViewModel(string _Loc)
    { SAP.LoadFiles(_Loc); }

    public void Command_PlayPause()
    { SAP.TogglePause(); }

    public void Command_Rewind()
    { SAP.Rewind(); }

    public void Command_Skip()
    { SAP.Skip(); }
}
