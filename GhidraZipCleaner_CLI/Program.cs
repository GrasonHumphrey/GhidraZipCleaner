using System.Security.Cryptography;

StreamReader srCGZF = null;
StreamReader srROM = null;

string gzfFilename = null;
string cgzfFilename = null;

byte[] romBytes = null;

bool import = false;

byte[] XOR_All_Bytes(byte[] romBytes, byte[] ghidraBytes)
{
    for (int i = 0; i < ghidraBytes.Length; i++)
    {
        ghidraBytes[i] ^= romBytes[i % romBytes.Length];
    }
    return ghidraBytes;
}


if (args.Length != 4)
{
    Console.WriteLine("Need exactly 4 arguments (don't forget quotes around file paths if they have spaces):");
    Console.WriteLine("1: import (cgzf->gzf)  or export (gzf->cgzf)");
    Console.WriteLine("2: GZF/CGZF file");
    Console.WriteLine("3: ROM binary location");
    Console.WriteLine("4: Output folder");
    return;
}

if (args[0].ToString().ToLower().Equals("import"))
{
    import = true;
}
else if (args[0].ToString().ToLower().Equals("export"))
{
    import = false;
}
else
{
    Console.WriteLine("Error: First argument must either be import (cgzf->gzf) or export (gzf->cgzf)");
    return;
}

// Check for minimum file name length
if ((import && args[1].Length < 6) || (!import && args[1].Length < 5))
{
    Console.WriteLine("Error: Second arg (GZF/CGZF file) is not a valid file (be sure to include the extension)");
    return;
}

// Check for correct file type
if ((import && !args[1].Substring(args[1].Length - 5, 5).Equals(".cgzf")) ||
    (!import && !args[1].Substring(args[1].Length - 4, 4).Equals(".gzf")))
{
    Console.WriteLine("Error: Second arg (GZF/CGZF file) must be of filetype .cgzf for import, filetype .gzf for export");
    return;
}

// Check for output location
DirectoryInfo outputDirectory = new DirectoryInfo(args[3]);
if (!outputDirectory.Exists)
{
    Console.WriteLine("Error: Output directory does not exist");
    return;
}

// Try to load GZF/CGZF
try
{ 
    srCGZF = new StreamReader(args[1]);

    int startIndex = Math.Max(args[1].LastIndexOf("\\"), 0);
    int endIndex = Math.Max(args[1].LastIndexOf("."), 0);
    cgzfFilename = args[1].Substring(startIndex, endIndex-startIndex);
}
catch (Exception ex)
{
    Console.WriteLine($"Error loading GZF/CGZF.\n\nError message: {ex.Message}\n\n" +
    $"Details:\n\n{ex.StackTrace}");
    return;
}

// Try to load ROM file
try
{
    srROM = new StreamReader(args[2]);

    using (var memstream = new MemoryStream())
    {
        srROM.BaseStream.CopyTo(memstream);
        romBytes = memstream.ToArray();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error loading ROM file.\n\nError message: {ex.Message}\n\n" +
    $"Details:\n\n{ex.StackTrace}");
    return;
}

// Read the GZF/CGZF
byte[] cgzfBytes = null;
using (var memstream = new MemoryStream())
{
    srCGZF.BaseStream.CopyTo(memstream);
    cgzfBytes = memstream.ToArray();
}

byte[] outBytes;
string path;
if (import)
{
    // Don't decode the last 16 bytes which make the checksum
    outBytes = XOR_All_Bytes(romBytes, cgzfBytes.Take(cgzfBytes.Length - 16).ToArray());
    byte[] retrievedHash = cgzfBytes.Skip(cgzfBytes.Length - 16).ToArray();

    using (var md5 = MD5.Create())
    {
        byte[] calculatedHash = md5.ComputeHash(outBytes);
        if (!Enumerable.SequenceEqual(calculatedHash, retrievedHash))
        {
            Console.WriteLine("Error: File checksum does not match expected. \n\n" +
                "This likely means the ROM used does not match the one used to generate the .cgzf");
            return;
        }
    }
    path = outputDirectory.ToString() + cgzfFilename + "_DECODE.gzf";
} else
{
    outBytes = new byte[cgzfBytes.Length + 16];

    using (var md5 = MD5.Create())
    {
        byte[] hash = md5.ComputeHash(cgzfBytes);
        XOR_All_Bytes(romBytes, cgzfBytes).CopyTo(outBytes, 0);
        hash.CopyTo(outBytes, cgzfBytes.Length);
    }
    path = outputDirectory.ToString() + cgzfFilename + "_CLEAN.cgzf";
}

// Save file
try
{
    // Delete the file if it exists.
    if (File.Exists(path))
    {
        File.Delete(path);
    }

    FileStream fs = File.Create(path);
    fs.Write(outBytes, 0, outBytes.Length);
    fs.Close();
    //srROM = new StreamReader(openFileDialogROM.FileName);
    //srCGZF = new StreamReader(openFileDialogCleanGZF.FileName);
    Console.WriteLine("Conversion success!");
    Console.WriteLine("Output file: " + path);
}
catch (Exception ex)
{
    Console.WriteLine($"Error saving file.\n\nError message: {ex.Message}\n\n" +
    $"Details:\n\n{ex.StackTrace}");
}
