using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Player;
using Player.Utils;
using System;
using ManagedBass;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using System.IO;
using Configuration = SixLabors.ImageSharp.Configuration;

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

    public static FuncValueConverter<Uri, Bitmap> UriToBitmap { get; } =
        new(_Value => Helpers.LoadFromResource(_Value));

    public static FuncValueConverter<List<byte>, Bitmap> BytesToBitmap { get; } =
        new(_Value =>
        {
            if (_Value is null)
            { return Helpers.DefaultImage; }
            
            using (var Data = Image.Load(new MemoryStream(_Value.ToArray())))
            {
                Stream Temp = new MemoryStream();
                
                //string Loc = Path.GetTempFileName();
                Data.SaveAsPng(Temp);
                
                return new Bitmap(Temp);
            }
        });
}
