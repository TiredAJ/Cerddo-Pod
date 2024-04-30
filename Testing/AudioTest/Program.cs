using NAudio.Wave;
using NetCoreAudio;
using System.Reflection;

namespace AudioTest;

internal class Program
{
    private static readonly string FileLoc = "Assets\\82-99 F.M.wav";

    //private static readonly Uri FileLoc = new Uri("Assets/82-99 F.M.wav", UriKind.Relative);
    //private static readonly Uri FileLoc = new Uri("\"E:\\GitHub\\MP3-Pod\\Testing\\AudioTest\\bin\\Debug\\net8.0\\Assets\\82-99 F.M.wav\"", UriKind.Absolute);

    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        Console.Clear();

        //Thread.Sleep(3000);

        Console.WriteLine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        if (Directory.Exists(FileLoc))
        {
            foreach (var F in Directory.GetFiles("AudioTest:///Assets/"))
            { Console.WriteLine(F); }
        }
        else
        { Console.WriteLine("No :("); }


        NA();

        //NCA();


    }

    private static void NCA()
    {
        Player P = new Player();

        P.Play(FileLoc);

        P.PlaybackFinished += (object? Sender, EventArgs e) => { Console.WriteLine("Stopped!"); };

        string Input;
        byte Vol = 0;
        bool bVol = false, Loop = true;

        while (Loop)
        {
            Console.WriteLine("Please enter a command: [p] to toggle pause, [q] to quit " +
            "[0-100] for volume");

            Input = Console.ReadLine().ToLower();
            bVol = byte.TryParse(Input, out Vol);

            switch (Input)
            {
                case "p":
                { _ = P.Paused ? P.Resume() : P.Pause(); break; }
                case "q":
                { P.Stop(); Loop = false; break; }
                default:
                {
                    if (bVol)
                    { P.SetVolume(Vol); bVol = false; }
                    else
                    { Console.WriteLine("Invalid command"); }
                    break;
                }
            }
        }
        Console.WriteLine("Stopping...");
    }

    private static void NA()
    {
        using (var AF = new AudioFileReader(FileLoc))
        using (var OutDev = new WaveOutEvent())
        {
            OutDev.Init(AF);
            OutDev.Play();

            OutDev.Volume = 0.25f;

            while (OutDev.PlaybackState == PlaybackState.Playing)
            { Thread.Sleep(2500); }

            OutDev.Stop();
        }
    }
}
