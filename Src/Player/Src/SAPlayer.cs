using ATL;

using CSharpFunctionalExtensions;

using ManagedBass;
using ManagedBass.Flac;

using Player.Utils;

using ReactiveUI;

using Utilities.Logging;
using Utilities.Platforms;
using Utilities.Zipping;

using static Utilities.Logging.LoggerBuilder;

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
        get => Tunes.Count == 0 ? SongData.Default : Tunes[CurrentSong];
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
        Log = Loggers["CerddoPod/Backend"];
        
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
            Log.Info("Resetting Bass");
            
            //Frees all streams, not sure if necessary when calling Bass.Free()
            foreach (SongData SD in Tunes)
            { Bass.StreamFree(SD.SoundHandle); }
            
            Bass.Free();
            Bass.PluginFree(BassFlacHandle);
            BassFlacHandle = 0;

            IsInitialised = false;
        }
        
        if (!IsInitialised)
        {
            string FlacPluginName = "";
            
            //Checks the platform to get the right name for the bass flac plugin
            //helps if it's written correctly ffs.
            switch (Platformer.GetPlatform())
            {
                case OSPlat.WINDOWS:
                { FlacPluginName = "libbassflac.dll"; break; }
                case OSPlat.LINUX:
                { FlacPluginName = "bassflac.so"; break; }                    
                case OSPlat.OSX:
                { FlacPluginName = "libbassflac.dylib"; break; }
                case OSPlat.FREEBSD:
                case OSPlat.OTHER:
                default:
                { Log.FatalThrow($"Unknown platform! {Platformer.GetPlatformStr()}"); return; }
            }

            if (BassHelpers.PluginLoad(out BassFlacHandle, FlacPluginName))
            { Log.Error($"Plugin \"{FlacPluginName}\" could not be loaded! {Bass.LastError}"); }
            else
            { Log.Info($"Plugin \"{FlacPluginName}\" Loaded."); }

            IsInitialised = Bass.Init(-1, 48000, DeviceInitFlags.Stereo);          

            if (!IsInitialised)
            { throw Log.FatalThrow<BassException>($"Bass couldn't initialise! {Bass.LastError}"); }
            
            Log.Info("Bass successfully Initialised.");

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
        Log.Info("Appears to be a directory.");

        return _LoadMix(Loc);
    }

    private static Result<string> ExtractFiles(string _Location)
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
    /// operation, and it's nested operations. 
    /// </returns>
    private Result _LoadMix(string _Location)
    {
        //Checks if the object has been disposed, just in case
        if (DisposedValue)
        { return Log.ErrorResult(DefMsg.PLAYER_DISPOSED); }

        //Checks if folder exists
        Result R = FolderChecker(_Location);

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
        if (Directory.Exists(_Loc))
        { Log.Info("Directory exists!"); }
        else
        { return Log.ErrorResult($"Folder \"{_Loc}\" does not exist!"); }

        if (Array.Exists(Directory.GetFiles(_Loc), X => X.EndsWith(METAFILEEXT)))
        { Log.Info($"{METAFILEEXT} file was found!"); }
        else
        { Log.Warning($"No {METAFILEEXT} datafile was present!"); }

        return Log.InfoResult("Directory ready for use.");
    }

    private Result _LoadFiles(string _Loc)
    {
        //https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing#get-all-matching-files
        string[] Files = Directory.GetFiles(_Loc);
        Queue<string> Songs = new();
        bool Errors = false;

        Log.Info($"Found {Files.Length} files in mix directory.");

        foreach (string F in Files)
        {
            string Ext = Path.GetExtension(F);

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
                        Songs.Enqueue(F);
                        Log.Info($"Song \"{Path.GetFileName(F)}\" loaded.");
                    }
                    else
                    { Log.Warning($"Song \"{Path.GetFileName(F)}\" is of incompatible file type."); }
                    break;
                }
            }
        }
        
        Log.Info($"Of {Files.Length} file(s), {Songs.Count} song(s) loaded.");

        if (Songs.Count > 0)
        {
            Result R = LoadSongs(Songs);

            if (!R.IsFailure)
            { return Result.Success(); }
            
            Log.Error($"\t{R.Error}");
            Errors = true;
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
        { return Log.ErrorResult(DefMsg.BASS_NO_INIT); }

        while (_Songs.Count > 0)
        {
            string Song = _Songs.Dequeue();

            SongData Temp = new();
            
            Log.Info($"Trying to load \"{Song}\".");
            
            try
            {
                Temp.SoundHandle = Path.GetExtension(Song).ToLower() switch
                {
                    ".flac" => BassFlac.CreateStream(Song),
                    _ => Bass.CreateStream(Song)
                };

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
                
                Track SongTrackInfo = new(Song);

                if (SongTrackInfo.EmbeddedPictures.Count > 0)
                {
                    Temp.Info = Temp.Info with
                    {
                        CoverImg = Maybe.From(SongTrackInfo.EmbeddedPictures.First().PictureData.ToList())
                    }; }
                else
                { Log.Warning($"No embedded pictures found for {Song}."); }

                Temp.Info = Temp.Info with { ArtistName = SongTrackInfo.Artist };
                Temp.Info = Temp.Info with { SongName = SongTrackInfo.Title ?? Path.GetFileNameWithoutExtension(Song) };
            }
            catch (Exception EXC)
            { return Log.ErrorResult(EXC.Message); }

            Tunes.Add(Temp);
        }

        NowPlaying = Tunes[0];
        
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

    private void Play()
    {
        if (!CheckPlay())
        { return; }

        Position = 0;
        
        Bass.GlobalStreamVolume = _Volume;
        Bass.ChannelPlay(Tunes[CurrentSong].SoundHandle, true);
        
        Syncer.InitSync(Tunes[CurrentSong].SoundHandle);
        
        if (!EndSubscribed)
        {
            Syncer.EndOfSong += SyncerOnEndOfSong;
            Log.Info("Subscribed to song end.");
            EndSubscribed = true;
        }
        
        PosRun = true;
        PosRunPause.Set();
        _ = PositionRunner();
    }

    private void SyncerOnEndOfSong(object? _Sender, EventArgs _E)
    {        
        if (EndSubscribed)
        {
            Syncer.EndOfSong -= SyncerOnEndOfSong;
            Log.Info("Unsubscribed from end of song.");
            EndSubscribed = false;
        }
        
        Skip();
    }

    private void Pause()
    {
        if (!CheckPlay())
        { return; }

        Bass.ChannelPause(Tunes[CurrentSong].SoundHandle);
        PosRunPause.Reset();
    }

    private void Resume()
    {
        if (!CheckPlay())
        { return; }

        Bass.ChannelPlay(Tunes[CurrentSong].SoundHandle);
        PosRunPause.Set();
    }

    private void Stop()
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

        if (Elapsed(Tunes[CurrentSong].SoundHandle) < 25)
        {
            Stop();
            CurrentSong--;
            Play();
        }
        else
        { Bass.ChannelSetPosition(Tunes[CurrentSong].SoundHandle, 0); }
    }

    private void SetPosition(double _NewPosition)
    {
        long BytePos = Bass.ChannelSeconds2Bytes(NowPlaying.SoundHandle, _NewPosition);

        Bass.ChannelSetPosition(NowPlaying.SoundHandle, BytePos);
    }
    #endregion

    #region Misc
    private static double Elapsed(int _Handle)
        => Bass.ChannelBytes2Seconds(_Handle, Bass.ChannelGetPosition(_Handle));

    private bool CheckPlay()
    {
        if (DisposedValue)
        { Log.Error(DefMsg.PLAYER_DISPOSED); return false; }

        switch (Tunes.Count)
        {
            case 0:
                Log.Error(DefMsg.TUNE_COUNT); return false;
            default:
                return true; 
        }
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
    private void Dispose(bool _Disposing)
    {
        Log.Warning("Disposing of SAPlayer!");

        if (DisposedValue)
        { return; }

        if (_Disposing)
        {
            foreach (SongData SD in Tunes)
            { Bass.StreamFree(SD.SoundHandle); }

            Bass.Free();
            Bass.PluginFree(BassFlacHandle);
        }

        DisposedValue = true;
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
        
        Dispose(true);
        
        Log.Warning("Disposed and closing program");
    }
    #endregion
}
