using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Text;

namespace Utilities;

public class Zipper
{
    /// <summary>
    /// Extracts the input file to the output directory
    /// </summary>
    /// <param name="_ZipLocation">Path to .tar.gz file to extract</param>
    /// <param name="_Destination">Directory to extract to</param>
    public static void Extract(string _ZipLocation, string _Destination)
    {
        //https://github.com/icsharpcode/SharpZipLib/wiki/GZip-and-Tar-Samples#anchorTar
        Logger.Log($"Extracting {_ZipLocation} to {_Destination}");

        try
        {
            using (var GZS = new GZipInputStream(File.OpenRead(_ZipLocation)))
            {
                TarArchive TA = TarArchive.CreateInputTarArchive(GZS, Encoding.Default);
                TA.ExtractContents(_Destination);
                TA.Close();
            }
        }
        catch (Exception EXC)
        { throw Logger.LogThrow(EXC.Message); }        
    }
}