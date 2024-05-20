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
    private static bool WriterRunning = false, Waiting = false;

    #region Pushing to _Log
    /// <summary>
    /// Pushes a string to the log
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public static void Log(string _Msg)
    { _PushLog(_Msg); }

    /// <summary>
    /// Pushes a <see cref="DefMsg"/> to the log
    /// </summary>
    /// <param name="_Msg"><see cref="DefMsg"/> to push</param>
    public static void Log(DefMsg _Msg)
    { _PushLog(_Msg.ToString()); }

    /// <summary>
    /// Pushes a string to the log, returning a failed result
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    /// <returns>Failed <see cref="Result"/> containing _Msg</returns>
    public static Result LogResult(string _Msg)
    {
        _PushLog(_Msg);
        return Result.Failure(_Msg);
    }
    
    /// <summary>
    /// Pushes a <see cref="DefMsg"/> to the log, returning a failed result
    /// </summary>
    /// <param name="_Msg"><see cref="DefMsg"/> to push</param>
    /// <returns>Failed <see cref="Result"/> containing _Msg</returns>
    public static Result LogResult(DefMsg _Msg)
    {
        _PushLog(_Msg.ToString());
        return Result.Failure(_Msg.ToString());
    }
    
    /// <summary>
    /// Pushes a string to the log, returning a failed result
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    /// <returns>Failed <see cref="Result"/> containing _Msg</returns>
    public static Result<T> LogResult<T>(string _Msg)
    {
        _PushLog(_Msg);
        return Result.Failure<T>(_Msg);
    }

    /// <summary>
    /// Pushes a <see cref="DefMsg"/> to the log, returning a failed result
    /// </summary>
    /// <param name="_Msg"><see cref="DefMsg"/> to push</param>
    /// <returns>Failed <see cref="Result"/> containing _Msg</returns>
    public static Result<T> LogResult<T>(DefMsg _Msg)
    {
        _PushLog(_Msg.ToString());
        return Result.Failure<T>(_Msg.ToString());
    }

    /// <summary>
    /// Pushes a string to the log, returning the message
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    /// <returns>_Msg</returns>
    public static string LogStr(string _Msg)
    {
        _PushLog(_Msg);
        return _Msg;
    }
    
    /// <summary>
    /// Pushes a <see cref="DefMsg"/> to the log, returning it's string equivalent
    /// </summary>
    /// <param name="_Msg"><see cref="DefMsg"/> to push</param>
    /// <returns>_Msg converted to it's string equivalents</returns>
    public static string LogStr(DefMsg _Msg)
    {
        _PushLog(_Msg.ToString());
        return _Msg.ToString();
    }
    
    /// <summary>
    /// Pushes a string to the log, returning an exception containing the message
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    /// <returns>An exception containing _Msg</returns>
    public static Exception LogThrow(string _Msg)
    {
        _PushLog(_Msg);
        Thread.Sleep(250);
        return new Exception(_Msg);
    }
    
    /// <summary>
    /// Pushes a <see cref="DefMsg"/> to the log, returning an exception containing the message
    /// </summary>
    /// <param name="_Msg"><see cref="DefMsg"/> to push</param>
    /// <returns>An exception containing _Msg</returns>
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

        if (Waiting)
        { return; }

        Waiting = true;
        
        WhileWriting.Wait();
        WhileWriting.Reset();

        Waiting = false;

        Task.Run(static () =>
        {
            (DateTime Stamp, string Msg) Temp;

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

/// <summary>
/// Handy, common, default messages
/// </summary>
public enum DefMsg
{
    /// <summary>
    /// When Player is disposed
    /// </summary>
    PlayerDisposed,
    /// <summary>
    /// When Tune contains less than 1 song
    /// </summary>
    TuneCount,
    /// <summary>
    /// When Bass isn't/hasn't been initialised
    /// </summary>
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
            case DefMsg.PlayerDisposed:
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