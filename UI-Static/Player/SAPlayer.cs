using ATL;
using CSharpFunctionalExtensions;
using ManagedBass;
using ManagedBass.Flac;
using Player.Utils;
using System.Diagnostics;
using Utilities;
using ReactiveUI;

namespace Player;

public class SAPlayer : PlayerBase, IDisposable
{
    #region Public Properties
    /// <summary>
    /// Property for setting Bass' <see cref="Bass.GlobalStreamVolume"/>
    /// </summary>
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
    /// <summary>
    /// Keeps up with the currently playing song
    /// </summary>
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
    /// <summary>
    /// Position of <see cref="NowPlaying"/>
    /// </summary>
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
    /// <summary>
    /// Holds the index of the currently played
    /// song from <see cref="PlayerBase.Tunes"/>
    /// </summary>
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
    #endregion

    #region Private
    /// <summary>
    /// Metadata for the whole application
    /// </summary>
    private Maybe<MPData> MetaData = Maybe<MPData>.None;
    #endregion
    
    #region Init
    public SAPlayer()
    { _Init(); }

    public SAPlayer(string _Loc)
    {
        FolderLoc = _Loc;
        _Init();
    }

    /// <summary>
    /// Contained initialiser, ensures Bass is freed and reset, then initialised for use
    /// </summary>
    private void _Init()
    {
        if (IsInitialised)
        {
            //Frees all streams, not sure if necessary when calling Bass.Free()
            foreach (var SD in Tunes)
            { Bass.StreamFree(SD.SoundHandle); }
            
            Bass.Free();
            Bass.PluginFree(BassFlacHandle);
            BassFlacHandle = 0;

            IsInitialised = false;
        }
        
        if (!IsInitialised)
        {
            string FlacPuginName = "";
            
            //Checks the platform to get the right name for the bass flac plugin
            switch (Platformer.GetPlatform())
            {
                case OSPlat.Windows:
                { FlacPuginName = "libbassflac.so"; break; }
                case OSPlat.Linux:
                { FlacPuginName = "bassflac.dll"; break; }                    
                case OSPlat.OSX:
                { FlacPuginName = "libbassflac.dylib"; break; }
                case OSPlat.Other:
                {Logger.LogThrow($"Unknown platform! {Platformer.GetPlatformStr()}"); return; }
            }

            if (BassHelpers.PluginLoad(out BassFlacHandle, FlacPuginName))
            { Logger.Log($"Plugin \"{FlacPuginName}\" could not be loaded! {Bass.LastError}"); }

            IsInitialised = Bass.Init(-1, 48000, DeviceInitFlags.Stereo);          

            if (!IsInitialised)
            { throw Logger.LogThrow($"Bass couldn't initialise! {Bass.LastError}"); }

            DisposedValue = false;
        }
    }
    #endregion

    #region Setup
    public Result LoadMix(string _Location)
    {
        string Loc = _Location;

        if (!File.Exists(Loc))
        { Logger.Log("Location doesn't seem to be a file, trying directory..."); }
        else if (!Directory.Exists(Loc))
        { return Logger.LogResult("Location doesn't seem to be a folder either!"); }
        else
        {
            var R = ExtractFiles(Loc);

            if (R.IsFailure)
            { return Logger.LogResult(R.Error); }

            Loc = R.Value;
        }

        return _LoadMix(Loc);
    }

    private Result<string> ExtractFiles(string _Location)
    {
        string TempLoc = Directory.CreateTempSubdirectory("Cerddo-Pod").FullName;

        try
        { Zipper.Extract(_Location, TempLoc); }
        catch (Exception EXC)
        { return Logger.LogResult<string>(EXC.ToString()); }

        return Result.Success(TempLoc);
    }
    
    /// <summary>
    /// Loads files from a specified *Folder/Directory*
    /// </summary>
    /// <param name="_Location">The path to the folder</param>
    /// <returns>
    /// A <see cref="Result"/> determining the outcome of the
    /// operation and it's nested operations. 
    /// </returns>
    private Result _LoadMix(string _Location)
    {
        //Checks if the object has been disposed, just in case
        if (DisposedValue)
        { return Logger.LogResult(DefMsg.PlayerDisposed); }

        Result R;

        //Checks if folder exists
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
        { Logger.Log($"No {METAFILEEXT} datafile was present!"); }
        
        return Result.Success();
    }

    private Result _LoadFiles(string _Loc)
    {
        //https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing#get-all-matching-files
        var Files = Directory.GetFiles(_Loc);
        //TODO: change to List or IEnummerable?
        Queue<string> _Songs = new();
        bool Errors = false;

        Logger.Log($"Found {Files.Length} files in mix");

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
        { Logger.Log(DefMsg.PlayerDisposed); return false; }
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
                Bass.PluginFree(BassFlacHandle);
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

    public void Closing()
    {
        Stop();
        
        this.Dispose(true);
        
        Logger.Log($"Disposing and closing program");
    }
    #endregion
}