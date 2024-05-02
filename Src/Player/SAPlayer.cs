using ATL;
using CSharpFunctionalExtensions;
using System.Collections.Immutable;
using Utilities;

namespace Player;

public class SAPlayer
{
    #region Public Members
    public const string METAFILEEXT = ".mpdata";
    public ImmutableArray<string> SUPPORTEDFILETYPES = [".wav", ".mp3", ".ogg"];
    public float Volume
    {
        get => _Volume;
        set
        {
            _Volume = value;

            Task.Run(() =>
            {
                foreach (var mSD in Tunes)
                { mSD.Sound.Volume = _Volume; }
            });
        }
    }

    public bool IsInitialised { get => _IsInitialised; }
    public bool IsPaused { get => _IsPaused; }
    #endregion

    #region Private Members
    private readonly Maybe<AudioEngine> ENG;
    private List<SongData> Tunes;
    private int CurrentSong = -1;
    private Maybe<MPData> MetaData;
    private Queue<string> Songs = new();
    private Maybe<IEnumerable<string>> InitSongs;
    private float _Volume = 0.0125f;
    private bool _IsInitialised = false, _IsPaused = false;
    #endregion

    public SAPlayer()
    { ENG = AudioEngine.CreateDefault(); }

    #region Setup
    public Result LoadFiles(string _Location)
    {
        Result R;

        R = FolderChecker(_Location);

        if (R.IsFailure)
        { return R; }

        R = _LoadFiles(_Location);

        if (R.IsFailure)
        { return R; }

        R = LoadSongs();

        if (R.IsSuccess)
        { IsInitialised = true; }

        return R;
    }

    private Result FolderChecker(string _Loc)
    {
        if (!Directory.Exists(_Loc))
        { return Result.Failure($"Folder [{_Loc}] does not exist!"); }
        else if (!Directory.GetFiles(_Loc).Any(X => X.EndsWith(METAFILEEXT)))
        { return Result.Failure($"No {METAFILEEXT} datafile was present!"); }
        else
        { return Result.Success(); }
    }

    private Result _LoadFiles(string _Loc)
    {
        var Files = Directory.GetFiles(_Loc);
        List<string> _Songs = new();
        bool Errors = false;

        string Ext = string.Empty, ErMsg = string.Empty;

        foreach (var F in Files)
        {
            Ext = Path.GetExtension(F);

            switch (Ext.ToLower())
            {
                case METAFILEEXT when MetaData.HasNoValue:
                {
                    MPData.LoadFromFile(F).Match
                    (
                        Val => MetaData = Val,
                        ErStr =>
                        {
                            Logger.PushLog(ErStr);
                            ErMsg += $"[{ErStr}] ";
                            Errors = true;
                        }
                    );

                    break;
                }
                default:
                {
                    if (SUPPORTEDFILETYPES.Contains(Ext))
                    { _Songs.Add(F); }
                    else
                    {
                        Errors = true;
                        Logger.PushLog($"Song {Path.GetFileName(F)} is of incompatible file type");
                        ErMsg += $"Song {Path.GetFileName(F)} is of incompatible file type";
                    }
                    break;
                }
            }
        }

        if (_Songs.Count > 0)
        {
            InitSongs = _Songs;
            Songs = new Queue<string>(InitSongs.Value);
        }
        else
        {
            Errors = true;
            ErMsg += "[There are no (suitable) songs in folder!]";
        }


        return Errors ? Result.Failure(ErMsg) : Result.Success();
    }

    private Result LoadSongs()
    {
        if (ENG.HasNoValue)
        { return Result.Failure("ENG is not initialised!"); }

        SongData Temp = new SongData();

        while (Songs.Count > 0)
        {
            Stream FSong = File.OpenRead(Songs.Dequeue());

            try
            { Temp.Sound = new SoundStream(FSong, ENG.Value); }
            catch (Exception EXC)
            { return Result.Failure(EXC.Message); }

            Temp.Sound.PropertyChanged += Song_PropertyChanged;
            Temp.Sound.Volume = Volume;

            try
            { Temp.CoverImg = Maybe.From(new Track(FSong).EmbeddedPictures.First().PictureData); }
            catch (Exception EXC)
            { return Result.Failure(EXC.Message); }

            Tunes.Add(Temp);
        }

        return Result.Success();
    }
    #endregion

    #region Media Controls
    public void TogglePause()
    {
        if (_IsPaused)
        { Play(); }
        else
        { Pause(); }
    }

    public void Pause()
    {

    }

    public void Play()
    { }

    public void Stop()
    { }

    public void Skip()
    { }

    public void Rewind()
    { }
    #endregion


    private void Song_PropertyChanged(object? _Sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Position")
        { return; }

        SoundStream Sound = _Sender as SoundStream;

        Console.WriteLine(Sound.Volume);
    }
}

struct MPData
{
    public MPData()
    {
    }

    public static Result<MPData> LoadFromFile(string _F)
    {
        return Result.Failure<MPData>("Not implemented!");
    }
}

struct SongData
{
    public SoundStream Sound;
    public Maybe<byte[]> CoverImg;
}