using System.Runtime.InteropServices;

namespace Utilities;

public class Platformer
{
    private static OSPlat _CurPlatform = OSPlat.Other;
    
    /// <summary>
    /// Gets the <see cref="OSPlat"/> of the current platform
    /// </summary>
    /// <returns><see cref="OSPlat"/></returns>
    public static OSPlat GetPlatform()
    {
        if (_CurPlatform != OSPlat.Other)
        { return _CurPlatform; }
        
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
        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X64:
                return OSArch.X64;
            case Architecture.Arm64:
                return OSArch.Arm64;
            default:
                return OSArch.Other;
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