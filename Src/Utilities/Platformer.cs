using System.Runtime.InteropServices;
using Utilities.Logging;

namespace Utilities.Platforms;

public class Platformer
{
    private static OSPlat _CurPlatform = OSPlat.Other;
    private static Logger Log;

    public Platformer()
    {
        Log = LoggerBuilder.Init().UseDefaultLoc().LogName("Utilities").Build();
    }
    
    /// <summary>
    /// Gets the <see cref="OSPlat"/> of the current platform
    /// </summary>
    /// <returns><see cref="OSPlat"/></returns>
    public static OSPlat GetPlatform()
    {
        if (_CurPlatform != OSPlat.Other)
        { Log.Info($"Using cached platform \"{_CurPlatform}\""); return _CurPlatform; }
        
        Log.Info("Trying to discern platform...");
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        { _CurPlatform = OSPlat.Linux; }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        { _CurPlatform = OSPlat.Windows; }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        { _CurPlatform = OSPlat.OSX; }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        { _CurPlatform = OSPlat.FreeBSD; }
        else
        { _CurPlatform = OSPlat.Other; }
        
        Log.Info($"Platform assumed as {_CurPlatform}.");
        
        return _CurPlatform;
    }

    /// <summary>
    /// Gets a string representation of the current platform
    /// </summary>
    /// <returns>Either "Linux", "Windows", "OSX", "FreeBSD" or otherwise the OSDescription
    /// provided by the runtime</returns>
    public static string GetPlatformStr()
    {
        switch (GetPlatform())
        {
            case OSPlat.Linux:
            { return "Linux"; }
            case OSPlat.Windows:
            { return "Windows"; }
            case OSPlat.OSX:
            { return "OSX"; }
            case OSPlat.FreeBSD:
            { return "FreeBSD"; }
            default:
            { return RuntimeInformation.OSDescription; }
        }
    }

    /// <summary>
    /// Gets the architecture (<see cref="OSArch"/>) of the current platform
    /// </summary>
    /// <returns><see cref="OSArch"/></returns>
    public static OSArch GetArchitecture()
    {
        Log.Info("Attempting to discern platform architecture.");
        
        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X64:
                return Log.InfoReturn(OSArch.X64);
            case Architecture.Arm64:
                return Log.InfoReturn(OSArch.Arm64);
            default:
                return Log.InfoReturn(OSArch.Other);
        }
    }

    public static string GetOSLogLocation()
    {
        switch (GetPlatform())
        {
            case OSPlat.Linux:
            case OSPlat.OSX:
            case OSPlat.FreeBSD:
                return "/var/log";
            case OSPlat.Windows:
                return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            case OSPlat.Other:
            default:
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
    }
}

public enum OSPlat
{
    Windows,
    Linux,
    OSX,
    FreeBSD,
    Other
}

public enum OSArch
{
    Arm64,
    X64,
    Other
}
