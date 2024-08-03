using CSharpFunctionalExtensions;
using System.Globalization;

namespace Common.Appearance;

public struct AppearanceCollection;

public struct ControlSpecs(Colour _Default)
{
    public Maybe<string> Font;
    public Maybe<float> FontSize;
    public Maybe<Colour> Foreground;
    public Maybe<Colour> Background;
    public Colour Default = _Default;

    //Forces each control to have a default colour
}

public struct Colour : IEquatable<Colour>
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

    public Result FromHex(string _HexARGB)
    {
        string HexStr = _HexARGB
                            .Replace("#", "")
                            .Trim();

        if (HexStr.Length is not (6 or 8))
        { return Result.Failure("Hex input must be 6 or 8 characters."); }

        List<byte> Vals = [];

        try
        {
            for (int i = 0; i < HexStr.Length; i+=2)
            {
                Vals.Add(byte.Parse(
                    HexStr.Substring(i, 2), 
                    NumberStyles.HexNumber)
                );
            }
        }
        catch (Exception EXC)
        { return Result.Failure(EXC.ToString()); }

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

        return Result.Success();
    }

    public static Result<Colour> MakeFromHex(string _HexARGB)
    {
        Colour C = new Colour();
        Result R = C.FromHex(_HexARGB);

        return R.IsFailure ? Result.Failure<Colour>(R.Error) : Result.Success(C);
    }

    public string ToHex()
        => $"{A:X}{R:X}{G:X}{B:X}";

    public static string ToHex(byte _R, byte _G, byte _B, byte _A = 0)
        => $"{_A:X}{_R:X}{_G:X}{_B:X}";
    #endregion

    #region Overrides    
    public override bool Equals(object? _O2)
    { return _O2 is Colour C && (A == C.A) && (R == C.R) && (G == C.G) && (B == C.B); }

    public bool Equals(Colour _O2)
    { return R == _O2.R && G == _O2.G && B == _O2.B && A == _O2.A; }

    public override int GetHashCode()
    { return HashCode.Combine(R, G, B, A); }
    #endregion
}
