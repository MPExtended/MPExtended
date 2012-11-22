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
            this.cmdRefresh = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cmdAddDownload = new System.Windows.Forms.Button();
            this.olvScraperItems = new BrightIdeasSoftware.ObjectListView();
            this.chItemTitle = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.barRenderer1 = new BrightIdeasSoftware.BarRenderer();
            this.chItemState = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.cmsItemActions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.txtItemId = new System.Windows.Forms.TextBox();
            this.cbItemType = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chItemProgress = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.progressRenderer1 = new MPExtended.Scrapers.ScraperManager.ProgressRenderer();
            this.txtItemName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvScraperItems)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbAvailableScrapers
            // 
            this.cbAvailableScrapers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAvailableScrapers.FormattingEnabled = true;
            this.cbAvailableScrapers.Location = new System.Drawing.Point(12, 12);
            this.cbAvailableScrapers.Name = "cbAvailableScrapers";
            this.cbAvailableScrapers.Size = new System.Drawing.Size(269, 21);
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
            this.cbInputRequests.Location = new System.Drawing.Point(13, 28);
            this.cbInputRequests.Name = "cbInputRequests";
            this.cbInputRequests.Size = new System.Drawing.Size(203, 21);
            this.cbInputRequests.TabIndex = 3;
            // 
            // cmdMatchItems
            // 
            this.cmdMatchItems.Location = new System.Drawing.Point(223, 26);
            this.cmdMatchItems.Name = "cmdMatchItems";
            this.cmdMatchItems.Size = new System.Drawing.Size(99, 23);
            this.cmdMatchItems.TabIndex = 4;
            this.cmdMatchItems.Text = "Select Match";
            this.cmdMatchItems.UseVisualStyleBackColor = true;
            this.cmdMatchItems.Click += new System.EventHandler(this.cmdMatchItems_Click);
            // 
            // cmdStart
            // 
            this.cmdStart.Location = new System.Drawing.Point(369, 10);
            this.cmdStart.Name = "cmdStart";
            this.cmdStart.Size = new System.Drawing.Size(75, 23);
            this.cmdStart.TabIndex = 5;
            this.cmdStart.Text = "Start";
            this.cmdStart.UseVisualStyleBackColor = true;
            this.cmdStart.Click += new System.EventHandler(this.cmdStart_Click);
            // 
            // cmdPauseResume
            // 
            this.cmdPauseResume.Location = new System.Drawing.Point(450, 10);
            this.cmdPauseResume.Name = "cmdPauseResume";
            this.cmdPauseResume.Size = new System.Drawing.Size(107, 23);
            this.cmdPauseResume.TabIndex = 5;
            this.cmdPauseResume.Text = "Pause";
            this.cmdPauseResume.UseVisualStyleBackColor = true;
            this.cmdPauseResume.Click += new System.EventHandler(this.cmdPauseResume_Click);
            // 
            // cmdStop
            // 
            this.cmdStop.Location = new System.Drawing.Point(563, 10);
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
            this.statusStrip1.Location = new System.Drawing.Point(0, 399);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(724, 22);
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
            this.lblScraperState.Location = new System.Drawing.Point(287, 15);
            this.lblScraperState.Name = "lblScraperState";
            this.lblScraperState.Size = new System.Drawing.Size(47, 13);
            this.lblScraperState.TabIndex = 7;
            this.lblScraperState.Text = "Stopped";
            // 
            // cmdRefresh
            // 
            this.cmdRefresh.Location = new System.Drawing.Point(644, 10);
            this.cmdRefresh.Name = "cmdRefresh";
            this.cmdRefresh.Size = new System.Drawing.Size(62, 23);
            this.cmdRefresh.TabIndex = 8;
            this.cmdRefresh.Text = "Update";
            this.cmdRefresh.UseVisualStyleBackColor = true;
            this.cmdRefresh.Click += new System.EventHandler(this.cmdRefresh_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmdMatchItems);
            this.groupBox1.Controls.Add(this.cbInputRequests);
            this.groupBox1.Location = new System.Drawing.Point(12, 298);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(338, 78);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Pending Input Requests";
            // 
            // cmdAddDownload
            // 
            this.cmdAddDownload.Location = new System.Drawing.Point(258, 49);
            this.cmdAddDownload.Name = "cmdAddDownload";
            this.cmdAddDownload.Size = new System.Drawing.Size(85, 23);
            this.cmdAddDownload.TabIndex = 10;
            this.cmdAddDownload.Text = "Add Download";
            this.cmdAddDownload.UseVisualStyleBackColor = true;
            this.cmdAddDownload.Click += new System.EventHandler(this.cmdAddDownload_Click);
            // 
            // olvScraperItems
            // 
            this.olvScraperItems.AllColumns.Add(this.chItemTitle);
            this.olvScraperItems.AllColumns.Add(this.chItemState);
            this.olvScraperItems.AllColumns.Add(this.chItemProgress);
            this.olvScraperItems.AlternateRowBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.olvScraperItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chItemTitle,
            this.chItemState,
            this.chItemProgress});
            this.olvScraperItems.ContextMenuStrip = this.cmsItemActions;
            this.olvScraperItems.FullRowSelect = true;
            this.olvScraperItems.Location = new System.Drawing.Point(12, 55);
            this.olvScraperItems.Name = "olvScraperItems";
            this.olvScraperItems.OwnerDraw = true;
            this.olvScraperItems.ShowGroups = false;
            this.olvScraperItems.Size = new System.Drawing.Size(694, 237);
            this.olvScraperItems.TabIndex = 10;
            this.olvScraperItems.UseCompatibleStateImageBehavior = false;
            this.olvScraperItems.View = System.Windows.Forms.View.Details;
            // 
            // chItemTitle
            // 
            this.chItemTitle.CellPadding = null;
            this.chItemTitle.Text = "Name";
            this.chItemTitle.Width = 428;
            // 
            // chItemState
            // 
            this.chItemState.CellPadding = null;
            this.chItemState.Text = "State";
            this.chItemState.Width = 105;
            // 
            // cmsItemActions
            // 
            this.cmsItemActions.Name = "cmsItemActions";
            this.cmsItemActions.Size = new System.Drawing.Size(61, 4);
            this.cmsItemActions.Opening += new System.ComponentModel.CancelEventHandler(this.cmsItemActions_Opening);
            this.cmsItemActions.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.cmsItemActions_ItemClicked);
            // 
            // txtItemId
            // 
            this.txtItemId.Location = new System.Drawing.Point(135, 51);
            this.txtItemId.Name = "txtItemId";
            this.txtItemId.Size = new System.Drawing.Size(117, 20);
            this.txtItemId.TabIndex = 12;
            this.txtItemId.Text = "72173_1x20";
            // 
            // cbItemType
            // 
            this.cbItemType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbItemType.FormattingEnabled = true;
            this.cbItemType.Items.AddRange(new object[] {
            "TV Episode",
            "Movie"});
            this.cbItemType.Location = new System.Drawing.Point(6, 51);
            this.cbItemType.Name = "cbItemType";
            this.cbItemType.Size = new System.Drawing.Size(121, 21);
            this.cbItemType.TabIndex = 13;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtItemName);
            this.groupBox2.Controls.Add(this.cbItemType);
            this.groupBox2.Controls.Add(this.txtItemId);
            this.groupBox2.Controls.Add(this.cmdAddDownload);
            this.groupBox2.Location = new System.Drawing.Point(357, 298);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(349, 78);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Add Item";
            // 
            // chItemProgress
            // 
            this.chItemProgress.CellPadding = null;
            this.chItemProgress.IsEditable = false;
            this.chItemProgress.Renderer = this.progressRenderer1;
            this.chItemProgress.Text = "Progress";
            this.chItemProgress.Width = 86;
            // 
            // txtItemName
            // 
            this.txtItemName.Location = new System.Drawing.Point(42, 25);
            this.txtItemName.Name = "txtItemName";
            this.txtItemName.Size = new System.Drawing.Size(301, 20);
            this.txtItemName.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Title";
            // 
            // ScraperManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(724, 421);
            this.Controls.Add(this.olvScraperItems);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cmdRefresh);
            this.Controls.Add(this.lblScraperState);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.cmdStop);
            this.Controls.Add(this.cmdPauseResume);
            this.Controls.Add(this.cmdStart);
            this.Controls.Add(this.cbAvailableScrapers);
            this.Name = "ScraperManager";
            this.Text = "Scraper Manager";
            this.Load += new System.EventHandler(this.ScraperManager_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvScraperItems)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
        private System.Windows.Forms.Button cmdRefresh;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button cmdAddDownload;
        private BrightIdeasSoftware.ObjectListView olvScraperItems;
        private BrightIdeasSoftware.OLVColumn chItemTitle;
        private BrightIdeasSoftware.OLVColumn chItemProgress;
        private ProgressRenderer progressRenderer;
        private BrightIdeasSoftware.BarRenderer barRenderer1;
        private BrightIdeasSoftware.OLVColumn chItemState;
        private ProgressRenderer progressRenderer1;
        private System.Windows.Forms.ContextMenuStrip cmsItemActions;
        private System.Windows.Forms.TextBox txtItemId;
        private System.Windows.Forms.ComboBox cbItemType;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtItemName;
    }
}

