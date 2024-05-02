using Player;

namespace MP3_Pod.ViewModels;

public class MainViewModel : ViewModelBase
{
    private SAPlayer SAP = new SAPlayer();

    public MainViewModel()
    { SAP.LoadFiles("/Assets"); }

    public MainViewModel(string _Loc)
    { SAP.LoadFiles(_Loc); }

    public void Command_PlayPause()
    { SAP.to }

    public void Command_Rewind()
    { }

    public void Command_Skip()
    { }
}
