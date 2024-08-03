using ManagedBass;

namespace Player.Utils;

public static class BassHelpers
{
    public static bool PluginLoad(out int _Handle, string _PluginName)
    {
        _Handle = Bass.PluginLoad(_PluginName);
        
        return _Handle != 0;
    }
}
