using CSharpFunctionalExtensions;
using SharpAudio;
using SharpAudio.Codec;
using System.Collections.Immutable;
using Utilities;

namespace Player;

public class SAPlayer
{
    #region Member variables
    public const string METAFILE = ".mpdata";
    public ImmutableArray<string> SUPPORTEDFILETYPES = [".wav", ".mp3", ".ogg"];

    public float Volume = 0.0125f;

    private readonly Maybe<AudioEngine> ENG;

    private List<Maybe<SoundStream>> Tunes = new List<Maybe<SoundStream>>();

    private Queue<string> Songs = new();
    private Maybe<IEnumerable<string>> InitSongs;
    private Maybe<MPData> MetaData;
    #endregion

    public SAPlayer()
    { ENG = AudioEngine.CreateDefault(); }

    public Result LoadFiles(string _Location)
    {
        var R = FolderChecker(_Location);

        if (R.IsFailure)
        { return R; }

        _LoadFiles(_Location);

        LoadSongs();
    }

    private Result FolderChecker(string _Loc)
    {
        if (!Directory.Exists(_Loc))
        { return Result.Failure($"Folder [{_Loc}] does not exist!"); }
        else if (!Directory.GetFiles(_Loc).Any(X => X.EndsWith(METAFILE)))
        { return Result.Failure($"No {METAFILE} datafile was present!"); }
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
                case METAFILE when MetaData.HasNoValue:
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

        SoundStream Temp;

        while (Tunes.Count < 3)
        {
            Temp = new SoundStream(File.OpenRead(Songs.Dequeue()), ENG.Value);
            Temp.PropertyChanged += Song_PropertyChanged;
            Temp.Volume = Volume;

            Tunes.Add(Temp);
        }
    }

    private void Song_PropertyChanged(object? _Sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Position")
        { return; }
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