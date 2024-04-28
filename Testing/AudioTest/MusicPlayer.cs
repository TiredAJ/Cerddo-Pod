using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;

namespace AudioTest;

public class MusicPlayer
{
    private ISoundOut SoundOut;
    private IWaveSource WaveSource;

    public void Load(string _FileLoc)
    {
        Reset();

        WaveSource = CodecFactory.Instance
                        .GetCodec(_FileLoc)
                        .ToSampleSource()
                        .ToWaveSource();
        SoundOut = new WasapiOut() { Latency = 100 };
        SoundOut.Initialize(WaveSource);
        SoundOut.Volume = 1f;
    }

    public void Play()
    {
        if (SoundOut is not null)
        { SoundOut.Play(); }
    }

    public void Stop()
    {
        if (SoundOut is not null)
        { SoundOut.Stop(); }
    }

    public void Volume(float _Vol)
    { SoundOut.Volume = _Vol / 100f; }

    private void Reset()
    {
        if (SoundOut is not null)
        {
            SoundOut.Dispose();
            SoundOut = null;
        }

        if (WaveSource != null)
        {
            WaveSource.Dispose();
            WaveSource = null;
        }
    }
}
