namespace Player.EffectsAdjustments;

class Adj_AdjustedEnds : Adjustment
{
    public long NewStartPos { get; set; }
    public long NewEndPos { get; set; }

    public Adj_AdjustedEnds(long _NewStartPos, long _NewEndPos)
    {
        NewStartPos = _NewStartPos;
        NewEndPos = _NewEndPos;
    }
    
    public override SongData Run(SongData _Input)
    {
        
    }
}

