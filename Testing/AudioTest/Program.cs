using ManagedBass;
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

        //SA();

        MB();

        //MDExtract();
    }

    private static void SA()
    {
        var ENG = AudioEngine.CreateDefault();
        var SoundA = new SoundStream(File.OpenRead(FileLoc), ENG);

        //SoundB.Volume = 0.0125f;
        SoundA.Play();
        SoundA.Volume = 0.0125f;

        //change to push
        Thread.Sleep(5000);

        Console.WriteLine($"Pos:{SoundA.Position}");
        Console.WriteLine("Stopping");
        SoundA.Stop();
        Console.WriteLine("Playing");

        SoundA.Play();
        Console.WriteLine("Should be playing");
    }


    private static void MDExtract()
    {
        //var Dir = ImageMetadataReader.ReadMetadata(FileLoc);

        //foreach (var D in Dir)
        //{
        //    foreach (var T in D.Tags)
        //    { Console.WriteLine($"{D.Name} - {T.Name} = {T.Description}"); }
        //}


        /*Albulm cover \/*/
        //Track SongMD = new Track(FileLoc);

        //Console.WriteLine(SongMD.EmbeddedPictures.Count);

        //foreach (var Pi in SongMD.EmbeddedPictures)
        //{
        //    var Img = SDImg.FromStream(new MemoryStream(Pi.PictureData));
        //
        //    Img.Save($"{SongMD.Title}-{Pi.PictureHash}.jpg", ImgForm.Jpeg);
        //}
    }

    private static void MB()
    {
        Bass.Init(-1, 44100, DeviceInitFlags.Stereo, nint.Zero);

        int S = Bass.CreateStream(FileLoc);

        Bass.GlobalMusicVolume = 1;

        if (S != 0)
        { Bass.ChannelPlay(S, true); }

        Thread.Sleep(3000);

        Console.WriteLine($"Pause successful: {(Bass.Pause() ? "true" : "false")}");
        Console.WriteLine(Bass.LastError);

        Thread.Sleep(1000);
        Bass.Start();
        Console.WriteLine(Bass.LastError);

        do
        {

        } while (true);
    }
}
