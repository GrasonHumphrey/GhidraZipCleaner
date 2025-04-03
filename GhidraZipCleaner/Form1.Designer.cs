namespace GhidraZipCleaner
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_cleanGZF = new System.Windows.Forms.Button();
            this.openFileDialogCleanGZF = new System.Windows.Forms.OpenFileDialog();
            this.openFileDialogGZF = new System.Windows.Forms.OpenFileDialog();
            this.btn_GZF = new System.Windows.Forms.Button();
            this.gzfLabel = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageImport = new System.Windows.Forms.TabPage();
            this.btn_Import = new System.Windows.Forms.Button();
            this.cgzfLabel = new System.Windows.Forms.Label();
            this.tabPageExport = new System.Windows.Forms.TabPage();
            this.btn_Export = new System.Windows.Forms.Button();
            this.btn_ROM = new System.Windows.Forms.Button();
            this.romLabel = new System.Windows.Forms.Label();
            this.openFileDialogROM = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialogGZF = new System.Windows.Forms.SaveFileDialog();
            this.saveFileDialogCGZF = new System.Windows.Forms.SaveFileDialog();
            this.tabControl1.SuspendLayout();
            this.tabPageImport.SuspendLayout();
            this.tabPageExport.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_cleanGZF
            // 
            this.btn_cleanGZF.Location = new System.Drawing.Point(47, 150);
            this.btn_cleanGZF.Name = "btn_cleanGZF";
            this.btn_cleanGZF.Size = new System.Drawing.Size(279, 58);
            this.btn_cleanGZF.TabIndex = 0;
            this.btn_cleanGZF.Text = "Select clean GZF";
            this.btn_cleanGZF.UseVisualStyleBackColor = true;
            this.btn_cleanGZF.Click += new System.EventHandler(this.btn_CleanGZF_Click);
            // 
            // openFileDialogCleanGZF
            // 
            this.openFileDialogCleanGZF.FileName = "openFileDialogCleanGZF";
            this.openFileDialogCleanGZF.Filter = "Clean Ghidra Zip Files (*.cgzf)|*.cgzf";
            // 
            // openFileDialogGZF
            // 
            this.openFileDialogGZF.FileName = "openFileDialogGZF";
            this.openFileDialogGZF.Filter = "Ghidra Zip Files (*.gzf)|*.gzf";
            // 
            // btn_GZF
            // 
            this.btn_GZF.Location = new System.Drawing.Point(79, 38);
            this.btn_GZF.Name = "btn_GZF";
            this.btn_GZF.Size = new System.Drawing.Size(212, 61);
            this.btn_GZF.TabIndex = 1;
            this.btn_GZF.Text = "Select GZF";
            this.btn_GZF.UseVisualStyleBackColor = true;
            this.btn_GZF.Click += new System.EventHandler(this.btn_GZF_Click);
            // 
            // gzfLabel
            // 
            this.gzfLabel.AutoSize = true;
            this.gzfLabel.Location = new System.Drawing.Point(50, 186);
            this.gzfLabel.Name = "gzfLabel";
            this.gzfLabel.Size = new System.Drawing.Size(114, 32);
            this.gzfLabel.TabIndex = 2;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageImport);
            this.tabControl1.Controls.Add(this.tabPageExport);
            this.tabControl1.Location = new System.Drawing.Point(2, -1);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(786, 321);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPageImport
            // 
            this.tabPageImport.Controls.Add(this.btn_Import);
            this.tabPageImport.Controls.Add(this.cgzfLabel);
            this.tabPageImport.Controls.Add(this.btn_cleanGZF);
            this.tabPageImport.Location = new System.Drawing.Point(10, 48);
            this.tabPageImport.Name = "tabPageImport";
            this.tabPageImport.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageImport.Size = new System.Drawing.Size(766, 263);
            this.tabPageImport.TabIndex = 0;
            this.tabPageImport.Text = "Import";
            this.tabPageImport.UseVisualStyleBackColor = true;
            // 
            // btn_Import
            // 
            this.btn_Import.Location = new System.Drawing.Point(500, 148);
            this.btn_Import.Name = "btn_Import";
            this.btn_Import.Size = new System.Drawing.Size(145, 60);
            this.btn_Import.TabIndex = 6;
            this.btn_Import.Text = "Import!";
            this.btn_Import.UseVisualStyleBackColor = true;
            this.btn_Import.Click += new System.EventHandler(this.btn_Import_Click);
            // 
            // cgzfLabel
            // 
            this.cgzfLabel.AutoSize = true;
            this.cgzfLabel.Location = new System.Drawing.Point(50, 60);
            this.cgzfLabel.Name = "cgzfLabel";
            this.cgzfLabel.Size = new System.Drawing.Size(0, 32);
            this.cgzfLabel.TabIndex = 1;
            // 
            // tabPageExport
            // 
            this.tabPageExport.Controls.Add(this.btn_Export);
            this.tabPageExport.Controls.Add(this.gzfLabel);
            this.tabPageExport.Controls.Add(this.btn_GZF);
            this.tabPageExport.Location = new System.Drawing.Point(10, 48);
            this.tabPageExport.Name = "tabPageExport";
            this.tabPageExport.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageExport.Size = new System.Drawing.Size(766, 263);
            this.tabPageExport.TabIndex = 1;
            this.tabPageExport.Text = "Export";
            this.tabPageExport.UseVisualStyleBackColor = true;
            // 
            // btn_Export
            // 
            this.btn_Export.Location = new System.Drawing.Point(500, 35);
            this.btn_Export.Name = "btn_Export";
            this.btn_Export.Size = new System.Drawing.Size(145, 60);
            this.btn_Export.TabIndex = 2;
            this.btn_Export.Text = "Export!";
            this.btn_Export.UseVisualStyleBackColor = true;
            this.btn_Export.Click += new System.EventHandler(this.btn_Export_Click);
            // 
            // btn_ROM
            // 
            this.btn_ROM.Location = new System.Drawing.Point(96, 379);
            this.btn_ROM.Name = "btn_ROM";
            this.btn_ROM.Size = new System.Drawing.Size(202, 59);
            this.btn_ROM.TabIndex = 4;
            this.btn_ROM.Text = "Select ROM";
            this.btn_ROM.UseVisualStyleBackColor = true;
            this.btn_ROM.Click += new System.EventHandler(this.btn_ROM_Click);
            // 
            // romLabel
            // 
            this.romLabel.AutoSize = true;
            this.romLabel.Location = new System.Drawing.Point(90, 331);
            this.romLabel.Name = "romLabel";
            this.romLabel.Size = new System.Drawing.Size(0, 32);
            this.romLabel.TabIndex = 5;
            // 
            // openFileDialogROM
            // 
            this.openFileDialogROM.FileName = "openFileDialogROM";
            this.openFileDialogROM.Filter = "ROM file (*.*)|*.*";
            // 
            // saveFileDialogGZF
            // 
            this.saveFileDialogGZF.DefaultExt = "gzf";
            this.saveFileDialogGZF.Filter = "Ghidra Zip File|*.gzf";
            // 
            // saveFileDialogCGZF
            // 
            this.saveFileDialogCGZF.DefaultExt = "cgzf";
            this.saveFileDialogCGZF.Filter = "Clean Ghidra Zip File|*.cgzf";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.romLabel);
            this.Controls.Add(this.btn_ROM);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Clean GZF Import/Export";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPageImport.ResumeLayout(false);
            this.tabPageImport.PerformLayout();
            this.tabPageExport.ResumeLayout(false);
            this.tabPageExport.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_cleanGZF;
        private System.Windows.Forms.OpenFileDialog openFileDialogCleanGZF;
        private System.Windows.Forms.OpenFileDialog openFileDialogGZF;
        private System.Windows.Forms.Button btn_GZF;
        private System.Windows.Forms.Label gzfLabel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageImport;
        private System.Windows.Forms.TabPage tabPageExport;
        private System.Windows.Forms.Label cgzfLabel;
        private System.Windows.Forms.Button btn_ROM;
        private System.Windows.Forms.Label romLabel;
        private System.Windows.Forms.Button btn_Import;
        private System.Windows.Forms.Button btn_Export;
        private System.Windows.Forms.OpenFileDialog openFileDialogROM;
        private System.Windows.Forms.SaveFileDialog saveFileDialogGZF;
        private System.Windows.Forms.SaveFileDialog saveFileDialogCGZF;
    }
}

