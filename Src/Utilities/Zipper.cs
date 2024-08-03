using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Text;
using Utilities.Logging;

namespace Utilities.Zipping;

public class Zipper
{
    private static Logger Log = null!;

    public Zipper()
    { Log = LoggerBuilder.Init().NewLogger().UseDefaultLoc().LogName("Zipper").Build(); }
    
    /// <summary>
    /// Extracts the input file to the output directory
    /// </summary>
    /// <param name="_ZipSource">Path to .tar.gz file to extract</param>
    /// <param name="_Destination">Directory to extract to</param>
    public static void Extract(string _ZipSource, string _Destination)
    {
        //https://github.com/icsharpcode/SharpZipLib/wiki/GZip-and-Tar-Samples#anchorTar
        Log.Info($"Extracting [{_ZipSource}] -> [{_Destination}]");

        try
        {
            using (GZipInputStream GZS = new(File.OpenRead(_ZipSource)))
            {
                TarArchive TA = TarArchive.CreateInputTarArchive(GZS, Encoding.UTF8);
                TA.ExtractContents(_Destination);
                TA.Close();
                Log.Info("Closing archive.");
            }
        }
        catch (Exception EXC)
        { throw Log.FatalThrow(EXC.Message); }        
    }

    public static void Compress(string _ZipDestination, string _Source)
    {
        Log.Info($"Compressing folder to [{_ZipDestination}].");
        
        Stream OutStream = File.Create(_ZipDestination);
        GZipOutputStream GZOutStream = new(OutStream);
        
        //hmmm, hasn't seemed to affect the size of the output
        GZOutStream.SetLevel(9);
        
        TarArchive TA = TarArchive.CreateOutputTarArchive(GZOutStream, Encoding.UTF8);
        
        TA.RootPath = _Source.Replace('\\', '/');

        if (TA.RootPath.EndsWith('/'))
        { TA.RootPath = TA.RootPath.Remove(TA.RootPath.Length - 1); }

        AddDirectoryFilesToTar(TA, _Source);
        
        TA.Close();
        
        Log.Info("Closing archive.");
    }

    private static void AddDirectoryFilesToTar(TarArchive TA, string _Source)
    {
        //the recurse option is left false for both because it's just unnecessary
        //for this. Ideally there's only 1 folder with all the audio in it, unless
        //we restructure so the folder contains the binaries and a nested folder
        //contains the music, but idk atm, not crucial
        
        TarEntry TE = TarEntry.CreateEntryFromFile(_Source);
        TA.WriteEntry(TE, false);

        Log.Info("Adding files to archive.");
        
        string[] Files = Directory.GetFiles(_Source);

        foreach (string F in Files)
        {
            Log.Info($"Adding {F} to archive.");
            TE = TarEntry.CreateEntryFromFile(F);
            TA.WriteEntry(TE, false);
        }
        
        Log.Info($"Finished adding {Files.Length} file(s) to archive.");
    }
}
