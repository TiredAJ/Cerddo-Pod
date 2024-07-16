using CSharpFunctionalExtensions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

using Utilities.Platforms;

namespace Utilities.Logging;

public abstract class LoggerBase
{
    #region Protected members
    protected ConcurrentQueue<string> FileTemp = new();
    //protected static ManualResetEventSlim WhileWriting = new (true);
    //protected bool WriterRunning = false, Waiting = false;
    protected static string DefaultLoc = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    #endregion

    #region Internal Members
    internal Maybe<string> LogLocation
    {
        get
        { return _LogLocation; }
        set
        {
            if (value.HasValue)
            {
                if (!Directory.Exists(Path.GetDirectoryName(value.Value)))
                {
                    try
                    {
                        Directory.CreateDirectory(value.Value);
                        _LogLocation = value;
                        return;
                    }
                    catch (Exception EXC)
                    { _LogLocation = $"{DefaultLoc}{Path.DirectorySeparatorChar}CerddoPod-Log.md"; }
                }
                
                _LogLocation = value;
            }
            else
            { _LogLocation = $"{DefaultLoc}{Path.DirectorySeparatorChar}CerddoPod-Log.md"; }
        }
    }
    internal Maybe<string> _LogLocation = string.Empty;
    internal Maybe<Action<string>> CustomStrLogger = Maybe<Action<string>>.None;
    internal string LogName = String.Empty;
    internal bool WriterThreadRun = false;
    #endregion
    
    /// <summary>
    /// Private, base function to push to all the logs
    /// </summary>
    /// <param name="_Msg">string message to push</param>
    protected void _PushLog(char _Severity, string _Msg)
    {
        var MSG = $"[{_Severity}] - [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] => {_Msg}";

        SendLog_Debug(MSG);
        SendLog_File(MSG);

        if (CustomStrLogger.HasValue)
        { CustomStrLogger.Value(_Msg); }
        // Task.Run(() =>
        // {
        // });
    }
    
    [Conditional("DEBUG")]
    private void SendLog_Debug(string _Msg)
    { Debug.WriteLine(_Msg); }

    private void SendLog_File(string _Msg)
    {
        //enques the current message to the file queue
        FileTemp.Enqueue(_Msg);

        if (!WriterThreadRun)
        { 
            WriterThreadRun = true;
            WriterThread();
        }
    }

    public async Task WriterThread()
    {
        await Task.Run(async() =>
        {
            string MSG = string.Empty;

            using (StreamWriter Writer = new (LogLocation.Value))
            {
                while (WriterThreadRun)
                {
                    if (FileTemp.TryDequeue(out MSG))
                    {
                        //Debug.WriteLine(MSG);
                        
                        Writer.WriteLineAsync(MSG);
                        
                        Writer.Flush();
                    }
                }
            }
        });
    }

    public void CloseWriter()
    { WriterThreadRun = false; }
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

public class LoggerBuilder
{
    private Logger _Logger;
    private char Separator = Path.DirectorySeparatorChar;
    
    private static Maybe<LoggerBuilder> Instance = Maybe<LoggerBuilder>.None;

    public static Dictionary<string, Logger> Loggers { get; private set; } = new ();

    private LoggerBuilder()
    { }

    public static void CloseLoggers()
    {
        foreach (var L in Loggers.Values)
        {
            L.Info("Closing Log...");
            L.CloseWriter();
        }
    }
    
    /// <summary>
    /// Init
    /// </summary>
    public static LoggerBuilder Init()
    {
        if (Instance.HasNoValue)
        { Instance = new LoggerBuilder(); }
        
        return Instance.Value;
    }

    /// <summary>
    /// Creates a new <see cref="Logger"/> object.
    /// </summary>
    /// <returns></returns>
    public LoggerBuilder NewLogger()
    {
        _Logger = new Logger();
        return this;
    }

    /// <summary>
    /// Determines the location of the log file.
    /// </summary>
    /// <param name="_Location">Location where the log file should go</param>
    public LoggerBuilder SetLogLocation(string _Location)
    {
        _Logger.LogLocation = _Location;
        return this;
    }

    /// <summary>
    /// Uses the default OS logging directory. Suggested to be used with <see cref="LogName"/>
    /// </summary>
    /// <returns></returns>
    public LoggerBuilder UseDefaultLoc()
    {
        _Logger.LogLocation = $"{Platformer.GetOSLogLocation()}{Separator}CerddoPod";
        return this;
    }

