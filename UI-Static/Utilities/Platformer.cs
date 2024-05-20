using System.Runtime.InteropServices;

namespace Utilities;

public class Platformer
{
    private static OSPlat _CurPlatform = OSPlat.Other;
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
        else
        { _CurPlatform = OSPlat.Other; }
        
        return _CurPlatform;
    }

    public static string GetPlatformStr()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        { return "Linux"; }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        { return "Windows"; }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        { return "OSX"; }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        { return "FreeBSD"; }
        else
        { return RuntimeInformation.OSDescription; }
    }

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
    Other
}

public enum OSArch
{
    Arm64,
    X64,
    Other
}
