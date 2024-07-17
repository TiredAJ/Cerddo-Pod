using CSharpFunctionalExtensions;

namespace Utilities.Logging;


//For voids (and throw)
public sealed partial class Logger : LoggerBase
{
    internal Logger()
    { }
    
    /// <summary>
    /// Pushes a warning to the log. User can ignore.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public void Info(string _Msg)
    { _PushLog('i', _Msg); }

    /// <summary>
    /// Pushes a warning to the log. User might want to note.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public void Warning(string _Msg)
    { _PushLog('@', _Msg); }

    /// <summary>
    /// Pushes an error to the log. User should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public void Error(string _Msg)
    { _PushLog('*', _Msg); }

    /// <summary>
    /// Pushes a fatal error to the log. User really should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public void Fatal(string _Msg)
    { _PushLog('!', _Msg); }
    
    /// <summary>
    /// Pushes an error to the log. User should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public void Error(DefMsg _Msg)
    { Error(_Msg.ToString()); }

    /// <summary>
    /// Pushes a fatal error to the log. User really should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public void Fatal(DefMsg _Msg)
    { Fatal(_Msg.ToString()); }
}

//For Results
public sealed partial class Logger
{
    public Result InfoResult(string _Msg)
    {
        Info(_Msg);
        return Result.Success();
    }
    
    public Result<T> InfoResult<T>(T _Msg)
    {
        if (_Msg != null)
        {
            Info(_Msg.ToString()!);
            return Result.Success<T>(_Msg);
        }
        return Result.Success<T>(default!);
    }
    
    /// <summary>
    /// Pushes an error to the log. User should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public Result<T> ErrorResult<T>(string _Msg)
    {
        Error(_Msg);
        return Result.Failure<T>(_Msg);
    }

    /// <summary>
    /// Pushes a fatal error to the log. User really should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public Result<T> FatalResult<T>(string _Msg)
    {
        Fatal(_Msg);
        return Result.Failure<T>(_Msg);
    }
    
    /// <summary>
    /// Pushes an error to the log. User should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public Result<T> ErrorResult<T>(DefMsg _Msg)
    { return ErrorResult<T>(_Msg.ToString()); }

    /// <summary>
    /// Pushes a fatal error to the log. User really should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public Result<T> FatalResult<T>(DefMsg _Msg)
    { return FatalResult<T>(_Msg.ToString()); }
    
    /// <summary>
    /// Pushes an error to the log. User should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public Result ErrorResult(string _Msg)
    {
        Error(_Msg);
        return Result.Failure(_Msg);
    }

    /// <summary>
    /// Pushes a fatal error to the log. User really should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public Result FatalResult(string _Msg)
    {
        Fatal(_Msg);
        return Result.Failure(_Msg);
    }
    
    /// <summary>
    /// Pushes an error to the log. User should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public Result ErrorResult(DefMsg _Msg)
    { return ErrorResult(_Msg.ToString()); }

    /// <summary>
    /// Pushes a fatal error to the log. User really should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public Result FatalResult(DefMsg _Msg)
    { return FatalResult(_Msg.ToString()); }
    
    /// <summary>
    /// Pushes a fatal error to the log. User really should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public T FatalThrow<T>(string _Msg) where T : Exception
    {
        Fatal(_Msg);
        return (T)new Exception(_Msg);
    }

    /// <summary>
    /// Pushes a fatal error to the log. User really should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public Exception FatalThrow(string _Msg)
    {
        Fatal(_Msg);
        return new Exception(_Msg);
    }
    
    /// <summary>
    /// Pushes a fatal error to the log. User really should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public T FatalThrow<T>(DefMsg _Msg) where T : Exception
    { return FatalThrow<T>(_Msg.ToString()); }

    /// <summary>
    /// Pushes a fatal error to the log. User really should investigate/report.
    /// </summary>
    /// <param name="_Msg">String message to push</param>
    public Exception FatalThrow(DefMsg _Msg)
    {
        return FatalThrow(_Msg.ToString());
    }
}

public sealed partial class Logger
{
    /// <summary>
    /// Pushes the input object to the log and returns it.
    /// </summary>
    /// <param name="_Msg">Object to log.</param>
    /// <typeparam name="T">Type of _Msg</typeparam>
    /// <returns>_Msg.</returns>
    public T InfoReturn<T>(T _Msg)
    {
        Info(_Msg.ToString());
        return _Msg;
    }
}
