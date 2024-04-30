using MetadataExtractor;
using SharpAudio;
using SharpAudio.Codec;

namespace AudioTest;

internal class Program
{
    private static readonly string FileLoc = $"Assets{Path.DirectorySeparatorChar}SteviaSphere_Dolphin.mp3";

    static void Main(string[] args)
    {
        Console.Clear();

        //Thread.Sleep(3000);

        SA();
        //MDExtract();
    }

    private static void SA()
    {
        var ENG = AudioEngine.CreateDefault();
        var SoundA = new SoundStream(File.OpenRead(FileLoc), ENG);
        var SoundB = new SoundStream(File.OpenRead(FileLoc), ENG);

        //SoundB.Volume = 0.0125f;
        SoundA.Play();
        SoundA.Volume = 0.0125f;

        //change to push
        Thread.Sleep(300);

        SoundB.Play();

        SoundB.PropertyChanged += SoundB_PropertyChanged;
    }

    private static void SoundB_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Console.WriteLine(e.PropertyName);
        Console.WriteLine((sender as SoundStream));


    }

    private static void MDExtract()
    {
        var Dir = ImageMetadataReader.ReadMetadata(FileLoc);

        foreach (var D in Dir)
        {
            foreach (var T in D.Tags)
            { Console.WriteLine($"{D.Name} - {T.Name} = {T.Description}"); }
        }
    }
}
