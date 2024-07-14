using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using CSharpFunctionalExtensions;
using Player;
using System;
using System.Collections.Generic;
using System.IO;
using System.Transactions;

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

    public static FuncValueConverter<SongData, string> DurationStr { get; } =
        new(_Value => _Value.Duration.TotalHours >= 1 ? 
                TimeSpan.FromSeconds(_Value.Duration.TotalSeconds).ToString("hh\\:mm\\:ss") :
                TimeSpan.FromSeconds(_Value.Duration.TotalSeconds).ToString("mm\\:ss"));

    public static FuncValueConverter<double, string> PositionStr { get; } =
        new(_Value => _Value >= 3600 ? 
                TimeSpan.FromSeconds(_Value).ToString("hh\\:mm\\:ss") :
                TimeSpan.FromSeconds(_Value).ToString("mm\\:ss"));

    public static FuncValueConverter<Uri, Bitmap> UriToBitmap { get; } =
        new(_Value => Helpers.LoadFromResource(_Value));

    public static FuncValueConverter<SongData, Bitmap> BytesToBitmap { get; } =
        new(_Value =>
        {
            if (_Value.CoverImg.HasNoValue)
            { return Helpers.DefaultImage; }
            
            return new Bitmap(new MemoryStream(_Value.CoverImg.Value.ToArray()));
        });
}
