using Player;

namespace Cerddo_Pod.ViewModels;

public class MainViewModel : ViewModelBase
{
    public SAPlayer SAP { get; private set; } = new();

    public string ArtistName => SAP.NowPlaying.ArtistName;
    public string TrackTitle => SAP.NowPlaying.SongName;

    //public int Volume
    //{
    //    get => SAP.Volume;
    //    set
    //    {
    //        SAP.Volume = value;
    //        this.RaiseAndSetIfChanged(ref SAP._Volume, value);
    //    }
    //}

    public MainViewModel()
    {
#if DEBUG
        _ = SAP.LoadFiles($"/run/media/tiredaj/AJStore/GitHub/Cerddo_Pod/Src/Cerddo_Pod.Desktop/bin/Debug/net8.0/Assets");
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
