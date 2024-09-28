using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization.Metadata;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.TypeResolvers;

namespace YAMLTesting;

class Program
{
    private static Song TempSong = new Song
    {
        Name = "Up & Up",
        Album = "A Head Full of Dreams",
        ArtistName = "Coldplay",
        TrackLength = 405L,
        Effects =
        [
            new Fade(345L,
                571L),
            new VolumeUp(5)
        ],
        OtherAdjustments = [new AdjustedEnds(20L, Int64.MaxValue/4)],
    };
    
    static void Main(string[] args)
    {
        Song? DeserialisedSong = null;
        
        var Serialiser = new SerializerBuilder()
                                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                                .WithTypeResolver(new DynamicTypeResolver())
                                .WithTagMapping("!Fade", typeof(Fade))
                                .WithTagMapping("!VolumeUp", typeof(VolumeUp))
                                .WithTagMapping("!AdjustedEnds", typeof(AdjustedEnds))
                                .Build();

        var YAMLOutput = Serialiser.Serialize(TempSong);
        Console.Write(YAMLOutput);
        
        DeserialisedSong = new DeserializerBuilder()
                                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                                .WithTypeResolver(new DynamicTypeResolver())
                                .WithTagMapping("!Fade", typeof(Fade))
                                .WithTagMapping("!VolumeUp", typeof(VolumeUp))
                                .WithTagMapping("!AdjustedEnds", typeof(AdjustedEnds))
                                .Build()
                                .Deserialize<Song>(YAMLOutput);
        
        Console.WriteLine("\n");
        Console.WriteLine($"Song: {DeserialisedSong.Name}");
        
        DeserialisedSong.RunEffects();
    }
}

public interface IAdjustment
{
    public virtual void RunAdjustment()
    { Debug.WriteLine("Running base adjustment!"); }
}

abstract class Effect : IAdjustment
{
    public long StartPos { get; set; }
    public long EndPos { get; set; }

    public virtual void RunAdjustment()
    { Debug.WriteLine("Running base effect!"); }
}

class AdjustedEnds : IAdjustment
{
    public AdjustedEnds()
    {
        StartPos = 0L;
        EndPos = long.MaxValue;
    }
    
    public AdjustedEnds(long _NewStartPos, long _NewEndPos)
    {
        StartPos = _NewStartPos;
        EndPos = _NewEndPos;
    }
    
    public long StartPos { get; set; }
    public long EndPos { get; set; }
    
    public void RunAdjustment()
    {
        Debug.WriteLine("Adjusting the ends!");
    }
}

class Fade : Effect
{
    public Fade()
    {
        StartPos = 0L;
        EndPos = long.MaxValue;
    }
    
    public Fade(long _StartPos, long _EndPos)
    {
        StartPos = _StartPos;
        EndPos = _EndPos;
    }
    
    public new void RunAdjustment()
    { Debug.WriteLine("Running Fade effect"); }
}

class VolumeUp : Effect
{
    public int PercentIncrease { get; set; }

    public VolumeUp()
    { PercentIncrease = 0; }
    
    public VolumeUp(int _PercentIncrease)
    { PercentIncrease = _PercentIncrease; }
    
    public new void RunAdjustment()
    { Debug.WriteLine("Increasing volume!"); }
}


class Song
{
    private List<(IAdjustment Adjustment, bool Adjusted)> _OtherAdjustments = new();
    
    public List<Effect> Effects { get; set; }

    public List<IAdjustment> OtherAdjustments
    {
        get => _OtherAdjustments.Select(X => X.Adjustment).ToList();
        set => _OtherAdjustments = value.Select(X => (X, false)).ToList();
    }
    public string Name { get; set; }
    public string ArtistName { get; set; }
    public string Album { get; set; }
    public long TrackLength { get; set; }
    
    public void RunEffects()
    {
        foreach (var Efct in Effects)
        { Efct.RunAdjustment(); }
    }

    public async Task TriggerAdjustments()
    {//trigger at somepoint after _OtherAdjustments is set

        (IAdjustment Adjustment, bool Adjusted) Adj;
        
        for (int i = 0; i < _OtherAdjustments.Count; i++)
        {
            Adj = _OtherAdjustments[i]; 
            
            if (!Adj.Adjusted)
            {
                Adj.Adjusted = true;
                Adj.Adjustment.RunAdjustment();
            }
        }
    }
}