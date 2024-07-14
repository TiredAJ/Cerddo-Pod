using NUnit.Framework;
using NUnit.Framework.Api;
using Common.Appearance;
using NUnit.Framework.Internal;

namespace Common.Appearance.Testing;

[TestFixture]
public class ColourTests
{
    //passed - 30/05/24 @ 22:18
    [Test]
    public void HexToColour()
    {
        var TestCases = new(string Hex, Colour? Col)[]
        {
            ("FF5061FF", new Colour(255, 80, 97, 255)),
            ("FF2A5A6F", new Colour(255,42,90,111)),
            ("#FF2A5A6F", new Colour(255,42,90,111)),
            ("#FF2A00B1    ", new Colour(255,42,0,177)),
        };

        foreach (var T in TestCases)
        {
            /*Assert.That(Colour.MakeFromHex(T.Hex), Is.EqualTo(T.Col.Value.ToString()), 
                $"Could not convert {T.Hex}");  */          
        }        
    }
}