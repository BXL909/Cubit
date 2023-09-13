namespace Cubit
{
    partial class About
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            btnExit = new CustomControls.RJControls.RJButton();
            pictureBox1 = new PictureBox();
            pictureBox2 = new PictureBox();
            pictureBox3 = new PictureBox();
            linkLabel1 = new LinkLabel();
            linkLabel2 = new LinkLabel();
            linkLabel3 = new LinkLabel();
            linkLabel4 = new LinkLabel();
            lblCurrentVersion = new Label();
            lblLatestVersion = new Label();
            linkLabelDownloadUpdate = new LinkLabel();
            lblErrorMessage = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            SuspendLayout();
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.FromArgb(255, 192, 128);
            btnExit.BackgroundColor = Color.FromArgb(255, 192, 128);
            btnExit.BorderColor = Color.PaleVioletRed;
            btnExit.BorderRadius = 14;
            btnExit.BorderSize = 0;
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.Font = new Font("Century Gothic", 9F, FontStyle.Regular, GraphicsUnit.Point);
            btnExit.ForeColor = Color.White;
            btnExit.Location = new Point(399, 5);
            btnExit.Name = "btnExit";
            btnExit.Padding = new Padding(3, 1, 0, 0);
            btnExit.Size = new Size(24, 24);
            btnExit.TabIndex = 2;
            btnExit.Text = "✖️";
            btnExit.TextColor = Color.White;
            btnExit.UseVisualStyleBackColor = false;
            btnExit.Click += BtnExit_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.PnVlfRPD_400x400;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(200, 200);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            pictureBox1.Click += PictureBox1_Click;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = Properties.Resources.logo;
            pictureBox2.Location = new Point(209, 39);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(70, 70);
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.TabIndex = 4;
            pictureBox2.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.Image = Properties.Resources.text;
            pictureBox3.Location = new Point(286, 39);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(132, 50);
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox3.TabIndex = 5;
            pictureBox3.TabStop = false;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.LinkColor = Color.FromArgb(255, 128, 0);
            linkLabel1.Location = new Point(378, 176);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(45, 15);
            linkLabel1.TabIndex = 6;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "BXL909";
            linkLabel1.LinkClicked += LinkLabel1_LinkClicked;
            // 
            // linkLabel2
            // 
            linkLabel2.AutoSize = true;
            linkLabel2.LinkColor = Color.FromArgb(255, 128, 0);
            linkLabel2.Location = new Point(314, 131);
            linkLabel2.Name = "linkLabel2";
            linkLabel2.Size = new Size(109, 15);
            linkLabel2.TabIndex = 7;
            linkLabel2.TabStop = true;
            linkLabel2.Text = "Support the project";
            linkLabel2.LinkClicked += LinkLabel2_LinkClicked;
            // 
            // linkLabel3
            // 
            linkLabel3.AutoSize = true;
            linkLabel3.LinkColor = Color.FromArgb(255, 128, 0);
            linkLabel3.Location = new Point(342, 146);
            linkLabel3.Name = "linkLabel3";
            linkLabel3.Size = new Size(79, 15);
            linkLabel3.TabIndex = 8;
            linkLabel3.TabStop = true;
            linkLabel3.Text = "Cubit website";
            linkLabel3.LinkClicked += LinkLabel3_LinkClicked;
            // 
            // linkLabel4
            // 
            linkLabel4.AutoSize = true;
            linkLabel4.LinkColor = Color.FromArgb(255, 128, 0);
            linkLabel4.Location = new Point(349, 161);
            linkLabel4.Name = "linkLabel4";
            linkLabel4.Size = new Size(74, 15);
            linkLabel4.TabIndex = 9;
            linkLabel4.TabStop = true;
            linkLabel4.Text = "Open source";
            linkLabel4.LinkClicked += LinkLabel4_LinkClicked;
            // 
            // lblCurrentVersion
            // 
            lblCurrentVersion.AutoSize = true;
            lblCurrentVersion.ForeColor = Color.Gray;
            lblCurrentVersion.Location = new Point(209, 131);
            lblCurrentVersion.Name = "lblCurrentVersion";
            lblCurrentVersion.Size = new Size(45, 15);
            lblCurrentVersion.TabIndex = 11;
            lblCurrentVersion.Text = "Cubit v";
            // 
            // lblLatestVersion
            // 
            lblLatestVersion.AutoSize = true;
            lblLatestVersion.ForeColor = Color.Gray;
            lblLatestVersion.Location = new Point(209, 146);
            lblLatestVersion.Name = "lblLatestVersion";
            lblLatestVersion.Size = new Size(69, 15);
            lblLatestVersion.TabIndex = 12;
            lblLatestVersion.Text = "(up to date)";
            // 
            // linkLabelDownloadUpdate
            // 
            linkLabelDownloadUpdate.AutoSize = true;
            linkLabelDownloadUpdate.LinkColor = Color.FromArgb(255, 128, 0);
            linkLabelDownloadUpdate.Location = new Point(209, 161);
            linkLabelDownloadUpdate.Name = "linkLabelDownloadUpdate";
            linkLabelDownloadUpdate.Size = new Size(101, 15);
            linkLabelDownloadUpdate.TabIndex = 13;
            linkLabelDownloadUpdate.TabStop = true;
            linkLabelDownloadUpdate.Text = "Download update";
            linkLabelDownloadUpdate.Visible = false;
            linkLabelDownloadUpdate.LinkClicked += LinkLabelDownloadUpdate_LinkClicked;
            // 
            // lblErrorMessage
            // 
            lblErrorMessage.ForeColor = Color.Gray;
            lblErrorMessage.Location = new Point(209, 5);
            lblErrorMessage.Name = "lblErrorMessage";
            lblErrorMessage.Size = new Size(184, 30);
            lblErrorMessage.TabIndex = 14;
            lblErrorMessage.Text = "Cubit v";
            lblErrorMessage.Visible = false;
            // 
            // About
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = btnExit;
            ClientSize = new Size(428, 200);
            ControlBox = false;
            Controls.Add(lblErrorMessage);
            Controls.Add(linkLabelDownloadUpdate);
            Controls.Add(lblLatestVersion);
            Controls.Add(lblCurrentVersion);
            Controls.Add(linkLabel4);
            Controls.Add(linkLabel3);
            Controls.Add(linkLabel2);
            Controls.Add(linkLabel1);
            Controls.Add(pictureBox3);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox1);
            Controls.Add(btnExit);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "About";
            Text = "About";
            Load += About_Load;
            Paint += About_Paint;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CustomControls.RJControls.RJButton btnExit;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private LinkLabel linkLabel1;
        private LinkLabel linkLabel2;
        private LinkLabel linkLabel3;
        private LinkLabel linkLabel4;
        private Label lblCurrentVersion;
        private Label lblLatestVersion;
        private LinkLabel linkLabelDownloadUpdate;
        private Label lblErrorMessage;
    }
}