using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json.Serialization.Metadata;

namespace YAMLTesting;

class Program
{
    private Song Temp = new Song()
    {
        Name = "Up & Up",
        Album = "A Head Full of Dreams",
        ArtistName = "Coldplay",
        TrackLength = 405L,
        Effects = [ new Fade(345L, 571L), new VolumeUp()]
    }
    
    static void Main(string[] args)
    { 
        
    }
}

public interface IAdjustment
{
    public void RunAdjustment();
}

abstract class Effect : IAdjustment
{
    public long StartPos { get; set; }
    public long EndPos { get; set; }

    public void RunAdjustment()
    { Debug.WriteLine("Running base adjustment!"); }
}

class AdjustedEnds : IAdjustment
{
    public AdjustedEnds(long _NewStartPos, long _NewEndPos, ref Song _AffectedSong)
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
        
    }

    public async TriggerAdjustments()
    {//trigger at somepoint after _OtherAdjustments is set

        foreach (var Adj in _OtherAdjustments)
        {
            if (!Adj.Adjusted)
            {
                Adj.Adjusted = true;
            }
        }
        
    }
}