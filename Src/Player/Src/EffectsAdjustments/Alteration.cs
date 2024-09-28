using Player;

namespace Player.EffectsAdjustments;

public interface IAlteration
{ public SongData Run(SongData _Input); }

public abstract class Effect : IAlteration
{
    public long StartPos { get; set; }
    public long EndPos { get; set; }

    public abstract SongData Run(SongData _Input);
}

public abstract class Adjustment : IAlteration
{
    private bool HasRun { get; set; }

    public abstract SongData Run(SongData _Input);
}
