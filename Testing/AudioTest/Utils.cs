namespace AudioTest;

public static class Extensions
{
    public static string TimeDisplay(this double _Val)
    {
        if (_Val >= TimeSpan.MaxValue.TotalSeconds)
        { return "-00:-00"; }

        return TimeSpan.FromSeconds(_Val).ToString("mm\\:ss");
    }
}