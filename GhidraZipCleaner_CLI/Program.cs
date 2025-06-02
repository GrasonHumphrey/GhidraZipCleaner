using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

StreamReader srCGZF = null;
StreamReader srROM = null;

string gzfFilename = null;
string cgzfFilename = null;

byte[] romBytes = null;

bool import = false;

int versionMajor = 2;
int versionMinor = 0;

int versionInfoLen = 2;

bool forceVersion = false;
byte[] headerIDStr = { (byte) 'C', (byte) 'G', (byte) 'Z', (byte) 'F' };

Console.WriteLine();
Console.WriteLine("GhidraZipCleaner version " + versionMajor + "." + versionMinor);
Console.WriteLine();

byte[] XOR_All_Bytes(byte[] romBytes, byte[] ghidraBytes)
{
    for (int i = 0; i < ghidraBytes.Length; i++)
    {
        ghidraBytes[i] ^= romBytes[i % romBytes.Length];
    }
    return ghidraBytes;
}

byte[] Encrypt_With_ROM(byte[] romBytes, byte[] ghidraBytes, byte[] salt)
{
    byte[] keyInfo = Get_Key_Info(romBytes, salt);
    byte[] encrypted;

    using (Aes aesAlg = Aes.Create())
    {
        aesAlg.IV = keyInfo.Take(16).ToArray();
        aesAlg.Key = keyInfo.Skip(32).ToArray();
        aesAlg.Padding = PaddingMode.PKCS7;

        // Create an encryptor to perform the stream transform.
        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using (MemoryStream msEncrypt = new MemoryStream())
        {
            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                csEncrypt.Write(ghidraBytes);
            }
            encrypted = msEncrypt.ToArray();
        }
    }
    byte[] outBytes = new byte[encrypted.Length + salt.Length];
    encrypted.CopyTo(outBytes, 0);
    salt.CopyTo(outBytes, encrypted.Length);
    return outBytes;
}

byte[] Decrypt_With_ROM(byte[] romBytes, byte[] inBytes, int saltLen)
{
    byte[] salt = inBytes.Skip(inBytes.Length - saltLen).ToArray();
    byte[] encryptedBytes = inBytes.Take(inBytes.Length - saltLen).ToArray();

    byte[] keyInfo = Get_Key_Info(romBytes, salt);
    byte[] decrypted;

    using (Aes aesAlg = Aes.Create())
    {
        aesAlg.IV = keyInfo.Take(16).ToArray();
        aesAlg.Key = keyInfo.Skip(32).ToArray();
        aesAlg.Padding = PaddingMode.PKCS7;

        // Create a decryptor to perform the stream transform.
        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        // Create the streams used for decryption.
        using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
        {
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            {
                decrypted = new byte[encryptedBytes.Length];
                for (int i = 0; i < encryptedBytes.Length; i++)
                {
                    decrypted[i] = (byte) csDecrypt.ReadByte();                    
                }
                
                /*
                byte[] readBuffer = new byte[encryptedBytes.Length];
                int numBytesRead = csDecrypt.Read(readBuffer);
                
                decrypted = new byte[numBytesRead];
                readBuffer.Take(numBytesRead).ToArray().CopyTo(decrypted, 0);
                */
            }
        }
    }
    return decrypted;
}

byte[] Get_Key_Info(byte[] romBytes, byte[] salt)
{
    using (SHA512 sha512 = SHA512.Create())
    {
        byte[] inputBytes = new byte[romBytes.Length + salt.Length];
        romBytes.CopyTo(inputBytes, 0);
        salt.CopyTo(inputBytes, romBytes.Length);
        return sha512.ComputeHash(inputBytes);
    }
}

