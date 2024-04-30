using SharpAudio;
using SharpAudio.Codec;

namespace AudioTest;

internal class Program
{
    private static readonly string FileLoc = $"Assets{Path.DirectorySeparatorChar}82-99 F.M.wav";

    static void Main(string[] args)
    {
        Console.Clear();

        //Thread.Sleep(3000);

        SA();
    }

    private static void SA()
    {
        var ENG = AudioEngine.CreateDefault();
        var SoundA = new SoundStream(File.OpenRead(FileLoc), ENG);
        var SoundB = new SoundStream(File.OpenRead(FileLoc), ENG);

        SoundA.Volume = 0.0125f;
        //SoundB.Volume = 0.0125f;

        SoundA.Play();
        //change to push
        Thread.Sleep(300);

        SoundB.Play();

        SoundB.PropertyChanged += SoundB_PropertyChanged;
    }

    private static void SoundB_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Console.WriteLine(e.PropertyName);
    }
}
