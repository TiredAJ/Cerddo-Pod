using ManagedBass;

namespace AudioTest;

internal class Program
{
    private static readonly string AssetsLoc = $"Assets{Path.DirectorySeparatorChar}";
    private static List<string> Files = new();
    private static Random rng = new Random(DateTime.Now.Millisecond);

    static void Main(string[] args)
    {
        Console.Clear();
        Console.CursorVisible = false;

        Bass.Init(-1, 44100, DeviceInitFlags.Stereo, nint.Zero);

        //Thread.Sleep(3000);

        foreach (var F in Directory.GetFiles(AssetsLoc))
        {
            if (Path.GetExtension(F) != ".txt")
            { Files.Add(Path.GetFileName(F)); }
        }

        //SA();

        foreach (var F in Files.OrderBy(_ => rng.Next()))
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
        Console.SetCursorPosition(0, 0);

        int S = Bass.CreateStream(_Song);

        Console.WriteLine($"Now playing: {_Song}");

        //Bass.GlobalMusicVolume = 1;
        Bass.GlobalStreamVolume = 600;

        if (S != 0)
        { Bass.ChannelPlay(S, true); }
        else
        { Console.WriteLine($"Error! {Bass.LastError}"); }

        Thread.Sleep(8000);

        //works as expected
        Bass.ChannelSetPosition(S, 0);

        ChannelInfo CI;
        double SLength = Bass.ChannelBytes2Seconds(S, Bass.ChannelGetLength(S));

        while (SLength > Bass.ChannelBytes2Seconds(S, Bass.ChannelGetPosition(S)))
        {
            Bass.ChannelGetInfo(S, out CI);
            Console.SetCursorPosition(0, 1);
            Console.WriteLine($"{SLength.TimeDisplay()} : {Bass.ChannelBytes2Seconds(S, Bass.ChannelGetPosition(S)).TimeDisplay()}    ");
        }
    }
}
