using NetCoreAudio;
using System.Reflection;

using AudioTest.Utils;

namespace AudioTest;

internal class Program
{
    private static readonly string FileLoc = "/run/media/tiredaj/AJStore/GitHub/MP3-Pod/Testing/AudioTest/bin/Debug/net8.0/Assets/82-99 F.M.wav";

    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        Console.Clear();

        //Thread.Sleep(3000);

        Console.WriteLine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        //var F = Directory.GetFiles(), "", SearchOption.AllDirectories);

        //foreach (var f in F)
        //{ Console.WriteLine(f); }

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
                { P.Stop(); Loop = false;  break; }
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

    private void XTTest()
    {
        XTSample XTS = new XTSample();
    }
}
