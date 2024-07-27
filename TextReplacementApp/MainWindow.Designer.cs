namespace TextReplacementApp
{
    partial class MainWindow
    {
        /// <summary>Required designer variable.</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>Clean up any resources being used.</summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.labelDirectoryPath = new System.Windows.Forms.Label();
            this.txtDirectoryPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.labelOriginal = new System.Windows.Forms.Label();
            this.txtSearchText = new System.Windows.Forms.TextBox();
            this.labelReplacement = new System.Windows.Forms.Label();
            this.txtReplaceText = new System.Windows.Forms.TextBox();
            this.btnDoIt = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelDirectoryPath
            // 
            this.labelDirectoryPath.AutoSize = true;
            this.labelDirectoryPath.Location = new System.Drawing.Point(35, 23);
            this.labelDirectoryPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDirectoryPath.Name = "labelDirectoryPath";
            this.labelDirectoryPath.Size = new System.Drawing.Size(87, 15);
            this.labelDirectoryPath.TabIndex = 0;
            this.labelDirectoryPath.Text = "&Starting Folder:";
            // 
            // txtDirectoryPath
            // 
            this.txtDirectoryPath.Location = new System.Drawing.Point(140, 20);
            this.txtDirectoryPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtDirectoryPath.Name = "txtDirectoryPath";
            this.txtDirectoryPath.Size = new System.Drawing.Size(381, 23);
            this.txtDirectoryPath.TabIndex = 1;
            this.txtDirectoryPath.TextChanged += new System.EventHandler(this.OnTextChangedDirectoryPath);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(528, 17);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(88, 27);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "&Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.OnClickBrowseButton);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Select the folder containing the files to be processed";
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // labelOriginal
            // 
            this.labelOriginal.AutoSize = true;
            this.labelOriginal.Location = new System.Drawing.Point(58, 61);
            this.labelOriginal.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelOriginal.Name = "labelOriginal";
            this.labelOriginal.Size = new System.Drawing.Size(64, 15);
            this.labelOriginal.TabIndex = 0;
            this.labelOriginal.Text = "Find &What:";
            // 
            // txtSearchText
            // 
            this.txtSearchText.Location = new System.Drawing.Point(141, 53);
            this.txtSearchText.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtSearchText.Name = "txtSearchText";
            this.txtSearchText.Size = new System.Drawing.Size(380, 23);
            this.txtSearchText.TabIndex = 3;
            this.txtSearchText.TextChanged += new System.EventHandler(this.OnTextChangedSearchText);
            // 
            // labelReplacement
            // 
            this.labelReplacement.AutoSize = true;
            this.labelReplacement.Location = new System.Drawing.Point(43, 95);
            this.labelReplacement.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelReplacement.Name = "labelReplacement";
            this.labelReplacement.Size = new System.Drawing.Size(79, 15);
            this.labelReplacement.TabIndex = 0;
            this.labelReplacement.Text = "&Replace With:";
            // 
            // txtReplaceText
            // 
            this.txtReplaceText.Location = new System.Drawing.Point(141, 92);
            this.txtReplaceText.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtReplaceText.Name = "txtReplaceText";
            this.txtReplaceText.Size = new System.Drawing.Size(380, 23);
            this.txtReplaceText.TabIndex = 4;
            this.txtReplaceText.TextChanged += new System.EventHandler(this.OnTextChangedReplaceText);
            // 
            // btnDoIt
            // 
            this.btnDoIt.Location = new System.Drawing.Point(141, 133);
            this.btnDoIt.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnDoIt.Name = "btnDoIt";
            this.btnDoIt.Size = new System.Drawing.Size(88, 27);
            this.btnDoIt.TabIndex = 5;
            this.btnDoIt.Text = "&Do It!";
            this.btnDoIt.UseVisualStyleBackColor = true;
            this.btnDoIt.Click += new System.EventHandler(this.OnClickDoItButton);
            // 
            // MainWindow
            // 
            this.AcceptButton = this.btnDoIt;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 180);
            this.Controls.Add(this.labelDirectoryPath);
            this.Controls.Add(this.txtDirectoryPath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.labelOriginal);
            this.Controls.Add(this.txtSearchText);
            this.Controls.Add(this.labelReplacement);
            this.Controls.Add(this.txtReplaceText);
            this.Controls.Add(this.btnDoIt);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "xyLOGIX Fast Text Replacer Tool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDirectoryPath;
        private System.Windows.Forms.TextBox txtDirectoryPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label labelOriginal;
        private System.Windows.Forms.TextBox txtSearchText;
        private System.Windows.Forms.Label labelReplacement;
        private System.Windows.Forms.TextBox txtReplaceText;
        private System.Windows.Forms.Button btnDoIt;
    }
}
