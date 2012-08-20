namespace MPExtended.Applications.Development.USSTest
{
    partial class UserSessionTester
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
            this.cmdStartMp = new System.Windows.Forms.Button();
            this.cmdCloseMp = new System.Windows.Forms.Button();
            this.cbPowerModes = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdSetPowermode = new System.Windows.Forms.Button();
            this.cmdMpStatus = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdStartMp
            // 
            this.cmdStartMp.Location = new System.Drawing.Point(145, 61);
            this.cmdStartMp.Name = "cmdStartMp";
            this.cmdStartMp.Size = new System.Drawing.Size(64, 23);
            this.cmdStartMp.TabIndex = 1;
            this.cmdStartMp.Text = "Start MP";
            this.cmdStartMp.UseVisualStyleBackColor = true;
            this.cmdStartMp.Click += new System.EventHandler(this.cmdStartMp_Click);
            // 
            // cmdCloseMp
            // 
            this.cmdCloseMp.Location = new System.Drawing.Point(208, 61);
            this.cmdCloseMp.Name = "cmdCloseMp";
            this.cmdCloseMp.Size = new System.Drawing.Size(64, 23);
            this.cmdCloseMp.TabIndex = 1;
            this.cmdCloseMp.Text = "Close MP";
            this.cmdCloseMp.UseVisualStyleBackColor = true;
            this.cmdCloseMp.Click += new System.EventHandler(this.cmdCloseMp_Click);
            // 
            // cbPowerModes
            // 
            this.cbPowerModes.FormattingEnabled = true;
            this.cbPowerModes.Location = new System.Drawing.Point(90, 106);
            this.cbPowerModes.Name = "cbPowerModes";
            this.cbPowerModes.Size = new System.Drawing.Size(121, 21);
            this.cbPowerModes.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 109);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "PowerModes:";
            // 
            // cmdSetPowermode
            // 
            this.cmdSetPowermode.Location = new System.Drawing.Point(218, 106);
            this.cmdSetPowermode.Name = "cmdSetPowermode";
            this.cmdSetPowermode.Size = new System.Drawing.Size(54, 23);
            this.cmdSetPowermode.TabIndex = 4;
            this.cmdSetPowermode.Text = "Set";
            this.cmdSetPowermode.UseVisualStyleBackColor = true;
            this.cmdSetPowermode.Click += new System.EventHandler(this.cmdSetPowermode_Click);
            // 
            // cmdMpStatus
            // 
            this.cmdMpStatus.Location = new System.Drawing.Point(12, 61);
            this.cmdMpStatus.Name = "cmdMpStatus";
            this.cmdMpStatus.Size = new System.Drawing.Size(75, 23);
            this.cmdMpStatus.TabIndex = 5;
            this.cmdMpStatus.Text = "MP Status";
            this.cmdMpStatus.UseVisualStyleBackColor = true;
            this.cmdMpStatus.Click += new System.EventHandler(this.cmdMpStatus_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(34, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(217, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "MediaPortal UserSessionService";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 144);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmdMpStatus);
            this.Controls.Add(this.cmdSetPowermode);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbPowerModes);
            this.Controls.Add(this.cmdCloseMp);
            this.Controls.Add(this.cmdStartMp);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdStartMp;
        private System.Windows.Forms.Button cmdCloseMp;
        private System.Windows.Forms.ComboBox cbPowerModes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cmdSetPowermode;
        private System.Windows.Forms.Button cmdMpStatus;
        private System.Windows.Forms.Label label2;
    }
}

