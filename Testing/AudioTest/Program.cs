using ManagedBass;

namespace AudioTest;

internal class Program
{
    private static readonly string AssetsLoc = $"Assets{Path.DirectorySeparatorChar}";
    private static List<string> Files = new();
    static void Main(string[] args)
    {
        Console.Clear();

        //Thread.Sleep(3000);

        foreach (var F in Directory.GetFiles(AssetsLoc))
        {
            if (Path.GetExtension(F) != ".txt")
            { Files.Add(Path.GetFileName(F)); }
        }

        //SA();

        foreach (var F in Files)
        { MB($"{AssetsLoc}{F}"); }

        //MDExtract();
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

    private static void MB(string _Song)
    {
        Bass.Init(-1, 44100, DeviceInitFlags.Stereo, nint.Zero);

        int S = Bass.CreateStream(_Song);

        Console.WriteLine($"Now playing: {_Song}");

        Bass.GlobalMusicVolume = 1;
        //Bass.GlobalStreamVolume = 6;

        if (S != 0)
        { Bass.ChannelPlay(S, true); }
        else
        { Console.WriteLine($"Error! {Bass.LastError}"); }

        Thread.Sleep(3000);

        //somehow need to get this to block until the stream's finished



        //Thread.Sleep(1000);
        //Bass.Start();
        //Console.WriteLine(Bass.LastError);
    }
}
