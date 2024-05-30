using NUnit.Framework;
using NUnit.Framework.Api;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Utilities.Logging;
using Utilities.Zipping;

namespace Player.Testing;

[TestFixture]
public class ZipperTester
{
    //passed - 30/05/24 @ 22:19
    [Test]
    public void ExtractTester()
    {
        string ZipLocation = "C:\\Users\\gecko\\Desktop\\Files.tar.gz";
        string Destination = "C:\\Users\\gecko\\Desktop\\AudioFiles";
        string OriginalFiles = "E:\\GitHub\\Cerddo-Pod\\Testing\\AudioFiles";

        if (Directory.Exists(Destination))
        {
            foreach (var F in Directory.GetFiles(Destination))
            { File.Delete(F); }
            
            Directory.Delete(Destination);
        }
        
        /*************************************************/
        Zipper.Extract(ZipLocation, Destination);
        /*************************************************/

        Assert.That(Directory.Exists(Destination), "Directory not made");
        Assert.That(
            () =>
            {
                var Original = Directory.GetFiles(OriginalFiles)
                    .Select(X => Path.GetFileName(X))
                    .Order();
                var New = Directory.GetFiles(Destination)
                    .Select(X => Path.GetFileName(X))
                    .Order();

                return Original.SequenceEqual(New);
            },
            "Files do not match!"
        );
    }

    //passed - 30/05/24 @ 22:19
    [Test]
    public void CompressTester()
    {
        var ZipDestination = "C:\\Users\\gecko\\Desktop\\Files.tar.gz";
        var DirSource = "E:\\GitHub\\Cerddo-Pod\\Testing\\AudioFiles";

        if (File.Exists(ZipDestination))
        { File.Delete(ZipDestination); }
        
        /*****************************************/
        Zipper.Compress(ZipDestination, DirSource);
        /*****************************************/
        
        Assert.That(File.Exists(ZipDestination), "Zip doesn't exist!");
    }
}
