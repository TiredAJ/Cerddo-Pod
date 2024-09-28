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
        get => Tunes.Count == 0 ? SongData.DEFAULT : Tunes[CurrentSong];
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
        LOG.Info("SAPlayer initialised");
        
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
            LOG.Info("Resetting Bass");
            
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
                { LOG.FatalThrow($"Unknown platform! {Platformer.GetPlatformStr()}"); return; }
            }

            if (BassHelpers.PluginLoad(out BassFlacHandle, FlacPluginName))
            { LOG.Error($"Plugin \"{FlacPluginName}\" could not be loaded! {Bass.LastError}"); }
            else
            { LOG.Info($"Plugin \"{FlacPluginName}\" Loaded."); }

            IsInitialised = Bass.Init(-1, 48000, DeviceInitFlags.Stereo);          

            if (!IsInitialised)
            { throw LOG.FatalThrow<BassException>($"Bass couldn't initialise! {Bass.LastError}"); }
            
            LOG.Info("Bass successfully Initialised.");

            DisposedValue = false;
        }
    }
    #endregion

    #region Setup
    public Result LoadMix(string _Location)
    {
        string Loc = _Location;
        
        LOG.Info("Loading mix....");

        if (File.Exists(Loc))
        {
            LOG.Info("Preparing to unzip Mix...");
            
            var R = ExtractFiles(Loc);

            if (R.IsFailure)
            { return LOG.ErrorResult(R.Error); }

            Loc = R.Value;
        }
        else 
        { LOG.Warning("Location doesn't seem to be a file, trying directory..."); }        
        
        if (!Directory.Exists(Loc))
        { return LOG.ErrorResult("Location doesn't seem to be a directory either!"); }
        LOG.Info("Appears to be a directory.");

        return _LoadMix(Loc);
    }

    private static Result<string> ExtractFiles(string _Location)
    {
        LOG.Info("Extracting files...");
        
        string TempLoc = Directory.CreateTempSubdirectory("Cerddo-Pod").FullName;

        try
        { Zipper.Extract(_Location, TempLoc); }
        catch (Exception EXC)
        { return LOG.ErrorResult<string>(EXC.ToString()); }

        return LOG.InfoResult<string>(TempLoc);
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
        { return LOG.ErrorResult(DefMsg.PLAYER_DISPOSED); }

        //Checks if folder exists
        Result R = FolderChecker(_Location);

        if (R.IsFailure)
        { return R; }

        R = _LoadFiles(_Location);

        if (R.IsFailure)
        { LOG.Error(R.Error); return R; }

        return R;
    }

    private static Result FolderChecker(string _Loc)
    {
        //https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing#get-all-matching-files
        if (Directory.Exists(_Loc))
        { LOG.Info("Directory exists!"); }
        else
        { return LOG.ErrorResult($"Folder \"{_Loc}\" does not exist!"); }

        if (Array.Exists(Directory.GetFiles(_Loc), X => X.EndsWith(METAFILEEXT)))
        { LOG.Info($"{METAFILEEXT} file was found!"); }
        else
        { LOG.Warning($"No {METAFILEEXT} datafile was present!"); }

        return LOG.InfoResult("Directory ready for use.");
    }

    private Result _LoadFiles(string _Loc)
    {
        //https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing#get-all-matching-files
        string[] Files = Directory.GetFiles(_Loc);
        Queue<string> Songs = new();
        bool Errors = false;

        LOG.Info($"Found {Files.Length} files in mix directory.");

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
                        LOG.Info($"Song \"{Path.GetFileName(F)}\" loaded.");
                    }
                    else
                    { LOG.Warning($"Song \"{Path.GetFileName(F)}\" is of incompatible file type."); }
                    break;
                }
            }
        }
        
        LOG.Info($"Of {Files.Length} file(s), {Songs.Count} song(s) loaded.");

        if (Songs.Count > 0)
        {
            Result R = LoadSongs(Songs);

            if (!R.IsFailure)
            { return Result.Success(); }
            
            LOG.Error($"\t{R.Error}");
            Errors = true;
        }
        else
        {
            Errors = true;
            LOG.Error("\tThere were no (suitable) songs found in mix!");
        }

        return Errors ? LOG.ErrorResult("Songs failed to load due to above errors.") : Result.Success();
    }

    private Result LoadSongs(Queue<string> _Songs)
    {
        if (!IsInitialised)
        { return LOG.ErrorResult(DefMsg.BASS_NO_INIT); }

        while (_Songs.Count > 0)
        {
            string Song = _Songs.Dequeue();

            SongData Temp = new();
            
            LOG.Info($"Trying to load \"{Song}\".");
            
            try
            {
                Temp.SoundHandle = Path.GetExtension(Song).ToLower() switch
                {
                    ".flac" => BassFlac.CreateStream(Song),
                    _ => Bass.CreateStream(Song)
                };

                if (Temp.SoundHandle == 0)
                {
                    LOG.Error($"Couldn't load \"{Song}\".");
                    LOG.Error($"\tBass error -> {Bass.LastError}");
                }
                
                //Why is this here?
                Thread.Sleep(100);
                Temp.Duration = Bass.ChannelBytes2Seconds(Temp.SoundHandle,
                            Bass.ChannelGetLength(Temp.SoundHandle))
                                    .DblToTS();
            }
            catch (Exception EXC)
            { return LOG.ErrorResult(EXC.Message); }

            try
            {
                LOG.Info($"Trying to load song info for \"{Song}\".");
                
                Track SongTrackInfo = new(Song);

                if (SongTrackInfo.EmbeddedPictures.Count > 0)
                {
                    Temp.Info = Temp.Info with
                    {
                        CoverImg = Maybe.From(SongTrackInfo.EmbeddedPictures.First().PictureData.ToList())
                    }; }
                else
                { LOG.Warning($"No embedded pictures found for {Song}."); }

                Temp.Info = Temp.Info with { ArtistName = SongTrackInfo.Artist };
                Temp.Info = Temp.Info with { SongName = SongTrackInfo.Title ?? Path.GetFileNameWithoutExtension(Song) };
            }
            catch (Exception EXC)
            { return LOG.ErrorResult(EXC.Message); }

            Tunes.Add(Temp);
        }

        NowPlaying = Tunes[0];
        
        return LOG.InfoResult("Successfully loaded (some) songs.");
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
            LOG.Info("Subscribed to song end.");
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
            LOG.Info("Unsubscribed from end of song.");
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
        { LOG.Error(DefMsg.PLAYER_DISPOSED); return false; }

        switch (Tunes.Count)
        {
            case 0:
                LOG.Error(DefMsg.TUNE_COUNT); return false;
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
        LOG.Warning("Disposing of SAPlayer!");

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
        LOG.Warning("Closing!");
        
        Stop();
        
        Dispose(true);
        
        LOG.Warning("Disposed and closing program");
    }
    #endregion
}
