namespace MPExtended.Scrapers.ScraperManager
{
    partial class ScraperManager
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
            this.components = new System.ComponentModel.Container();
            this.cbAvailableScrapers = new System.Windows.Forms.ComboBox();
            this.timerUpdateScraperState = new System.Windows.Forms.Timer(this.components);
            this.cbInputRequests = new System.Windows.Forms.ComboBox();
            this.cmdMatchItems = new System.Windows.Forms.Button();
            this.cmdStart = new System.Windows.Forms.Button();
            this.cmdPauseResume = new System.Windows.Forms.Button();
            this.cmdStop = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsCurrentStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblScraperState = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbAvailableScrapers
            // 
            this.cbAvailableScrapers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAvailableScrapers.FormattingEnabled = true;
            this.cbAvailableScrapers.Location = new System.Drawing.Point(12, 12);
            this.cbAvailableScrapers.Name = "cbAvailableScrapers";
            this.cbAvailableScrapers.Size = new System.Drawing.Size(332, 21);
            this.cbAvailableScrapers.TabIndex = 0;
            this.cbAvailableScrapers.SelectedIndexChanged += new System.EventHandler(this.cbAvailableScrapers_SelectedIndexChanged);
            // 
            // timerUpdateScraperState
            // 
            this.timerUpdateScraperState.Interval = 500;
            this.timerUpdateScraperState.Tick += new System.EventHandler(this.timerUpdateScraperState_Tick);
            // 
            // cbInputRequests
            // 
            this.cbInputRequests.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbInputRequests.FormattingEnabled = true;
            this.cbInputRequests.Location = new System.Drawing.Point(12, 95);
            this.cbInputRequests.Name = "cbInputRequests";
            this.cbInputRequests.Size = new System.Drawing.Size(251, 21);
            this.cbInputRequests.TabIndex = 3;
            // 
            // cmdMatchItems
            // 
            this.cmdMatchItems.Location = new System.Drawing.Point(269, 93);
            this.cmdMatchItems.Name = "cmdMatchItems";
            this.cmdMatchItems.Size = new System.Drawing.Size(75, 23);
            this.cmdMatchItems.TabIndex = 4;
            this.cmdMatchItems.Text = "Match";
            this.cmdMatchItems.UseVisualStyleBackColor = true;
            this.cmdMatchItems.Click += new System.EventHandler(this.cmdMatchItems_Click);
            // 
            // cmdStart
            // 
            this.cmdStart.Location = new System.Drawing.Point(12, 50);
            this.cmdStart.Name = "cmdStart";
            this.cmdStart.Size = new System.Drawing.Size(75, 23);
            this.cmdStart.TabIndex = 5;
            this.cmdStart.Text = "Start";
            this.cmdStart.UseVisualStyleBackColor = true;
            this.cmdStart.Click += new System.EventHandler(this.cmdStart_Click);
            // 
            // cmdPauseResume
            // 
            this.cmdPauseResume.Location = new System.Drawing.Point(93, 50);
            this.cmdPauseResume.Name = "cmdPauseResume";
            this.cmdPauseResume.Size = new System.Drawing.Size(89, 23);
            this.cmdPauseResume.TabIndex = 5;
            this.cmdPauseResume.Text = "Pause";
            this.cmdPauseResume.UseVisualStyleBackColor = true;
            this.cmdPauseResume.Click += new System.EventHandler(this.cmdPauseResume_Click);
            // 
            // cmdStop
            // 
            this.cmdStop.Location = new System.Drawing.Point(188, 50);
            this.cmdStop.Name = "cmdStop";
            this.cmdStop.Size = new System.Drawing.Size(75, 23);
            this.cmdStop.TabIndex = 5;
            this.cmdStop.Text = "Stop";
            this.cmdStop.UseVisualStyleBackColor = true;
            this.cmdStop.Click += new System.EventHandler(this.cmdStop_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsCurrentStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 155);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(359, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsCurrentStatus
            // 
            this.tsCurrentStatus.Name = "tsCurrentStatus";
            this.tsCurrentStatus.Size = new System.Drawing.Size(39, 17);
            this.tsCurrentStatus.Text = "Status";
            // 
            // lblScraperState
            // 
            this.lblScraperState.AutoSize = true;
            this.lblScraperState.Location = new System.Drawing.Point(269, 55);
            this.lblScraperState.Name = "lblScraperState";
            this.lblScraperState.Size = new System.Drawing.Size(35, 13);
            this.lblScraperState.TabIndex = 7;
            this.lblScraperState.Text = "label1";
            // 
            // ScraperManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 177);
            this.Controls.Add(this.lblScraperState);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.cmdStop);
            this.Controls.Add(this.cmdPauseResume);
            this.Controls.Add(this.cmdStart);
            this.Controls.Add(this.cmdMatchItems);
            this.Controls.Add(this.cbInputRequests);
            this.Controls.Add(this.cbAvailableScrapers);
            this.Name = "ScraperManager";
            this.Text = "Scraper Manager";
            this.Load += new System.EventHandler(this.ScraperManager_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbAvailableScrapers;
        private System.Windows.Forms.Timer timerUpdateScraperState;
        private System.Windows.Forms.ComboBox cbInputRequests;
        private System.Windows.Forms.Button cmdMatchItems;
        private System.Windows.Forms.Button cmdStart;
        private System.Windows.Forms.Button cmdPauseResume;
        private System.Windows.Forms.Button cmdStop;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsCurrentStatus;
        private System.Windows.Forms.Label lblScraperState;
    }
}

