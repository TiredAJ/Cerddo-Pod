using Player;
using ReactiveUI;
using System.IO;
using CSharpFunctionalExtensions;

namespace MP3_Pod.ViewModels;

public class MainViewModel : ViewModelBase
{
    private SAPlayer SAP = new SAPlayer();

    public string ArtistName => SAP.NowPlaying.ArtistName;
    public string TrackTitle => SAP.NowPlaying.SongName;

    public int Volume
    {
        get => SAP.Volume;
        set
        {
            SAP.Volume = value;
            this.RaiseAndSetIfChanged(ref SAP._Volume, value);
        }
    }

    public MainViewModel()
    { SAP.LoadFiles($"Assets{Path.DirectorySeparatorChar}"); }

    public MainViewModel(string _Loc)
    { SAP.LoadFiles(_Loc); }

    public void Command_PlayPause()
    { SAP.TogglePause(); }

    public void Command_Rewind()
    { SAP.Rewind(); }

    public void Command_Skip()
    { SAP.Skip(); }
}
