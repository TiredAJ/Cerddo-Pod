using ATL;
using CSharpFunctionalExtensions;
using ManagedBass;
using ManagedBass.Flac;
using Player.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Immutable;
using System.Diagnostics;
using Utilities;
using Avalonia.Media.Imaging;

namespace Player;

//ToDo
// - Normalise audio -> https://www.izotope.com/en/learn/audio-normalization.html



public class SAPlayer : ReactiveObject, IDisposable
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

            this.RaiseAndSetIfChanged(ref _Volume, value);
        }
    }
    public bool IsInitialised { get; private set; } = false;
    public bool IsPaused { get; private set; } = false;
    public SongData NowPlaying
    {
        get
        {
            if (Tunes.Count == 0)
            { return SongData.Default; }
            else
            { return Tunes[CurrentSong]; }
        }
        private set => this.RaiseAndSetIfChanged(ref _NowPlaying, value);
    }
    public double Position
    {
        get => _Position;
        set
        {
            if (!value.Equals(_ParPosition))
            {
                this.RaiseAndSetIfChanged(ref _Position, value);
                SetPosition(value);
                return;
            }
            
            this.RaiseAndSetIfChanged(ref _Position, value);
        }
    }
    #endregion

    #region Private Members
    private List<SongData> Tunes = [];
    private SongData _NowPlaying = SongData.Default;
    private double _Position = 0d;
    private double _ParPosition = 0d;
    public int CurrentSong
    {
        get => _CurrentSong;
        private set
        {
            if (value >= Tunes.Count)
            { _CurrentSong = 0; }
            else if (value < 0)
            { _CurrentSong = Tunes.Count - 1; }
            else
            { _CurrentSong = value; }

            NowPlaying = Tunes[_CurrentSong];
        }
    }
    private int _CurrentSong = 0;

    private Maybe<MPData> MetaData = Maybe<MPData>.None;
    private string FolderLoc = "";
    public int _Volume = 200;
    private bool DisposedValue, PosRun, EndSubsribed = false;
    private ManualResetEventSlim PosRunPause = new ManualResetEventSlim(true);    
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
            if (Bass.PluginLoad("libbassflac.so") == 0)
            { Debug.WriteLine($"Libflac could not be loaded! {Bass.LastError}"); }
            
            Bass.PluginLoad("libbassflac.so");
            
            IsInitialised = Bass.Init(-1, 44100, DeviceInitFlags.Stereo);

            if (!IsInitialised)
            { Logger.Log($"Bass couldn't initialise! {Bass.LastError}"); }

            DisposedValue = false;
        }
    }
    #endregion

    #region Setup
    #if Debug
    public Result LoadFiles()
    { return LoadFiles(FolderLoc); }
    #endif

    public Result LoadFiles(string _Location)
    {
        if (DisposedValue)
        { return Logger.LogResult(DefMsg.BassDispose); }

        Result R;

        R = FolderChecker(_Location);

        if (R.IsFailure)
        { return R; }

        R = _LoadFiles(_Location);

        if (R.IsFailure)
        { return R; }

        return R;
    }

    private static Result FolderChecker(string _Loc)
    {
        //https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing#get-all-matching-files
        if (!Directory.Exists(_Loc))
        { return Logger.LogResult($"Folder \"{_Loc}\" does not exist!"); }
        else if (!Directory.GetFiles(_Loc).Any(X => X.EndsWith(METAFILEEXT)))
        { return Logger.LogResult($"No {METAFILEEXT} datafile was present!"); }
        else
        { return Result.Success(); }
    }

    private Result _LoadFiles(string _Loc)
    {
        //https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing#get-all-matching-files
        var Files = Directory.GetFiles(_Loc);
        Queue<string> _Songs = new();
        bool Errors = false;

        Debug.WriteLine($"Files: {Files.Length}");

        string Ext, ErMsg = string.Empty;

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
                    //        Logger.Log(ErStr);
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
                    { Logger.Log($"Song \"{Path.GetFileName(F)}\" is of incompatible file type"); }
                    break;
                }
            }
        }

        if (_Songs.Count > 0)
        {
            Result R = LoadSongs(_Songs);

            if (R.IsFailure)
            { ErMsg += R.Error; }
            #if DEBUG
            else
            { Tunes = Tunes.Shuffle(); }
            #endif
        }
        else
        {
            Errors = true;
            ErMsg += "[There are no (suitable) songs in folder!]";
        }

        return Errors ? Logger.LogResult(ErMsg) : Result.Success();
    }

    private Result LoadSongs(Queue<string> _Songs)
    {
        if (!IsInitialised)
        { return Logger.LogResult(DefMsg.BassNoInit); }

        SongData Temp;

        while (_Songs.Count > 0)
        {
            string Song = _Songs.Dequeue();

            Temp = new();
            
            try
            {
                switch (Path.GetExtension(Song).ToLower())
                {
                    case ".flac":
                    {
                        Temp.SoundHandle = BassFlac.CreateStream(Song);
                        break;
                    }
                    default:
                    {
                        Temp.SoundHandle = Bass.CreateStream(Song);
                        break;
                    }
                }

                if (Temp.SoundHandle == 0)
                {
                    Debug.WriteLine($"Could not load {Song}!"); 
                    Debug.WriteLine($"Create Stream: {Song} -> {Bass.LastError}");
                    Debug.WriteLine($"Duration: {Bass.ChannelGetLength(Temp.SoundHandle)}");
                }
                
                Thread.Sleep(100);
                Temp.Duration = Bass.ChannelBytes2Seconds(Temp.SoundHandle,
                            Bass.ChannelGetLength(Temp.SoundHandle))
                                    .DblToTS();
            }
            catch (Exception EXC)
            { return Logger.LogResult(EXC.Message); }

            try
            {
                Track TSong = new(Song);

                if (TSong.EmbeddedPictures.Count > 0)
                { Temp.CoverImg = Maybe.From(TSong.EmbeddedPictures.First().PictureData.ToList()); }

                Temp.ArtistName = TSong.Artist;
                Temp.SongName = TSong.Title is not null ? TSong.Title : Path.GetFileNameWithoutExtension(Song);
            }
            catch (Exception EXC)
            { return Logger.LogResult(EXC.Message); }

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
        if (!CheckPlay())
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
        if (!CheckPlay())
        { return; }

        Position = 0;
        
        //Debug.WriteLine($"[Play] Current Index: {CurrentSong}");
        
        Bass.GlobalStreamVolume = _Volume;
        Bass.ChannelPlay(Tunes[CurrentSong].SoundHandle, true);
        
        Syncer.InitSync(Tunes[CurrentSong].SoundHandle);
        
        if (!EndSubsribed)
        {
            Syncer.EndOfSong += SyncerOnEndOfSong;;
            EndSubsribed = true;
        }
        
        Debug.WriteLine($"[Play] EndSubsribed: {EndSubsribed}");
        
        PosRun = true;
        PosRunPause.Set();
        PositionRunner();
    }

    private void SyncerOnEndOfSong(object? _Sender, EventArgs _E)
    {
        
        if (EndSubsribed)
        {
            Syncer.EndOfSong -= SyncerOnEndOfSong;
            EndSubsribed = false;
        }
        
        Skip();
    }

    public void Pause()
    {
        if (!CheckPlay())
        { return; }

        Bass.ChannelPause(Tunes[CurrentSong].SoundHandle);
        PosRunPause.Reset();
    }

    public void Resume()
    {
        if (!CheckPlay())
        { return; }

        Bass.ChannelPlay(Tunes[CurrentSong].SoundHandle, false);
        PosRunPause.Set();
    }

    public void Stop()
    {
        if (!CheckPlay())
        { return; }

        Bass.ChannelStop(Tunes[CurrentSong].SoundHandle);
        PosRun = false;
        PosRunPause.Set();
    }

    public void Skip()
    {
        if (!CheckPlay())
        { return; }

        Stop();
        CurrentSong++;
        Play();
    }

    public void Rewind()
    {
        if (!CheckPlay())
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

    public void SetPosition(double _Position)
    {
        var BytePos = Bass.ChannelSeconds2Bytes(NowPlaying.SoundHandle, _Position);

        Bass.ChannelSetPosition(NowPlaying.SoundHandle, BytePos, PositionFlags.Bytes);
    }
    #endregion

    #region Misc
    private static double Elapsed(int _Handle)
        => Bass.ChannelBytes2Seconds(_Handle, Bass.ChannelGetPosition(_Handle));

    private bool CheckPlay()
    {
        if (DisposedValue)
        { Logger.Log(DefMsg.BassDispose); return false; }
        else if (Tunes.Count == 0)
        { Logger.Log(DefMsg.TuneCount); return false; }
        else
        { return true; }
    }

    private async Task PositionRunner()
    {
        await Task.Run(() =>
        {
            do
            {
                PosRunPause.Wait();
                _ParPosition = NowPlaying.SoundHandle.ChannelPos();
                Position = _ParPosition;
                Thread.Sleep(250);
            } while (PosRun);
        });
    }
    #endregion

    #region Disposal
    protected virtual void Dispose(bool _Disposing)
    {
        if (!DisposedValue)
        {
            if (_Disposing)
            {
                foreach (var SD in Tunes)
                { Bass.StreamFree(SD.SoundHandle); }

                Bass.Free();
            }

            DisposedValue = true;
        }
    }

    ~SAPlayer()
    { Dispose(false); }

    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}

struct MPData
{
    public MPData()
    {
    }

    public static Result<MPData> LoadFromFile(string _F)
    {
        return Logger.LogResult<MPData>("Not implemented!");
    }
}

public struct SongData
{
    public int SoundHandle;
    public string SongName;
    public string ArtistName;
    public Maybe<List<byte>> CoverImg;
    public TimeSpan Duration;

    public static SongData Default = new SongData()
    {
        SoundHandle = 0,
        ArtistName = "Nessie",
        SongName = "The Loch",
        Duration = TimeSpan.MaxValue,
        CoverImg = new Maybe<List<byte>>()
    };
}

public class Syncer
{
    public static event EventHandler? EndOfSong;
    private static SyncProcedure IntSyncer = new SyncProcedure(SyncProc);

    public static void InitSync(int _Handle)
    { Bass.ChannelSetSync(_Handle, SyncFlags.End | SyncFlags.Mixtime, 0, IntSyncer); }

    private static void SyncProc(int _Handle, int _Channel, int _Data, IntPtr _User)
    { EndOfSong?.Invoke(null, EventArgs.Empty); }
}