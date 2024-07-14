using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Common.Appearance;
using System;

namespace MixerStudio.Utils;

public static class ConvertersUtils
{
    public static FuncValueConverter<double, string> PercentageConverter { get; } =
        new(_Value => $"{((_Value / 1500f) * 100f):N0}%");
    
    public static FuncValueConverter<double, string> SecsToStr { get; } =
        new(_Value => _Value > TimeSpan.MaxValue.TotalSeconds 
            ? "-00:-00" 
            : TimeSpan.FromSeconds(_Value).ToString("mm\\:ss"));

    public static FuncValueConverter<(Control C, ControlSpecs CSpec), Color> ControlColour { get; } =
        new(_Value => _Value.CSpec.Background.HasValue
            ? _Value.CSpec.Background.Value.ColToAvCol()
            : _Value.CSpec.Default.ColToAvCol());
}
