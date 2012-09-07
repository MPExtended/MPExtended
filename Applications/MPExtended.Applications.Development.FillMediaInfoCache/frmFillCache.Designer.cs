namespace MPExtended.Applications.Development.FillMediaInfoCache
{
    partial class frmFillCache
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
            this.btnStart = new System.Windows.Forms.Button();
            this.lblMovies = new System.Windows.Forms.Label();
            this.lblEpisodes = new System.Windows.Forms.Label();
            this.prgMovies = new System.Windows.Forms.ProgressBar();
            this.prgEpisodes = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(16, 13);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(256, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblMovies
            // 
            this.lblMovies.AutoSize = true;
            this.lblMovies.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMovies.Location = new System.Drawing.Point(13, 55);
            this.lblMovies.Name = "lblMovies";
            this.lblMovies.Size = new System.Drawing.Size(90, 15);
            this.lblMovies.TabIndex = 1;
            this.lblMovies.Text = "Movies (8 / 12):";
            // 
            // lblEpisodes
            // 
            this.lblEpisodes.AutoSize = true;
            this.lblEpisodes.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEpisodes.Location = new System.Drawing.Point(13, 124);
            this.lblEpisodes.Name = "lblEpisodes";
            this.lblEpisodes.Size = new System.Drawing.Size(102, 15);
            this.lblEpisodes.TabIndex = 2;
            this.lblEpisodes.Text = "Episodes (8 / 12):";
            // 
            // prgMovies
            // 
            this.prgMovies.Location = new System.Drawing.Point(16, 82);
            this.prgMovies.Name = "prgMovies";
            this.prgMovies.Size = new System.Drawing.Size(256, 23);
            this.prgMovies.TabIndex = 3;
            // 
            // prgEpisodes
            // 
            this.prgEpisodes.Location = new System.Drawing.Point(16, 153);
            this.prgEpisodes.Name = "prgEpisodes";
            this.prgEpisodes.Size = new System.Drawing.Size(256, 23);
            this.prgEpisodes.TabIndex = 4;
            // 
            // frmFillCache
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 189);
            this.Controls.Add(this.prgEpisodes);
            this.Controls.Add(this.prgMovies);
            this.Controls.Add(this.lblEpisodes);
            this.Controls.Add(this.lblMovies);
            this.Controls.Add(this.btnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "frmFillCache";
            this.Text = "Fill MediaInfo cache";
            this.Load += new System.EventHandler(this.frmFillCache_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblMovies;
        private System.Windows.Forms.Label lblEpisodes;
        private System.Windows.Forms.ProgressBar prgMovies;
        private System.Windows.Forms.ProgressBar prgEpisodes;

    }
}

