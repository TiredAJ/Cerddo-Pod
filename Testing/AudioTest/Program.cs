using ATL;
using ManagedBass;
using SixLabors.ImageSharp;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace AudioTest;

internal class Program
{
    private static readonly string AssetsLoc = $"Assets{Path.DirectorySeparatorChar}";
    private static List<string> Files = new();
    private static Random rng = new Random(DateTime.Now.Millisecond);
    private static SyncProcedure Syncer = new(EndSync);
    private static bool EndOfSong = false;
    private static ManualResetEvent EndSignal = new ManualResetEvent(false);
    private static Track CurTrack;
    private static Process? ImageView;

    static void Main(string[] args)
    {
        Console.Clear();
        Console.CursorVisible = false;
        Console.OutputEncoding = Encoding.UTF8;

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

    private static string? MDExtract(string _Song)
    {
        //Albulm cover \/
        CurTrack = new Track(_Song);

        //Console.WriteLine(SongMD.EmbeddedPictures.Count);

        if (CurTrack.EmbeddedPictures.Count > 0)
        {
            var First = CurTrack.EmbeddedPictures.First();

            using (var Img = Image.Load(new MemoryStream(First.PictureData)))
            { Img.SaveAsJpeg($"{First.PictureHash}.jpg"); }

            return $"{First.PictureHash}.jpg";
        }
        else
        { return default; }
    }

    private static void MB(string _Song)
    {
        Console.SetCursorPosition(0, 0);

        int S = Bass.CreateStream(_Song);

        Bass.ChannelSetSync(S, SyncFlags.End | SyncFlags.Mixtime, 0, Syncer);

        Console.WriteLine($"Now playing: {_Song}");

        //Bass.GlobalMusicVolume = 1;
        Bass.GlobalStreamVolume = 600;

        if (S != 0)
        { Bass.ChannelPlay(S, true); }
        else
        { Console.WriteLine($"Error! {Bass.LastError}"); }

        EndOfSong = false;

        //Thread.Sleep(8000);

        //works as expected
        //Bass.ChannelSetPosition(S, 0);

        DisplayInfo(_Song, S);

        EndSignal.WaitOne();
        EndSignal.Reset();
    }

    private static void EndSync(int _Handle, int _Chnl, int _Data, IntPtr _Usr)
    {
        Debug.WriteLine("End of stream!");
        EndOfSong = true;
        EndSignal.Set();
    }

    private static async Task DisplayInfo(string _SongFile, int _S)
    {
        Console.Clear();
        Console.SetCursorPosition(0, 0);

        string? ImgLoc = MDExtract(_SongFile);

        if (CurTrack?.Title == null)
        { Console.WriteLine($"Now playing: {_SongFile}"); }
        else
        { Console.WriteLine($"Now Playing: {CurTrack.Title}"); }

        if (ImgLoc is not null)
        { await OpenInApp(ImgLoc); }
        else
        { Debug.WriteLine("No image!"); }

        ChannelInfo CI;
        double SLength = Bass.ChannelBytes2Seconds(_S, Bass.ChannelGetLength(_S));

        while (!EndOfSong)
        {
            Bass.ChannelGetInfo(_S, out CI);
            Console.SetCursorPosition(0, 1);
            Console.WriteLine($"{SLength.TimeDisplay()} : {Bass.ChannelBytes2Seconds(_S, Bass.ChannelGetPosition(_S)).TimeDisplay()}        ");
        }
    }

    private static async Task OpenInApp(string _FileLoc)
    {
        await Task.Run(() =>
        {
            ProcessStartInfo PSI = new ProcessStartInfo();
            PSI.RedirectStandardError = false;
            PSI.RedirectStandardOutput = false;
            PSI.ErrorDialog = false;

            if (ImageView is not null)
            {
                ImageView.Kill();
                ImageView.Close();
            }

            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                { PSI.FileName = "cmd"; }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                { PSI.FileName = "xdg-open"; }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                { PSI.FileName = "open"; }

                PSI.Arguments = $"/c {_FileLoc}";

                ImageView = new Process() { StartInfo = PSI };
                ImageView.Start();
            }
            catch (Exception EXC)
            { Debug.WriteLine(EXC.Message); }
        });
    }
}
