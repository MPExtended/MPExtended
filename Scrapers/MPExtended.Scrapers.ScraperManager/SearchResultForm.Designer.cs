namespace MPExtended.Scrapers.ScraperManager
{
  partial class SearchResultForm
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
            this.lvSearchResult = new System.Windows.Forms.ListView();
            this.chId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.linkImdb = new System.Windows.Forms.LinkLabel();
            this.txtOverview = new System.Windows.Forms.RichTextBox();
            this.txtFirstAired = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmdChoose = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.cmdSearch = new System.Windows.Forms.Button();
            this.txtSeriesName = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvSearchResult
            // 
            this.lvSearchResult.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chId,
            this.chName});
            this.lvSearchResult.FullRowSelect = true;
            this.lvSearchResult.Location = new System.Drawing.Point(12, 34);
            this.lvSearchResult.MultiSelect = false;
            this.lvSearchResult.Name = "lvSearchResult";
            this.lvSearchResult.Size = new System.Drawing.Size(328, 135);
            this.lvSearchResult.TabIndex = 0;
            this.lvSearchResult.UseCompatibleStateImageBehavior = false;
            this.lvSearchResult.View = System.Windows.Forms.View.Details;
            this.lvSearchResult.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.lvSearchResult_AfterLabelEdit);
            this.lvSearchResult.SelectedIndexChanged += new System.EventHandler(this.lvSearchResult_SelectedIndexChanged);
            // 
            // chId
            // 
            this.chId.Text = "Id";
            this.chId.Width = 1;
            // 
            // chName
            // 
            this.chName.Text = "Name";
            this.chName.Width = 316;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.linkImdb);
            this.groupBox1.Controls.Add(this.txtOverview);
            this.groupBox1.Controls.Add(this.txtFirstAired);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(13, 175);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(328, 188);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Details";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // linkImdb
            // 
            this.linkImdb.AutoSize = true;
            this.linkImdb.Location = new System.Drawing.Point(64, 63);
            this.linkImdb.Name = "linkImdb";
            this.linkImdb.Size = new System.Drawing.Size(0, 13);
            this.linkImdb.TabIndex = 3;
            this.linkImdb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkImdb_LinkClicked);
            // 
            // txtOverview
            // 
            this.txtOverview.Location = new System.Drawing.Point(67, 83);
            this.txtOverview.Name = "txtOverview";
            this.txtOverview.ReadOnly = true;
            this.txtOverview.Size = new System.Drawing.Size(255, 99);
            this.txtOverview.TabIndex = 2;
            this.txtOverview.Text = "";
            // 
            // txtFirstAired
            // 
            this.txtFirstAired.Location = new System.Drawing.Point(67, 30);
            this.txtFirstAired.Name = "txtFirstAired";
            this.txtFirstAired.ReadOnly = true;
            this.txtFirstAired.Size = new System.Drawing.Size(255, 20);
            this.txtFirstAired.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 86);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Overview:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "IMDB Id";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "FirstAired";
            // 
            // cmdChoose
            // 
            this.cmdChoose.Location = new System.Drawing.Point(16, 369);
            this.cmdChoose.Name = "cmdChoose";
            this.cmdChoose.Size = new System.Drawing.Size(159, 23);
            this.cmdChoose.TabIndex = 3;
            this.cmdChoose.Text = "Choose";
            this.cmdChoose.UseVisualStyleBackColor = true;
            this.cmdChoose.Click += new System.EventHandler(this.cmdChoose_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Location = new System.Drawing.Point(181, 369);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(159, 23);
            this.cmdCancel.TabIndex = 3;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.statusStrip1.Location = new System.Drawing.Point(0, 404);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(353, 20);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(82, 15);
            this.lblStatus.Text = "Search Results";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cmdSearch
            // 
            this.cmdSearch.Location = new System.Drawing.Point(265, 5);
            this.cmdSearch.Name = "cmdSearch";
            this.cmdSearch.Size = new System.Drawing.Size(75, 23);
            this.cmdSearch.TabIndex = 5;
            this.cmdSearch.Text = "Search";
            this.cmdSearch.UseVisualStyleBackColor = true;
            this.cmdSearch.Click += new System.EventHandler(this.cmdSearch_Click);
            // 
            // txtSeriesName
            // 
            this.txtSeriesName.Location = new System.Drawing.Point(12, 7);
            this.txtSeriesName.Name = "txtSeriesName";
            this.txtSeriesName.Size = new System.Drawing.Size(247, 20);
            this.txtSeriesName.TabIndex = 6;
            // 
            // SearchResultForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(353, 424);
            this.Controls.Add(this.txtSeriesName);
            this.Controls.Add(this.cmdSearch);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdChoose);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.lvSearchResult);
            this.Name = "SearchResultForm";
            this.ShowInTaskbar = false;
            this.Text = "Search Results";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ListView lvSearchResult;
    private System.Windows.Forms.ColumnHeader chId;
    private System.Windows.Forms.ColumnHeader chName;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RichTextBox txtOverview;
    private System.Windows.Forms.TextBox txtFirstAired;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button cmdChoose;
    private System.Windows.Forms.Button cmdCancel;
    private System.Windows.Forms.StatusStrip statusStrip1;
    private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    private System.Windows.Forms.LinkLabel linkImdb;
    private System.Windows.Forms.Button cmdSearch;
    private System.Windows.Forms.TextBox txtSeriesName;
  }
}