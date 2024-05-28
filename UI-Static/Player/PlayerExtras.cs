using ManagedBass;
using ReactiveUI;
using Utilities.Logging;
using CSharpFunctionalExtensions;

namespace Player;

public class PlayerBase : ReactiveObject
{
    #region Public Members
    /// <summary>
    /// File extensions
    /// </summary>
    public const string METAFILEEXT = ".mpdata", VIEWFILEEXT = ".mpview";
    /// <summary>
    /// Audio file types supported by the application
    /// </summary>
    public static readonly string[] SUPPORTEDFILETYPES = [".wav", ".mp3", ".ogg", ".aiff", ".mp2", ".mp1", ".flac"];
    /// <summary>
    /// State of Bass' initialisation
    /// </summary>
    public bool IsInitialised { get; protected set; }
    #endregion

    #region Private Members
    /// <summary>
    /// List of loaded songs
    /// </summary>
    protected List<SongData> Tunes = [];
    /// <summary>
    /// Current song
    /// </summary>
    protected SongData _NowPlaying = SongData.Default;
    /// <summary>
    /// Current position in <see cref="_NowPlaying"/>
    /// </summary>
    protected double _Position = 0d;
    /// <summary>
    /// Used to compare against <see cref="_Position"/> to check for user input
    /// </summary>
    protected double _ParPosition = 0d;
    /// <summary>
    /// Index of current song in <see cref="Tunes"/>
    /// </summary>
    protected int _CurrentSong = 0;
    /// <summary>
    /// Handle for the Bass Flac plugin
    /// </summary>
    protected int BassFlacHandle = 0;
    /// <summary>
    /// Volume for <see cref="ManagedBass.Bass.GlobalStreamVolume"/>
    /// </summary>
    protected int _Volume = 200;
    /// <summary>
    /// (Unused?) location of extracted mix
    /// </summary>
    protected string FolderLoc = "";
    /// <summary>
    /// Whether object has been disposed
    /// </summary>
    protected bool DisposedValue;
    /// <summary>
    /// Flag for <see cref="SAPlayer.PositionRunner()">PositionRunner()</see>
    /// </summary>
    protected bool PosRun;
    /// <summary>
    /// Flag for whether the player has subscribed to <see cref="Syncer.EndOfSong"/>
    /// </summary>
    protected bool EndSubsribed = false;
    /// <summary>
    /// Used to control <see cref="SAPlayer.PositionRunner()">PositionRunner()</see>
    /// </summary>
    protected ManualResetEventSlim PosRunPause = new(true);    
    #endregion
}

/// <summary>
/// Used to handle syncing with the end of a song
/// </summary>
public class Syncer
{
    /// <summary> Primary event to subscribe to </summary>
    public static event EventHandler? EndOfSong;
    private static SyncProcedure IntSyncer = new SyncProcedure(SyncProc);

    /// <summary>
    /// Sets the sync in Bass to the inputted song handle
    /// </summary>
    /// <param name="_Handle">Handle of song to subscribe to it's end</param>
    public static void InitSync(int _Handle)
    { Bass.ChannelSetSync(_Handle, SyncFlags.End | SyncFlags.Mixtime, 0, IntSyncer); }

    /// <summary>
    /// Called when the song has finished
    /// </summary>
    private static void SyncProc(int _Handle, int _Channel, int _Data, IntPtr _User)
    { EndOfSong?.Invoke(null, EventArgs.Empty); }
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

/// <summary>
/// Contains all the data necessary to represent a song.
/// </summary>
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

/// <summary>
/// This contains the effects that will be applied
/// to the it's owning song.
/// </summary>
public struct SongEffects
{
    
}