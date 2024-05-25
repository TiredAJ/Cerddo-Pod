using CSharpFunctionalExtensions;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Utilities;

public sealed class Logger
{
    private static ConcurrentQueue<(DateTime Stamp, string Msg)> FileTemp = new();
    private static ManualResetEventSlim WhileWriting = new (true);
    private static string LogLocaction = string.Empty;
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
    { _PushLog(_Msg.StrDisplay()); }

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
        _PushLog(_Msg.StrDisplay());
        return Result.Failure(_Msg.StrDisplay());
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
        _PushLog(_Msg.StrDisplay());
        return Result.Failure<T>(_Msg.StrDisplay());
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
        _PushLog(_Msg.StrDisplay());
        return _Msg.StrDisplay();
    }
    
    /// <summary>
    /// Pushes a string to the log, returning an exception containing the message
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    /// <returns>An exception containing _Msg</returns>
    public static Exception LogThrow(string _Msg)
    {
        _PushLog(_Msg);
        Thread.Sleep(500);
        return new Exception(_Msg);
    }
    
    /// <summary>
    /// Private, base function to push to all the logs
    /// </summary>
    /// <param name="_Msg">string message to push</param>
    private static void _PushLog(string _Msg)
    {
        Task.Run(() =>
        {
            var MSG = (DateTime.Now, _Msg);

            SendLog_Debug(MSG);
            SendLog_File(MSG);
        });
    }
    #endregion

    #region Actual logging
    [Conditional("DEBUG")]
    private static void SendLog_Debug((DateTime Stamp, string Msg) _Msg)
    { Debug.WriteLine(_Msg.StrDisplay()); }

    private static void SendLog_File((DateTime Stamp, string Msg) _Msg)
    {
        //Checks that there's a log file to write to
        if (LogLocaction == string.Empty)
        {
            LogLocaction = Path.Combine(
                        Environment.GetFolderPath
                            (Environment.SpecialFolder.DesktopDirectory),
                        "CerddoPod-Log.txt");
        }

        //enques the current message to the file queue
        FileTemp.Enqueue(_Msg);

        //If there's already a thread waiting to execute, it'll return
        //because the message has already been 
        if (Waiting || WriterRunning)
        { return; }

        Waiting = true;
        
        WhileWriting.Wait();
        WhileWriting.Reset();

        Waiting = false;

        Task.Run(static () =>
        {
            if (FileTemp.IsEmpty)
            { return; }
            
            WriterRunning = true;
            
            (DateTime Stamp, string Msg) Temp;

            using (StreamWriter Writer = new (LogLocaction, true))
            {
                while (!FileTemp.IsEmpty)
                {
                    if (FileTemp.TryDequeue(out Temp))
                    { Writer.WriteLine(Temp.StrDisplay()); }
                }
                
                Writer.Close();
            }
            
            WhileWriting.Set();
            WriterRunning = false;
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
            { return "Player object has been disposed! Please re-initialise."; }
            case DefMsg.TuneCount:
            { return "Tunes contains no songs to play."; }
            case DefMsg.BassNoInit:
            { return "Bass is not initialised."; }
            default:
            { return "Unknown Default Message."; }
        }
    }
}