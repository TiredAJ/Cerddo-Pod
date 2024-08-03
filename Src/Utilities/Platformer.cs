using System.Runtime.InteropServices;
using Utilities.Logging;

namespace Utilities.Platforms;

public class Platformer
{
    private static OSPlat _CurPlatform = OSPlat.OTHER;
    
    /// <summary>
    /// Gets the <see cref="OSPlat"/> of the current platform
    /// </summary>
    /// <returns><see cref="OSPlat"/></returns>
    public static OSPlat GetPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        { _CurPlatform = OSPlat.LINUX; }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        { _CurPlatform = OSPlat.WINDOWS; }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        { _CurPlatform = OSPlat.OSX; }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        { _CurPlatform = OSPlat.FREEBSD; }
        else
        { _CurPlatform = OSPlat.OTHER; }
        
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
            case OSPlat.LINUX:
            { return "Linux"; }
            case OSPlat.WINDOWS:
            { return "Windows"; }
            case OSPlat.OSX:
            { return "OSX"; }
            case OSPlat.FREEBSD:
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
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => OSArch.X64,
            Architecture.Arm64 => OSArch.ARM64,
            _ => OSArch.OTHER
        };
    }

    public static string GetOSLogLocation()
    {
        switch (GetPlatform())
        {
            case OSPlat.LINUX:
            case OSPlat.OSX:
            case OSPlat.FREEBSD:
                return "/var/log";
            case OSPlat.WINDOWS:
                return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            case OSPlat.OTHER:
            default:
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
    }
}

public enum OSPlat
{
    WINDOWS,
    LINUX,
    OSX,
    FREEBSD,
    OTHER
}

public enum OSArch
{
    ARM64,
    X64,
    OTHER
}
