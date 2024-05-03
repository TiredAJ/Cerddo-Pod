using Avalonia.Data.Converters;

namespace MP3_Pod.Utils;

public static class ConvertersUtils
{
    public static FuncValueConverter<double, string> PercentageConverter { get; } =
        new(Value => $"{((Value / 1500f) * 100f):N0}%");
}