    /// <summary>
    /// Adjusts the log location to be specific to a project and component. So for the component "SAPlayer" in "CerddoPod",
    /// <see cref="_ProjectName"/> would be "CerddoPod" and <see cref="_ComponentName"/> would be "SAPlayer". So that log
    /// would look something like "/var/log/CerddoPod/SAPlayer-Log.md".
    /// MUST be used after either <see cref="UseDefaultLoc"/> or <see cref="SetLogLocation"/>, otherwise <see cref="UseDefaultLoc"/>
    /// will be used. This will also be the name of the logger in <see cref="Loggers"/> (e.g. "CerddoPod/SAPlayer".
    /// </summary>
    /// <param name="_ProjectName">Name of project <see cref="_ComponentName"/> belongs to.</param>
    /// <param name="_ComponentName">Name of component to log.</param>
    /// <returns></returns>
    public LoggerBuilder LogName(string _ProjectName, string _ComponentName)
    {
        if (_Logger.LogLocation.HasNoValue)
        { this.UseDefaultLoc(); }
        
        _Logger.LogLocation += $"{Separator}{_ProjectName}{Separator}{_ComponentName}-Log.md";
        
        _Logger.LogName = $"{_ProjectName}/{_ComponentName}";
        
        return this;
    }
    
    /// <summary>
    /// Adjusts the log location to be specific to a standalone component. So for the component "Zipper", <c "_ComponentName"/>
    /// would be "Zipper". So that log would look something like "/var/log/CerddoPod/Utils/Zipper-Log.md".
    /// MUST be used after either <see cref="UseDefaultLoc"/> or <see cref="SetLogLocation"/>, otherwise <see cref="UseDefaultLoc"/>
    /// will be used. This will also be the name of the logger in <see cref="Loggers"/> (e.g. "CerddoPod/Utils/Zipper".
    /// </summary>
    /// <param name="_ProjectName">Name of project <see cref="_ComponentName"/> belongs to.</param>
    /// <param name="_ComponentName">Name of component to log.</param>
    /// <returns></returns>
    public LoggerBuilder LogName(string _ComponentName)
    {
        if (_Logger.LogLocation.HasNoValue)
        { this.UseDefaultLoc(); }
        
        _Logger.LogLocation += $"{Separator}CerddoPod{Separator}Utils{Separator}{_ComponentName}-Log.md";
        
        _Logger.LogName = $"Utils/{_ComponentName}";
        
        return this;
    }

    private void _Build()
    {
        if (!Directory.Exists(Path.GetDirectoryName(_Logger.LogLocation.Value)))
        { Directory.CreateDirectory(Path.GetDirectoryName(_Logger.LogLocation.Value)); }
        
        if (!File.Exists(_Logger.LogLocation.Value))
        { File.Create(_Logger.LogLocation.Value).Close(); }

        _Logger.WriterThreadRun = false;

        //_Logger.Writer = new StreamWriter(_Logger.LogLocation.Value, Encoding.UTF8, new FileStreamOptions(){Mode = FileMode.Append, Access = FileAccess.Write, Options = FileOptions.None});
    }

    /// <summary>
    /// Creates the logger object and adds it to <see cref="Loggers"/>.
    /// </summary>
    /// <returns>New Logger object to use.</returns>
    public Logger BuildAndStore()
    {
        _Build();
        Loggers.Add(_Logger.LogName, _Logger);
        return Loggers[_Logger.LogName];        
    }

    /// <summary>
    /// Creates the logger object without adding it to <see cref="Loggers"/>.
    /// </summary>
    /// <returns>New Logger object to use.</returns>
    public Logger Build()
    {
        _Build();
        return _Logger;
    }

    /// <summary>
    /// Stores the logger in <see cref="Loggers"/> without returning the new logger.
    /// </summary>
    public void Store()
    {
        _Build();
        Loggers.Add(_Logger.LogName, _Logger);
    }

    /// <summary>
    /// Copies settings from existing Logger.
    /// </summary>
    /// <param name="_L">Logger to copy from.</param>
    public LoggerBuilder CopyFrom(Logger _L)
    {
        _Logger = new Logger()
        {
            LogLocation = _L.LogLocation, 
            CustomStrLogger = _L.CustomStrLogger,
            LogName = _L.LogName
        };
        return this;
    }

    /// <summary>
    /// Creates the custom logger function called when a new log is created.
    /// </summary>
    /// <param name="_CustomStrLogger">A function that takes a string, returning void.</param>
    public LoggerBuilder CustomLogger(Action<string> _CustomStrLogger)
    {
        _Logger.CustomStrLogger = _CustomStrLogger;
        return this;
    }
}
