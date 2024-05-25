using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Text;

namespace Utilities;

public class Zipper
{
    /// <summary>
    /// Extracts the input file to the output directory
    /// </summary>
    /// <param name="_ZipSource">Path to .tar.gz file to extract</param>
    /// <param name="_Destination">Directory to extract to</param>
    public static void Extract(string _ZipSource, string _Destination)
    {
        //https://github.com/icsharpcode/SharpZipLib/wiki/GZip-and-Tar-Samples#anchorTar
        Logger.Log($"Extracting {_ZipSource} to {_Destination}");

        try
        {
            using (var GZS = new GZipInputStream(File.OpenRead(_ZipSource)))
            {
                TarArchive TA = TarArchive.CreateInputTarArchive(GZS, Encoding.UTF8);
                TA.ExtractContents(_Destination);
                TA.Close();
                Logger.Log("Closing archive.");
            }
        }
        catch (Exception EXC)
        { throw Logger.LogThrow(EXC.Message); }        
    }

    public static void Compress(string _ZipDestination, string _Source)
    {
        Logger.Log($"Compressing folder to [{_ZipDestination}].");
        
        Stream OutStream = File.Create(_ZipDestination);
        Stream GZOutStream = new GZipOutputStream(OutStream);
        TarArchive TA = TarArchive.CreateOutputTarArchive(GZOutStream, Encoding.UTF8);

        TA.RootPath = _Source.Replace('\\', '/');

        if (TA.RootPath.EndsWith("/"))
        { TA.RootPath = TA.RootPath.Remove(TA.RootPath.Length - 1); }

        AddDirectoryFilesToTar(TA, _Source);
        
        TA.Close();
        
        Logger.Log("Closing archive.");
    }

    private static void AddDirectoryFilesToTar(TarArchive TA, string _Source)
    {
        //the recurse option is left false for both because it's just unnecessary
        //for this. Ideally there's only 1 folder with all the audio in it, unless
        //we restructure so the folder contains the binaries and a nested folder
        //contains the music, but idk atm, not crucial
        
        TarEntry TE = TarEntry.CreateEntryFromFile(_Source);
        TA.WriteEntry(TE, false);

        Logger.Log("Adding files to archive.");
        
        var Files = Directory.GetFiles(_Source);

        foreach (var F in Files)
        {
            TE = TarEntry.CreateEntryFromFile(F);
            TA.WriteEntry(TE, false);
        }
        
        Logger.Log($"Finished adding {Files.Length} file(s) to archive.");
    }
}