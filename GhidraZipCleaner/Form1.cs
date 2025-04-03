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

            // Don't decode the last 16 bytes which make the checksum
            byte[] outBytes = XOR_All_Bytes(romBytes, cgzfBytes.Take(cgzfBytes.Length-16).ToArray());
            byte[] retrievedHash = cgzfBytes.Skip(cgzfBytes.Length - 16).ToArray();

            using (var md5 = MD5.Create())
            {
                byte[] calculatedHash = md5.ComputeHash(outBytes);
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
                byte[] hash = md5.ComputeHash(gzfBytes);
                XOR_All_Bytes(romBytes, gzfBytes).CopyTo(outBytes, 0);
                hash.CopyTo(outBytes, gzfBytes.Length);
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
