using Common.Appearance;
using Avalonia.Media;

namespace MixerStudio.Utils;

public static class Extensions
{
    public static Colour AvColToCol(this Color _AvCol)
        => new () { A = _AvCol.A, B = _AvCol.B, G = _AvCol.G, R = _AvCol.R };

    public static Color ColToAvCol(this Colour _Col)
        => Color.FromArgb(_Col.A, _Col.R, _Col.G, _Col.B);
}
