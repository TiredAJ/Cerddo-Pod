using Avalonia.Data.Converters;
using Player;
using Player.Utils;
using System;
using ManagedBass;

namespace Cerddo_Pod.Utils;

public static class ConvertersUtils
{
    public static FuncValueConverter<double, string> PercentageConverter { get; } =
        new(_Value => $"{((_Value / 1500f) * 100f):N0}%");
    
    public static FuncValueConverter<double, string> SecsToStr { get; } =
        new(_Value => _Value > TimeSpan.MaxValue.TotalSeconds ? "-00:-00" : TimeSpan.FromSeconds(_Value).ToString("mm\\:ss"));

    //take SongData and return the name of the song
    public static FuncValueConverter<SongData, string> SongName { get; } =
        new(_Value => _Value.SongName);
    
    public static FuncValueConverter<SongData, string> Artist { get; } =
        new(_Value => _Value.ArtistName);

    public static FuncValueConverter<SongData, double> Duration { get; } =
        new(_Value => _Value.Duration.TotalSeconds);
    
}
