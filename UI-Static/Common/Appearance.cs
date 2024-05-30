using CSharpFunctionalExtensions;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace Common.Appearance;

public struct AppearanceCollection
{
}

public struct ControlSpecs
{
    public Maybe<string> Font;
    public Maybe<float> FontSize;
    public Maybe<Colour> Foreground;
    public Maybe<Colour> Background;
}

public struct Colour
{
    #region Properties    
    /// <summary>
    /// Red
    /// </summary>
    public byte R { get; set; }
    /// <summary>
    /// Green
    /// </summary>
    public byte G { get; set; }
    /// <summary>
    /// Blue
    /// </summary>
    public byte B { get; set; }
    /// <summary>
    /// Alpha
    /// </summary>
    public byte A { get; set; }
    #endregion

    #region Constructors
    public Colour()
    {
        A = 0xFF;
        R = 0x00;
        G = 0x00;
        B = 0x00;
    }

    public Colour(byte _A, byte _R, byte _G, byte _B)
    {
        A = _A;
        R = _R;
        G = _G;
        B = _B;        
    }

    public Colour(byte _R, byte _G, byte _B)
    {
        A = 0xFF;
        R = _R;
        G = _G;
        B = _B;
    }
    #endregion
    
    #region Conversions
    public void FromARGB(byte _A, byte _R, byte _G, byte _B)
    {
        A = _A;
        R = _R;
        B = _B;
        G = _G;
    }

    public void FromRGB(byte _R, byte _G, byte _B)
    {
        A = 0xff;
        R = _R;
        B = _B;
        G = _G;
    }

    public void FromHex(string _HexARGB)
    {
        string HexStr = _HexARGB
                            .Replace("#", "")
                            .Trim();

        List<byte> Vals = new();

        for (int i = 0; i < HexStr.Length; i+=2)
        { 
            Vals.Add(byte.Parse(
                            HexStr.Substring(i, 2), 
                            NumberStyles.HexNumber)
                            );
        }

        if (Vals.Count == 3)
        {
            A = 0xff;
            R = Vals[0];
            G = Vals[1];
            B = Vals[2];
        }
        else
        {
            A = Vals[0];
            R = Vals[1];
            G = Vals[2];
            B = Vals[3];
        }
    }

    public static Colour MakeFromHex(string _HexARGB)
    {
        var C = new Colour();
        C.FromHex(_HexARGB);
        return C;
    }

    public string ToHex()
        => $"{A:X}{R:X}{G:X}{B:X}";

    public static string ToHex(byte _R, byte _G, byte _B, byte _A = 0)
        => $"{_A:X}{_R:X}{_G:X}{_B:X}";
    #endregion

    #region Overrides    
    public override bool Equals(object? _O2)
    {
        if (_O2 is Colour C)
        {
            if (A != C.A)
            { return false; }

            if (R != C.R)
            { return false; }

            if (G != C.G)
            { return false; }

            if (B != C.B)
            { return false;}

            return true;
        }
        else
        { return false; }
    }
    #endregion
}