using ATL;
using CSharpFunctionalExtensions;
using ManagedBass;
using Player.Utils;
using System.Collections.Immutable;
using System.Diagnostics;
using Utilities;
using System.Reflection;

namespace Player;

public class SAPlayer
{
    #region Public Members
    public const string METAFILEEXT = ".mpdata";
    public ImmutableArray<string> SUPPORTEDFILETYPES = [".wav", ".mp3", ".ogg", ".aiff", ".mp2", ".mp1", ".flac"];
    public int Volume
    {
        get => _Volume;
        set
        {
            _Volume = value;

            Bass.GlobalStreamVolume = _Volume;

            Debug.WriteLine($"Vol: {_Volume}");
        }
    }
    public bool IsInitialised { get; private set; } = false;
    public bool IsPaused { get; private set; } = false;
    public SongData NowPlaying { get => Tunes[CurrentSong]; }
    #endregion

    #region Private Members
    private List<SongData> Tunes = new();
    private int CurrentSong
    {
        get => _CurrentSong;
        set
        {
            if (value >= Tunes.Count)
            { _CurrentSong = 0; }
            else if (value < 0)
            { _CurrentSong = Tunes.Count - 1; }
            else
            { _CurrentSong = value; }
        }
    }
    private int _CurrentSong = 0;

    private Maybe<MPData> MetaData;
    private string FolderLoc = "";
    public int _Volume = 200;
    #endregion

    #region Init
    public SAPlayer()
    { _Init(); }

    public SAPlayer(string _Loc)
    {
        FolderLoc = _Loc;
        _Init();
    }

    private void _Init()
    {
        if (!IsInitialised)
        {
            IsInitialised = Bass.Init(-1, 44100, DeviceInitFlags.Stereo);

            if (!IsInitialised)
            { Debug.WriteLine($"Bass couldn't initialise! {Bass.LastError}"); }
        }
    }
    #endregion

    #region Setup
    public Result LoadFiles()
    { return LoadFiles(FolderLoc); }

    public Result LoadFiles(string _Location)
    {
        Result R;

        R = FolderChecker(_Location);

        if (R.IsFailure)
        { return R; }

        R = _LoadFiles(_Location);

        if (R.IsFailure)
        { return R; }

        return R;
    }

    private Result FolderChecker(string _Loc)
    {
        //Debug.WriteLine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        //Debug.WriteLine(new DirectoryInfo(_Loc).ToString());

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
        Queue<string> _Songs = new();
        bool Errors = false;

        Debug.WriteLine($"Files: {Files.Count()}");

        string Ext = string.Empty, ErMsg = string.Empty;

        foreach (var F in Files)
        {
            Ext = Path.GetExtension(F);

            switch (Ext.ToLower())
            {
                case METAFILEEXT when MetaData.HasNoValue:
                {
                    //MPData.LoadFromFile(F).Match
                    //(
                    //    Val => MetaData = Val,
                    //    ErStr =>
                    //    {
                    //        Logger.PushLog(ErStr);
                    //        ErMsg += $"[{ErStr}] ";
                    //        Errors = true;
                    //    }
                    //);

                    break;
                }
                default:
                {
                    if (SUPPORTEDFILETYPES.Contains(Ext))
                    { _Songs.Enqueue(F); }
                    else
                    { Logger.PushLog($"Song {Path.GetFileName(F)} is of incompatible file type"); }
                    break;
                }
            }
        }

        if (_Songs.Count > 0)
        {
            Result R = LoadSongs(_Songs);

            if (R.IsFailure)
            { ErMsg += R.Error; }
        }
        else
        {
            Errors = true;
            ErMsg += "[There are no (suitable) songs in folder!]";
        }

        return Errors ? Result.Failure(ErMsg) : Result.Success();
    }

    private Result LoadSongs(Queue<string> _Songs)
    {
        if (!IsInitialised)
        { return Result.Failure("ENG is not initialised!"); }

        SongData Temp = new SongData();

        while (_Songs.Count > 0)
        {
            string Song = _Songs.Dequeue();

            try
            {
                Temp.SoundHandle = Bass.CreateStream(Song);
                Console.WriteLine($"Bass Err: {Bass.LastError}");
                Temp.Duration = Bass.ChannelBytes2Seconds(Temp.SoundHandle,
                            Bass.ChannelGetLength(Temp.SoundHandle))
                                    .DblToTS();
            }
            catch (Exception EXC)
            { return Result.Failure(EXC.Message); }

            try
            {
                Track TSong = new Track(Song);

                if (TSong.EmbeddedPictures.Count > 0)
                { Temp.CoverImg = Maybe.From(TSong.EmbeddedPictures.First().PictureData); }

                Temp.ArtistName = TSong.Artist;
                Temp.SongName = TSong.Title is not null ? TSong.Title : Path.GetFileNameWithoutExtension(Song);
            }
            catch (Exception EXC)
            { return Result.Failure(EXC.Message); }

            Tunes.Add(Temp);
        }

        return Result.Success();
    }
    #endregion

    #region Media Controls
    /// <summary>
    /// Starts if stopped, resumes if paused or pauses if playing
    /// </summary>
    public void TogglePause()
    {
        if (!CheckSongs())
        { return; }

        switch (Bass.ChannelIsActive(Tunes[CurrentSong].SoundHandle))
        {
            case PlaybackState.Playing:
            { Pause(); break; }
            case PlaybackState.Paused:
            { Resume(); break; }
            case PlaybackState.Stopped:
            case PlaybackState.Stalled:
            default:
            { Play(); break; }
        }
    }

    public void Play()
    {
        if (!CheckSongs())
        { return; }
        Bass.GlobalStreamVolume = _Volume;
        Bass.ChannelPlay(Tunes[CurrentSong].SoundHandle, true);
    }

    public void Pause()
    {
        if (!CheckSongs())
        { return; }
        
        Bass.ChannelPause(Tunes[CurrentSong].SoundHandle); 
    }

    public void Resume()
    {
        if (!CheckSongs())
        { return; }
        
        Bass.ChannelPlay(Tunes[CurrentSong].SoundHandle, false); 
    }

    public void Stop()
    {
        if (!CheckSongs())
        { return; }

        Bass.ChannelStop(Tunes[CurrentSong].SoundHandle); 
    }

    public void Skip()
    {
        if (!CheckSongs())
        { return; }

        Stop();
        CurrentSong++;
        Play();
    }

    public void Rewind()
    {
        if (!CheckSongs())
        { return; }
        else if (Elapsed(Tunes[CurrentSong].SoundHandle) < 25)
        {
            Stop();
            CurrentSong--;
            Play();
        }
        else
        { Bass.ChannelSetPosition(Tunes[CurrentSong].SoundHandle, 0); }
    }
    #endregion

    #region Misc
    private double Elapsed(int _Handle)
        => Bass.ChannelBytes2Seconds(_Handle, Bass.ChannelGetPosition(_Handle));

    private bool CheckSongs()
        => Tunes.Count > 0 ? true : false;

    #endregion
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

public struct SongData
{
    public int SoundHandle;
    public string SongName, ArtistName;
    public Maybe<byte[]> CoverImg;
    public TimeSpan Duration;
}