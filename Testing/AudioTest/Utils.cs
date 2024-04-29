using Pie;
using Pie.Audio;
using Pie.Audio.Stream;

using PieStream = Pie.Audio.Stream.Stream;

namespace AudioTest.Utils;

public class PieStreamPlayer : IDisposable
{
    private AudioDevice AD;
    private PieStream Strm;
    private AudioFormat AF;
    private byte[] Buff;
    private AudioBuffer[] Buffs;
    private int CurBuff;
    private ushort Vo;
    private object Lock = new object();

    public PieStreamPlayer(AudioDevice _AD, string _Path)
    {
        AD = _AD;
        Strm = PieStream.FromFile(_Path);

        

        //gay 29/04/2024 21:43
    }
}
