using System;

namespace Player.Utils;

public static class Extensions
{
    /// <summary>
    /// Converts a double to a timespan
    /// </summary>
    /// <param name="_D">Seconds as a double</param>
    /// <returns>A timespan representing _D</returns>
    public static TimeSpan DblToTS(this double _D)
    {
        if (_D >= TimeSpan.MaxValue.TotalSeconds)
        { return TimeSpan.Zero; }
        else
        { return TimeSpan.FromSeconds(_D); }
    }

    /// <summary>
    /// Converts the timespan to a display-friendly string
    /// </summary>
    /// <param name="_TS">Timespan to display</param>
    /// <returns>Formatted string that only shows the hours if it's relevant</returns>
    public static string TSDisplay(this TimeSpan _TS)
    {
        if (_TS.Hours > 0)
        { return _TS.ToString("hh\\:mm\\:ss"); }
        else
        { return _TS.ToString("mm\\:ss"); }
    }
}
