using ATL;
using CSharpFunctionalExtensions;
using ManagedBass;
using ManagedBass.Flac;
using Player.Utils;
using System.Diagnostics;
using Utilities.Platforms;
using Utilities.Zipping;
using ReactiveUI;
using static Utilities.Logging.LoggerBuilder;
using Utilities.Logging;

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
    {
        Log = Loggers["CerddoPod/SAPlayer"];
        
        Log.Info("SAPlayer initialised");
        
        _Init();
    }

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
            Log.Info("Reseting Bass");
            
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
                { Log.FatalThrow($"Unknown platform! {Platformer.GetPlatformStr()}"); return; }
            }

            if (BassHelpers.PluginLoad(out BassFlacHandle, FlacPuginName))
            { Log.Error($"Plugin \"{FlacPuginName}\" could not be loaded! {Bass.LastError}"); }
            else
            { Log.Info($"Plugin \"{FlacPuginName}\" Loaded."); }

            IsInitialised = Bass.Init(-1, 48000, DeviceInitFlags.Stereo);          

            if (!IsInitialised)
            { throw Log.FatalThrow<BassException>($"Bass couldn't initialise! {Bass.LastError}"); }
            else
            { Log.Info("Bass successfully Initialised."); }

            DisposedValue = false;
        }
    }
    #endregion

    #region Setup
    public Result LoadMix(string _Location)
    {
        string Loc = _Location;
        
        Log.Info("Loading mix....");

        if (File.Exists(Loc))
        {
            Log.Info("Preparing to unzip Mix...");
            
            var R = ExtractFiles(Loc);

            if (R.IsFailure)
            { return Log.ErrorResult(R.Error); }

            Loc = R.Value;
        }
        else 
        { Log.Warning("Location doesn't seem to be a file, trying directory..."); }        
        
        if (!Directory.Exists(Loc))
        { return Log.ErrorResult("Location doesn't seem to be a directory either!"); }
        else
        { Log.Info("Appears to be a directory."); }

        return _LoadMix(Loc);
    }

    private Result<string> ExtractFiles(string _Location)
    {
        Log.Info("Extracting files...");
        
        string TempLoc = Directory.CreateTempSubdirectory("Cerddo-Pod").FullName;

        try
        { Zipper.Extract(_Location, TempLoc); }
        catch (Exception EXC)
        { return Log.ErrorResult<string>(EXC.ToString()); }

        return Log.InfoResult<string>(TempLoc);
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
        { return Log.ErrorResult(DefMsg.PlayerDisposed); }

        Result R;

        //Checks if folder exists
        R = FolderChecker(_Location);

        if (R.IsFailure)
        { return R; }

        R = _LoadFiles(_Location);

        if (R.IsFailure)
        { Log.Error(R.Error); return R; }

        return R;
    }

    private static Result FolderChecker(string _Loc)
    {
        //https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing#get-all-matching-files
        if (!Directory.Exists(_Loc))
        { return Log.ErrorResult($"Folder \"{_Loc}\" does not exist!"); }
        else
        { Log.Info("Directory exists!"); }
        
        if (!Directory.GetFiles(_Loc).Any(X => X.EndsWith(METAFILEEXT)))
        { Log.Warning($"No {METAFILEEXT} datafile was present!"); }
        else
        { Log.Info($"{METAFILEEXT} file was found!"); }
        
        return Log.InfoResult($"Directory ready for use.");
    }

    private Result _LoadFiles(string _Loc)
    {
        //https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing#get-all-matching-files
        var Files = Directory.GetFiles(_Loc);
        //TODO: change to List or IEnummerable?
        Queue<string> _Songs = new();
        bool Errors = false;

        Log.Info($"Found {Files.Length} files in mix directory.");

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
                    {
                        _Songs.Enqueue(F);
                        Log.Info($"Song \"{Path.GetFileName(F)}\" loaded.");
                    }
                    else
                    { Log.Warning($"Song \"{Path.GetFileName(F)}\" is of incompatible file type."); }
                    break;
                }
            }
        }
        
        Log.Info($"Of {Files.Length} file(s), {_Songs.Count} song(s) loaded.");

        if (_Songs.Count > 0)
        {
            Result R = LoadSongs(_Songs);

            if (R.IsFailure)
            {
                Log.Error($"\t{R.Error}");
                Errors = true;
            }
        }
        else
        {
            Errors = true;
            Log.Error("\tThere were no (suitable) songs found in mix!");
        }

        return Errors ? Log.ErrorResult("Songs failed to load due to above errors.") : Result.Success();
    }

    private Result LoadSongs(Queue<string> _Songs)
    {
        if (!IsInitialised)
        { return Log.ErrorResult(DefMsg.BassNoInit); }

        SongData Temp;

        while (_Songs.Count > 0)
        {
            string Song = _Songs.Dequeue();

            Temp = new();
            
            Log.Info($"Trying to load \"{Song}\".");
            
            try
            {
                switch (Path.GetExtension(Song).ToLower())
                {
                    case ".flac":
                    { Temp.SoundHandle = BassFlac.CreateStream(Song); break; }
                    default:
                    { Temp.SoundHandle = Bass.CreateStream(Song); break; }
                }

                if (Temp.SoundHandle == 0)
                {
                    Log.Error($"Couldn't load \"{Song}\".");
                    Log.Error($"\tBass error -> {Bass.LastError}");
                }
                
                //Why is this here?
                Thread.Sleep(100);
                Temp.Duration = Bass.ChannelBytes2Seconds(Temp.SoundHandle,
                            Bass.ChannelGetLength(Temp.SoundHandle))
                                    .DblToTS();
            }
            catch (Exception EXC)
            { return Log.ErrorResult(EXC.Message); }

            try
            {
                Log.Info($"Trying to load song info for \"{Song}\".");
                
                Track TSong = new(Song);

                if (TSong.EmbeddedPictures.Count > 0)
                { Temp.CoverImg = Maybe.From(TSong.EmbeddedPictures.First().PictureData.ToList()); }
                else
                { Log.Warning($"No embedded pictures found for {Song}."); }

                Temp.ArtistName = TSong.Artist;
                Temp.SongName = TSong.Title is not null ? TSong.Title : Path.GetFileNameWithoutExtension(Song);
            }
            catch (Exception EXC)
            { return Log.ErrorResult(EXC.Message); }

            Tunes.Add(Temp);
        }

        return Log.InfoResult("Successfully loaded (some) songs.");
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
            Syncer.EndOfSong += SyncerOnEndOfSong;
            Log.Info("Subscribed to song end.");
            EndSubsribed = true;
        }
        
        //Debug.WriteLine($"[Play] EndSubsribed: {EndSubsribed}");
        
        PosRun = true;
        PosRunPause.Set();
        PositionRunner();
    }

    private void SyncerOnEndOfSong(object? _Sender, EventArgs _E)
    {        
        if (EndSubsribed)
        {
            Syncer.EndOfSong -= SyncerOnEndOfSong;
            Log.Info("Unsubsribed from end of song.");
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
        { Log.Error(DefMsg.PlayerDisposed); return false; }
        else if (Tunes.Count == 0)
        { Log.Error(DefMsg.TuneCount); return false; }
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
        Log.Warning("Disposing of SAPlayer!");
        
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
        Log.Warning("Closing!");
        
        Stop();
        
        this.Dispose(true);
        
        Log.Warning($"Disposed and closing program");
    }
    #endregion
}
