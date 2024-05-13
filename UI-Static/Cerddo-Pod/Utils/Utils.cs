using Avalonia.Controls;
using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Cerddo_Pod.ViewModels;
using System.IO;
using System.Threading.Tasks;

namespace Cerddo_Pod.Utils;

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

    public static bool Equals(this double _Da, double _Db)
    {
        if (double.IsNaN(_Da) || double.IsNaN(_Db))
        { return false; }
        else if (double.IsInfinity(_Da) || double.IsInfinity(_Db))
        { return _Da == _Db; }
        else
        { return ((long)_Da * 100) == ((long)_Db * 100); }
    }
}

public class Helpers
{
    public static Uri DefaultImageUri =
        new Uri("avares://Cerddo_Pod/Assets/Icons/Default.png");

    public static Bitmap DefaultImage => LoadFromResource(DefaultImageUri);

    public static Bitmap LoadFromResource(Uri? _Resource)
    {
        Uri Resource;
        
        if (_Resource is null || !AssetLoader.Exists(_Resource))
        { Resource = DefaultImageUri; }
        else
        { Resource = _Resource; }

        using (var S = AssetLoader.Open(Resource))
        { return new Bitmap(S); }
    }

    public static void LoadFromProperties(Image _PBX, byte[] _Data)
    {
        Task.Run(() =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                using (Stream S = new MemoryStream(_Data))
                { _PBX.Source = new Bitmap(S); }
            });
        });
    }

    public static MainViewModel? GetParentContext(object? _Sender)
    {
        var View = _Sender as UserControl;

        if (View != null && View.Parent != null &&
            (View.Parent.DataContext as MainViewModel) != null)
        { return View.Parent.DataContext as MainViewModel; }
        else
        { return null; }
    }
}