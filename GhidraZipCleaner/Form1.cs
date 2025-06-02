using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GhidraZipCleaner
{
    public partial class Form1 : Form
    {

        StreamReader srGZF = null;
        StreamReader srCGZF = null;
        StreamReader srROM = null;

        string gzfFilename = null;
        string cgzfFilename = null;

        byte[] romBytes = null;

        int versionMajor = 2;
        int versionMinor = 0;
        int versionInfoLen = 2;

        int saltLen = 16;

        bool forceVersion = false;
        byte[] headerIDStr = { (byte)'C', (byte)'G', (byte)'Z', (byte)'F' };

        public Form1()
        {
            InitializeComponent();
        }

        private void btn_CleanGZF_Click(object sender, EventArgs e)
        {
            if (openFileDialogCleanGZF.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    srCGZF = new StreamReader(openFileDialogCleanGZF.FileName);
                    //SetText(sr.ReadToEnd().Length.ToString());

                    int index = openFileDialogCleanGZF.SafeFileName.LastIndexOf(".");
                    if (index >= 0)
                        cgzfFilename = openFileDialogCleanGZF.SafeFileName.Substring(0, index);
                    cgzfLabel.Text = openFileDialogCleanGZF.SafeFileName;

                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        private void btn_GZF_Click(object sender, EventArgs e)
        {
            if (openFileDialogGZF.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    srGZF = new StreamReader(openFileDialogGZF.FileName);
                    //gzfLabel.Text += sr.ReadToEnd().Length.ToString();

                    int index = openFileDialogGZF.SafeFileName.LastIndexOf(".");
                    if (index >= 0)
                        gzfFilename = openFileDialogGZF.SafeFileName.Substring(0, index); 
                    gzfLabel.Text += openFileDialogGZF.SafeFileName;

                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        private void btn_ROM_Click(object sender, EventArgs e)
        {
            if (openFileDialogROM.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    srROM = new StreamReader(openFileDialogROM.FileName);
                    //gzfLabel.Text += sr.ReadToEnd().Length.ToString();
                    romLabel.Text += openFileDialogROM.SafeFileName;

                    using (var memstream = new MemoryStream())
                    {
                        srROM.BaseStream.CopyTo(memstream);
                        romBytes = memstream.ToArray();
                    }

                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        private byte[] XOR_All_Bytes(byte[] romBytes, byte[] ghidraBytes)
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

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(ghidraBytes, 0, ghidraBytes.Length);
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
            byte[] decrypted = new byte[encryptedBytes.Length];

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.IV = keyInfo.Take(16).ToArray();
                aesAlg.Key = keyInfo.Skip(32).ToArray();

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        for (int i = 0; i < encryptedBytes.Length; i++)
                        {
                            decrypted[i] = (byte)csDecrypt.ReadByte();
                        }
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

        private void btn_Import_Click(object sender, EventArgs e)
        {        

            if (srCGZF == null || srCGZF.BaseStream.Length == 0)
            {
                MessageBox.Show("No .cgzf file selected!");
                return;
            }
            if (srROM == null || srROM.BaseStream.Length == 0)
            {
                MessageBox.Show("No ROM file selected!");
                return;
            }

            byte[] cgzfBytes = null;
            using (var memstream = new MemoryStream())
            {
                srCGZF.BaseStream.CopyTo(memstream);
                cgzfBytes = memstream.ToArray();
            }

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
                versionMinor = cgzfBytes[headerIDStr.Length+1];
            }
            else
            {
                // No header ID found, meaning legacy edition
                versionMajor = 1;
                versionMinor = 0;
            }

            // Don't decode the last 16 bytes which make the checksum
            byte[] outBytes;
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
                    // Legacy import
                    outBytes = XOR_All_Bytes(romBytes, cgzfBytes.Take(cgzfBytes.Length - 16).ToArray());
                    calculatedHash = md5.ComputeHash(outBytes);
                }
                else
                {
                    calculatedHash = md5.ComputeHash(outBytes.Take(400).ToArray());
                }
                if (!Enumerable.SequenceEqual(calculatedHash, retrievedHash))
                {
                    MessageBox.Show("Error: File checksum does not match expected. \n\n" +
                        "This likely means the ROM used does not match the one used to generate the .cgzf");
                    return;
                }
            }

            saveFileDialogGZF.FileName = cgzfFilename + "_DECODE";

            if (saveFileDialogGZF.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = (FileStream)saveFileDialogGZF.OpenFile();
                    fs.Write(outBytes, 0, outBytes.Length);
                    fs.Close();
                    //srROM = new StreamReader(openFileDialogROM.FileName);
                    srCGZF = new StreamReader(openFileDialogCleanGZF.FileName);
                    MessageBox.Show("Import success!");
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        private void btn_Export_Click(object sender, EventArgs e)
        {
            // Generate random salt
            byte[] salt = new byte[saltLen];

            Random rand = new Random();
            rand.NextBytes(salt);

            if (srGZF == null || srGZF.BaseStream.Length == 0)
            {
                MessageBox.Show("No .gzf file selected!");
                return;
            }
            if (srROM == null || srROM.BaseStream.Length == 0)
            {
                MessageBox.Show("No ROM file selected!");
                return;
            }

            byte[] gzfBytes = null;
            using (var memstream = new MemoryStream())
            {
                srGZF.BaseStream.CopyTo(memstream);
                gzfBytes = memstream.ToArray();
            }

            byte[] outBytes = new byte[gzfBytes.Length + 16];

            using (var md5 = MD5.Create())
            {
                byte[] hash;

                if (versionMajor == 1)
                {
                    outBytes = new byte[gzfBytes.Length + 16];
                    hash = md5.ComputeHash(gzfBytes);
                    XOR_All_Bytes(romBytes, gzfBytes).CopyTo(outBytes, 0);
                    hash.CopyTo(outBytes, gzfBytes.Length);
                }
                else
                {
                    hash = md5.ComputeHash(gzfBytes.Take(400).ToArray());
                    byte[] encryptedBytes = Encrypt_With_ROM(romBytes, gzfBytes, salt);
                    // Outbytes needs to hold header ID, version number, encrypted bytes, and hash
                    outBytes = new byte[headerIDStr.Length + versionInfoLen + encryptedBytes.Length + hash.Length];
                    headerIDStr.CopyTo(outBytes, 0);
                    outBytes[headerIDStr.Length] = (byte)versionMajor;
                    outBytes[headerIDStr.Length+1] = (byte)versionMinor;
                    encryptedBytes.CopyTo(outBytes, headerIDStr.Length + versionInfoLen);
                    hash.CopyTo(outBytes, headerIDStr.Length + versionInfoLen + encryptedBytes.Length);
                }
            }

            saveFileDialogCGZF.FileName = gzfFilename + "_CLEAN";

            if (saveFileDialogCGZF.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FileStream fs = (FileStream)saveFileDialogCGZF.OpenFile();
                    fs.Write(outBytes, 0, outBytes.Length);
                    fs.Close();
                    srROM = new StreamReader(openFileDialogROM.FileName);
                    srGZF = new StreamReader(openFileDialogGZF.FileName);
                    MessageBox.Show("Export success!");
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