if (args.Length != 4)
{
    if (args.Length == 5)
    {
        try
        {
            versionMajor = int.Parse(args[4]);
            forceVersion = true;
            if (versionMajor == 1)
            {
                Console.WriteLine("WARNING!!!  Forcing legacy mode, which can generate cgzf's that are not copyright safe.");
                Console.WriteLine("This should only be used to import old cgzf's generated with the old version of GhidraZipCleaner.");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("Using version: " + versionMajor.ToString());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Need exactly 4 arguments (don't forget quotes around file paths if they have spaces):");
            Console.WriteLine("1: import (cgzf->gzf)  or export (gzf->cgzf)");
            Console.WriteLine("2: GZF/CGZF file");
            Console.WriteLine("3: ROM binary location");
            Console.WriteLine("4: Output folder");
            return;
        }
        
    }
    else
    {
        Console.WriteLine("Need exactly 4 arguments (don't forget quotes around file paths if they have spaces):");
        Console.WriteLine("1: import (cgzf->gzf)  or export (gzf->cgzf)");
        Console.WriteLine("2: GZF/CGZF file");
        Console.WriteLine("3: ROM binary location");
        Console.WriteLine("4: Output folder");
        return;
    }
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

int saltLen = 16;

if (import)
{
    if (!forceVersion)
    {
        // Check CGZF version
        bool headerStrFound = true;
        for (int i = 0; i < headerIDStr.Length; i++)
        {
            if (!cgzfBytes[i].Equals(headerIDStr[i]))
            {
                headerStrFound = false; break;
            }
        }

        if (headerStrFound)
        {
            versionMajor = cgzfBytes[headerIDStr.Length];
            versionMinor = cgzfBytes[headerIDStr.Length + 1];
            Console.WriteLine("Importing from version: " + versionMajor + "." + versionMinor);
            Console.WriteLine();
        }
        else
        {
            // No header ID found, meaning legacy edition
            versionMajor = 1;
            versionMinor = 0;
        }
    }
    

    // Don't decode the last 16 bytes which make the checksum
    if (versionMajor == 1)
    {
        outBytes = XOR_All_Bytes(romBytes, cgzfBytes.Take(cgzfBytes.Length - 16).ToArray());
    }
    else
    {
        outBytes = Decrypt_With_ROM(romBytes, cgzfBytes.Take(cgzfBytes.Length - 16).Skip(headerIDStr.Length + versionInfoLen).ToArray(), saltLen);
    }

    byte[] retrievedHash = cgzfBytes.Skip(cgzfBytes.Length - 16).ToArray();

    using (var md5 = MD5.Create())
    {
        byte[] calculatedHash;
        if (versionMajor == 1)
        {
            Console.WriteLine("Importing with legacy mode...");
            calculatedHash = md5.ComputeHash(outBytes);
        }
        else
        {
            calculatedHash = md5.ComputeHash(outBytes.Take(400).ToArray());
        }
        
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
    // Generate random salt
    byte[] salt = new byte[saltLen];

    Random rand = new Random();
    rand.NextBytes(salt);

    using (var md5 = MD5.Create())
    {
        byte[] hash;
        if (versionMajor == 1)
        {
            outBytes = new byte[cgzfBytes.Length + 16];
            hash = md5.ComputeHash(cgzfBytes);
            XOR_All_Bytes(romBytes, cgzfBytes).CopyTo(outBytes, 0);
            hash.CopyTo(outBytes, cgzfBytes.Length);
        }
        else
        {
            hash = md5.ComputeHash(cgzfBytes.Take(400).ToArray());
            byte[] encryptedBytes = Encrypt_With_ROM(romBytes, cgzfBytes, salt);
            // Outbytes needs to hold header ID, version number, encrypted bytes, and hash
            outBytes = new byte[headerIDStr.Length + versionInfoLen + encryptedBytes.Length + hash.Length];
            headerIDStr.CopyTo(outBytes, 0);
            outBytes[headerIDStr.Length] = (byte) versionMajor;
            outBytes[headerIDStr.Length + 1] = (byte) versionMinor;
            encryptedBytes.CopyTo(outBytes, headerIDStr.Length + versionInfoLen);
            hash.CopyTo(outBytes, headerIDStr.Length + versionInfoLen + encryptedBytes.Length);

            Console.WriteLine("Exporting with version: " + versionMajor + "." + versionMinor);
            Console.WriteLine();
        }
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
