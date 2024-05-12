using CSharpFunctionalExtensions;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Utilities;

public sealed class Logger
{
    private static ConcurrentQueue<(DateTime Stamp, string Msg)> FileTemp = new();
    private static ConcurrentQueue<(DateTime Stamp, string Msg)> _Log = new();
    private static ManualResetEventSlim WhileWriting = new ManualResetEventSlim(true);
    private static object Lock = new();
    private static string LogLoc = string.Empty;
    private static bool WriterRunning = false;

    #region Pushing to _Log
    public static void Log(string _Msg)
    { _PushLog(_Msg); }

    public static void Log(DefMsg _Msg)
    { _PushLog(_Msg.ToString()); }

    public static Result LogResult(string _Msg)
    {
        _PushLog(_Msg);
        return Result.Failure(_Msg);
    }

    public static Result LogResult(DefMsg _Msg)
    {
        _PushLog(_Msg.ToString());
        return Result.Failure(_Msg.ToString());
    }

    public static Result<T> LogResult<T>(string _Msg)
    {
        _PushLog(_Msg);
        return Result.Failure<T>(_Msg);
    }

    public static Result<T> LogResult<T>(DefMsg _Msg)
    {
        _PushLog(_Msg.ToString());
        return Result.Failure<T>(_Msg.ToString());
    }

    private static void _PushLog(string _Msg)
    {
        Task.Run(() =>
        {
            var MSG = (DateTime.Now, _Msg);

            _Log.Enqueue(MSG);

            SendLog_Debug(MSG);
            SendLog_StdOut(MSG);
            SendLog_File(MSG);
        });
    }
    #endregion

    #region Actual logging
    private static void SendLog_Debug((DateTime Stamp, string Msg) _Msg)
    { Debug.WriteLine(_Msg.StrDisplay()); }

    private static void SendLog_StdOut((DateTime Stamp, string Msg) _Msg)
    { Console.WriteLine(_Msg.StrDisplay()); }

    private static void SendLog_File((DateTime Stamp, string Msg) _Msg)
    {
        if (LogLoc == string.Empty)
        {
            LogLoc = Path.Combine(
                        Environment.GetFolderPath
                            (Environment.SpecialFolder.DesktopDirectory),
                        "Log.txt");
        }

        FileTemp.Enqueue(_Msg);

        WhileWriting.Wait();
        WhileWriting.Reset();

        Task.Run(static () =>
        {
            (DateTime Stamp, string Msg) Temp = (DateTime.MinValue, string.Empty);

            while (!FileTemp.IsEmpty)
            {
                if (FileTemp.IsEmpty)
                { break; }

                using (StreamWriter Writer = new StreamWriter(LogLoc, true))
                {
                    while (!FileTemp.IsEmpty)
                    {
                        if (FileTemp.TryDequeue(out Temp))
                        { Writer.WriteLine(Temp.StrDisplay()); }
                    }
                }
            }
            WhileWriting.Set();
        });
    }
    #endregion
}

public enum DefMsg
{
    BassDispose,
    TuneCount,
    BassNoInit,
}

static class Extensions
{
    internal static string StrDisplay(this (DateTime Stamp, string Msg) _Msg)
    { return $"[{_Msg.Stamp:dd/MM/yyyy HH:mm:ss}]: {_Msg.Msg}"; }

    internal static string StrDisplay(this DefMsg _Msg)
    {
        switch (_Msg)
        {
            case DefMsg.BassDispose:
            { return "Bass has been disposed! Please re-initialise."; }
            case DefMsg.TuneCount:
            { return "Tunes contains no songs to play."; }
            case DefMsg.BassNoInit:
            { return "Bass is not initialised."; }
            default:
            { return "Unknown Default Message"; }
        }
    }
}