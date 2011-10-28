namespace MPExtended.Applications.Development.StreamingService
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
            System.Windows.Forms.Button cmdStartStream;
            this.cmdSeekToPos = new System.Windows.Forms.Button();
            this.txtStartPos = new System.Windows.Forms.TextBox();
            this.cmdInitMovie = new System.Windows.Forms.Button();
            this.cmdFinishStreaming = new System.Windows.Forms.Button();
            this.cmdConnect = new System.Windows.Forms.Button();
            this.cmdSeek0 = new System.Windows.Forms.Button();
            this.cmdSeek100 = new System.Windows.Forms.Button();
            this.cmdSeek200 = new System.Windows.Forms.Button();
            this.cmdSeek300 = new System.Windows.Forms.Button();
            this.cmdSeek400 = new System.Windows.Forms.Button();
            this.cmdSeek500 = new System.Windows.Forms.Button();
            this.cmdSeek1000 = new System.Windows.Forms.Button();
            this.cmdSeek1500 = new System.Windows.Forms.Button();
            this.cmdSeek2000 = new System.Windows.Forms.Button();
            this.cmdSeek2500 = new System.Windows.Forms.Button();
            this.cmdSeek3000 = new System.Windows.Forms.Button();
            this.cmdSeek3600 = new System.Windows.Forms.Button();
            this.lbLog = new System.Windows.Forms.ListBox();
            this.txtFileSize = new System.Windows.Forms.TextBox();
            this.cbMovies = new System.Windows.Forms.ComboBox();
            this.cbChannels = new System.Windows.Forms.ComboBox();
            this.cmdInitChannel = new System.Windows.Forms.Button();
            this.cmdPlayInVlc = new System.Windows.Forms.Button();
            this.cbStartAutoPlayback = new System.Windows.Forms.CheckBox();
            this.cbLanguage = new System.Windows.Forms.ComboBox();
            this.cbProfiles = new System.Windows.Forms.ComboBox();
            this.cbSubtitle = new System.Windows.Forms.ComboBox();
            this.cmdInitIdTypeStreaming = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtItemId = new System.Windows.Forms.TextBox();
            this.cbItemType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtProvider = new System.Windows.Forms.TextBox();
            cmdStartStream = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmdStartStream
            // 
            cmdStartStream.Location = new System.Drawing.Point(11, 207);
            cmdStartStream.Name = "cmdStartStream";
            cmdStartStream.Size = new System.Drawing.Size(356, 23);
            cmdStartStream.TabIndex = 5;
            cmdStartStream.Text = "Start Stream";
            cmdStartStream.UseVisualStyleBackColor = true;
            cmdStartStream.Click += new System.EventHandler(this.cmdStartStream_Click);
            // 
            // cmdSeekToPos
            // 
            this.cmdSeekToPos.Location = new System.Drawing.Point(293, 301);
            this.cmdSeekToPos.Name = "cmdSeekToPos";
            this.cmdSeekToPos.Size = new System.Drawing.Size(75, 23);
            this.cmdSeekToPos.TabIndex = 0;
            this.cmdSeekToPos.Text = "Seek";
            this.cmdSeekToPos.UseVisualStyleBackColor = true;
            this.cmdSeekToPos.Click += new System.EventHandler(this.cmdSeekToPos_Click);
            // 
            // txtStartPos
            // 
            this.txtStartPos.Location = new System.Drawing.Point(11, 301);
            this.txtStartPos.Name = "txtStartPos";
            this.txtStartPos.Size = new System.Drawing.Size(276, 20);
            this.txtStartPos.TabIndex = 2;
            // 
            // cmdInitMovie
            // 
            this.cmdInitMovie.Location = new System.Drawing.Point(329, 67);
            this.cmdInitMovie.Name = "cmdInitMovie";
            this.cmdInitMovie.Size = new System.Drawing.Size(41, 23);
            this.cmdInitMovie.TabIndex = 3;
            this.cmdInitMovie.Text = "Init Streaming";
            this.cmdInitMovie.UseVisualStyleBackColor = true;
            this.cmdInitMovie.Click += new System.EventHandler(this.cmdInitMovie_Click);
            // 
            // cmdFinishStreaming
            // 
            this.cmdFinishStreaming.Location = new System.Drawing.Point(12, 359);
            this.cmdFinishStreaming.Name = "cmdFinishStreaming";
            this.cmdFinishStreaming.Size = new System.Drawing.Size(356, 23);
            this.cmdFinishStreaming.TabIndex = 3;
            this.cmdFinishStreaming.Text = "Finish Streaming";
            this.cmdFinishStreaming.UseVisualStyleBackColor = true;
            this.cmdFinishStreaming.Click += new System.EventHandler(this.cmdFinishStreaming_Click);
            // 
            // cmdConnect
            // 
            this.cmdConnect.Location = new System.Drawing.Point(12, 12);
            this.cmdConnect.Name = "cmdConnect";
            this.cmdConnect.Size = new System.Drawing.Size(355, 23);
            this.cmdConnect.TabIndex = 4;
            this.cmdConnect.Text = "Connect";
            this.cmdConnect.UseVisualStyleBackColor = true;
            this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
            // 
            // cmdSeek0
            // 
            this.cmdSeek0.Location = new System.Drawing.Point(31, 236);
            this.cmdSeek0.Name = "cmdSeek0";
            this.cmdSeek0.Size = new System.Drawing.Size(46, 23);
            this.cmdSeek0.TabIndex = 6;
            this.cmdSeek0.Text = "0";
            this.cmdSeek0.UseVisualStyleBackColor = true;
            this.cmdSeek0.Click += new System.EventHandler(this.cmdSeek0_Click);
            // 
            // cmdSeek100
            // 
            this.cmdSeek100.Location = new System.Drawing.Point(83, 236);
            this.cmdSeek100.Name = "cmdSeek100";
            this.cmdSeek100.Size = new System.Drawing.Size(46, 23);
            this.cmdSeek100.TabIndex = 6;
            this.cmdSeek100.Text = "100";
            this.cmdSeek100.UseVisualStyleBackColor = true;
            this.cmdSeek100.Click += new System.EventHandler(this.cmdSeek100_Click);
            // 
            // cmdSeek200
            // 
            this.cmdSeek200.Location = new System.Drawing.Point(135, 236);
            this.cmdSeek200.Name = "cmdSeek200";
            this.cmdSeek200.Size = new System.Drawing.Size(46, 23);
            this.cmdSeek200.TabIndex = 6;
            this.cmdSeek200.Text = "200";
            this.cmdSeek200.UseVisualStyleBackColor = true;
            this.cmdSeek200.Click += new System.EventHandler(this.cmdSeek200_Click);
            // 
            // cmdSeek300
            // 
            this.cmdSeek300.Location = new System.Drawing.Point(187, 236);
            this.cmdSeek300.Name = "cmdSeek300";
            this.cmdSeek300.Size = new System.Drawing.Size(46, 23);
            this.cmdSeek300.TabIndex = 6;
            this.cmdSeek300.Text = "300";
            this.cmdSeek300.UseVisualStyleBackColor = true;
            this.cmdSeek300.Click += new System.EventHandler(this.cmdSeek300_Click);
            // 
            // cmdSeek400
            // 
            this.cmdSeek400.Location = new System.Drawing.Point(239, 236);
            this.cmdSeek400.Name = "cmdSeek400";
            this.cmdSeek400.Size = new System.Drawing.Size(46, 23);
            this.cmdSeek400.TabIndex = 6;
            this.cmdSeek400.Text = "400";
            this.cmdSeek400.UseVisualStyleBackColor = true;
            this.cmdSeek400.Click += new System.EventHandler(this.cmdSeek400_Click);
            // 
            // cmdSeek500
            // 
            this.cmdSeek500.Location = new System.Drawing.Point(291, 236);
            this.cmdSeek500.Name = "cmdSeek500";
            this.cmdSeek500.Size = new System.Drawing.Size(46, 23);
            this.cmdSeek500.TabIndex = 6;
            this.cmdSeek500.Text = "500";
            this.cmdSeek500.UseVisualStyleBackColor = true;
            this.cmdSeek500.Click += new System.EventHandler(this.cmdSeek500_Click);
            // 
            // cmdSeek1000
            // 
            this.cmdSeek1000.Location = new System.Drawing.Point(31, 265);
            this.cmdSeek1000.Name = "cmdSeek1000";
            this.cmdSeek1000.Size = new System.Drawing.Size(46, 23);
            this.cmdSeek1000.TabIndex = 6;
            this.cmdSeek1000.Text = "1000";
            this.cmdSeek1000.UseVisualStyleBackColor = true;
            this.cmdSeek1000.Click += new System.EventHandler(this.cmdSeek1000_Click);
            // 
            // cmdSeek1500
            // 
            this.cmdSeek1500.Location = new System.Drawing.Point(83, 265);
            this.cmdSeek1500.Name = "cmdSeek1500";
            this.cmdSeek1500.Size = new System.Drawing.Size(46, 23);
            this.cmdSeek1500.TabIndex = 6;
            this.cmdSeek1500.Text = "1500";
            this.cmdSeek1500.UseVisualStyleBackColor = true;
            this.cmdSeek1500.Click += new System.EventHandler(this.cmdSeek1500_Click);
            // 
            // cmdSeek2000
            // 
            this.cmdSeek2000.Location = new System.Drawing.Point(135, 265);
            this.cmdSeek2000.Name = "cmdSeek2000";
            this.cmdSeek2000.Size = new System.Drawing.Size(46, 23);
            this.cmdSeek2000.TabIndex = 6;
            this.cmdSeek2000.Text = "2000";
            this.cmdSeek2000.UseVisualStyleBackColor = true;
            this.cmdSeek2000.Click += new System.EventHandler(this.cmdSeek2000_Click);
            // 
            // cmdSeek2500
            // 
            this.cmdSeek2500.Location = new System.Drawing.Point(187, 265);
            this.cmdSeek2500.Name = "cmdSeek2500";
            this.cmdSeek2500.Size = new System.Drawing.Size(46, 23);
            this.cmdSeek2500.TabIndex = 6;
            this.cmdSeek2500.Text = "2500";
            this.cmdSeek2500.UseVisualStyleBackColor = true;
            this.cmdSeek2500.Click += new System.EventHandler(this.cmdSeek2500_Click);
            // 
            // cmdSeek3000
            // 
            this.cmdSeek3000.Location = new System.Drawing.Point(239, 265);
            this.cmdSeek3000.Name = "cmdSeek3000";
            this.cmdSeek3000.Size = new System.Drawing.Size(46, 23);
            this.cmdSeek3000.TabIndex = 6;
            this.cmdSeek3000.Text = "3000";
            this.cmdSeek3000.UseVisualStyleBackColor = true;
            this.cmdSeek3000.Click += new System.EventHandler(this.cmdSeek3000_Click);
            // 
            // cmdSeek3600
            // 
            this.cmdSeek3600.Location = new System.Drawing.Point(291, 265);
            this.cmdSeek3600.Name = "cmdSeek3600";
            this.cmdSeek3600.Size = new System.Drawing.Size(46, 23);
            this.cmdSeek3600.TabIndex = 6;
            this.cmdSeek3600.Text = "3600";
            this.cmdSeek3600.UseVisualStyleBackColor = true;
            this.cmdSeek3600.Click += new System.EventHandler(this.cmdSeek3600_Click);
            // 
            // lbLog
            // 
            this.lbLog.FormattingEnabled = true;
            this.lbLog.Location = new System.Drawing.Point(12, 388);
            this.lbLog.Name = "lbLog";
            this.lbLog.Size = new System.Drawing.Size(353, 225);
            this.lbLog.TabIndex = 7;
            // 
            // txtFileSize
            // 
            this.txtFileSize.Location = new System.Drawing.Point(12, 619);
            this.txtFileSize.Name = "txtFileSize";
            this.txtFileSize.Size = new System.Drawing.Size(353, 20);
            this.txtFileSize.TabIndex = 9;
            // 
            // cbMovies
            // 
            this.cbMovies.FormattingEnabled = true;
            this.cbMovies.Location = new System.Drawing.Point(64, 69);
            this.cbMovies.Name = "cbMovies";
            this.cbMovies.Size = new System.Drawing.Size(258, 21);
            this.cbMovies.TabIndex = 12;
            // 
            // cbChannels
            // 
            this.cbChannels.FormattingEnabled = true;
            this.cbChannels.Location = new System.Drawing.Point(63, 124);
            this.cbChannels.Name = "cbChannels";
            this.cbChannels.Size = new System.Drawing.Size(259, 21);
            this.cbChannels.TabIndex = 14;
            // 
            // cmdInitChannel
            // 
            this.cmdInitChannel.Location = new System.Drawing.Point(328, 122);
            this.cmdInitChannel.Name = "cmdInitChannel";
            this.cmdInitChannel.Size = new System.Drawing.Size(41, 23);
            this.cmdInitChannel.TabIndex = 15;
            this.cmdInitChannel.Text = "Init Streaming";
            this.cmdInitChannel.UseVisualStyleBackColor = true;
            this.cmdInitChannel.Click += new System.EventHandler(this.cmdInitChannel_Click);
            // 
            // cmdPlayInVlc
            // 
            this.cmdPlayInVlc.Location = new System.Drawing.Point(293, 332);
            this.cmdPlayInVlc.Name = "cmdPlayInVlc";
            this.cmdPlayInVlc.Size = new System.Drawing.Size(75, 23);
            this.cmdPlayInVlc.TabIndex = 16;
            this.cmdPlayInVlc.Text = "Play";
            this.cmdPlayInVlc.UseVisualStyleBackColor = true;
            this.cmdPlayInVlc.Click += new System.EventHandler(this.cmdPlayInVlc_Click);
            // 
            // cbStartAutoPlayback
            // 
            this.cbStartAutoPlayback.AutoSize = true;
            this.cbStartAutoPlayback.Location = new System.Drawing.Point(12, 334);
            this.cbStartAutoPlayback.Name = "cbStartAutoPlayback";
            this.cbStartAutoPlayback.Size = new System.Drawing.Size(157, 17);
            this.cbStartAutoPlayback.TabIndex = 17;
            this.cbStartAutoPlayback.Text = "Automatically start playback";
            this.cbStartAutoPlayback.UseVisualStyleBackColor = true;
            // 
            // cbLanguage
            // 
            this.cbLanguage.FormattingEnabled = true;
            this.cbLanguage.Location = new System.Drawing.Point(64, 152);
            this.cbLanguage.Name = "cbLanguage";
            this.cbLanguage.Size = new System.Drawing.Size(303, 21);
            this.cbLanguage.TabIndex = 18;
            // 
            // cbProfiles
            // 
            this.cbProfiles.FormattingEnabled = true;
            this.cbProfiles.Location = new System.Drawing.Point(63, 41);
            this.cbProfiles.Name = "cbProfiles";
            this.cbProfiles.Size = new System.Drawing.Size(304, 21);
            this.cbProfiles.TabIndex = 19;
            // 
            // cbSubtitle
            // 
            this.cbSubtitle.FormattingEnabled = true;
            this.cbSubtitle.Location = new System.Drawing.Point(63, 180);
            this.cbSubtitle.Name = "cbSubtitle";
            this.cbSubtitle.Size = new System.Drawing.Size(304, 21);
            this.cbSubtitle.TabIndex = 20;
            // 
            // cmdInitIdTypeStreaming
            // 
            this.cmdInitIdTypeStreaming.Location = new System.Drawing.Point(329, 93);
            this.cmdInitIdTypeStreaming.Name = "cmdInitIdTypeStreaming";
            this.cmdInitIdTypeStreaming.Size = new System.Drawing.Size(41, 23);
            this.cmdInitIdTypeStreaming.TabIndex = 21;
            this.cmdInitIdTypeStreaming.Text = "Init Streaming";
            this.cmdInitIdTypeStreaming.UseVisualStyleBackColor = true;
            this.cmdInitIdTypeStreaming.Click += new System.EventHandler(this.cmdInitIdTypeStreaming_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 101);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "Type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(236, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(16, 13);
            this.label2.TabIndex = 22;
            this.label2.Text = "Id";
            // 
            // txtItemId
            // 
            this.txtItemId.Location = new System.Drawing.Point(259, 98);
            this.txtItemId.Name = "txtItemId";
            this.txtItemId.Size = new System.Drawing.Size(63, 20);
            this.txtItemId.TabIndex = 23;
            // 
            // cbItemType
            // 
            this.cbItemType.FormattingEnabled = true;
            this.cbItemType.Location = new System.Drawing.Point(64, 97);
            this.cbItemType.Name = "cbItemType";
            this.cbItemType.Size = new System.Drawing.Size(80, 21);
            this.cbItemType.TabIndex = 24;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 183);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Subtitles";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 155);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 26;
            this.label4.Text = "Language";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 127);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(51, 13);
            this.label5.TabIndex = 25;
            this.label5.Text = "Channels";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(10, 72);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(36, 13);
            this.label6.TabIndex = 25;
            this.label6.Text = "Movie";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(10, 44);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 13);
            this.label8.TabIndex = 25;
            this.label8.Text = "Profiles";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(150, 101);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(46, 13);
            this.label9.TabIndex = 27;
            this.label9.Text = "Provider";
            // 
            // txtProvider
            // 
            this.txtProvider.Location = new System.Drawing.Point(202, 98);
            this.txtProvider.Name = "txtProvider";
            this.txtProvider.Size = new System.Drawing.Size(31, 20);
            this.txtProvider.TabIndex = 28;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 675);
            this.Controls.Add(this.txtProvider);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbItemType);
            this.Controls.Add(this.txtItemId);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdInitIdTypeStreaming);
            this.Controls.Add(this.cbSubtitle);
            this.Controls.Add(this.cbProfiles);
            this.Controls.Add(this.cbLanguage);
            this.Controls.Add(this.cbStartAutoPlayback);
            this.Controls.Add(this.cmdPlayInVlc);
            this.Controls.Add(this.cmdInitChannel);
            this.Controls.Add(this.cbChannels);
            this.Controls.Add(this.cbMovies);
            this.Controls.Add(this.txtFileSize);
            this.Controls.Add(this.lbLog);
            this.Controls.Add(this.cmdSeek3600);
            this.Controls.Add(this.cmdSeek500);
            this.Controls.Add(this.cmdSeek3000);
            this.Controls.Add(this.cmdSeek400);
            this.Controls.Add(this.cmdSeek2500);
            this.Controls.Add(this.cmdSeek300);
            this.Controls.Add(this.cmdSeek2000);
            this.Controls.Add(this.cmdSeek200);
            this.Controls.Add(this.cmdSeek1500);
            this.Controls.Add(this.cmdSeek100);
            this.Controls.Add(this.cmdSeek1000);
            this.Controls.Add(this.cmdSeek0);
            this.Controls.Add(cmdStartStream);
            this.Controls.Add(this.cmdConnect);
            this.Controls.Add(this.cmdFinishStreaming);
            this.Controls.Add(this.cmdInitMovie);
            this.Controls.Add(this.txtStartPos);
            this.Controls.Add(this.cmdSeekToPos);
            this.Name = "Form1";
            this.Text = "Streaming Tester";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdSeekToPos;
        private System.Windows.Forms.TextBox txtStartPos;
        private System.Windows.Forms.Button cmdInitMovie;
        private System.Windows.Forms.Button cmdFinishStreaming;
        private System.Windows.Forms.Button cmdConnect;
        private System.Windows.Forms.Button cmdSeek0;
        private System.Windows.Forms.Button cmdSeek100;
        private System.Windows.Forms.Button cmdSeek200;
        private System.Windows.Forms.Button cmdSeek300;
        private System.Windows.Forms.Button cmdSeek400;
        private System.Windows.Forms.Button cmdSeek500;
        private System.Windows.Forms.Button cmdSeek1000;
        private System.Windows.Forms.Button cmdSeek1500;
        private System.Windows.Forms.Button cmdSeek2000;
        private System.Windows.Forms.Button cmdSeek2500;
        private System.Windows.Forms.Button cmdSeek3000;
        private System.Windows.Forms.Button cmdSeek3600;
        private System.Windows.Forms.ListBox lbLog;
        private System.Windows.Forms.TextBox txtFileSize;
        private System.Windows.Forms.ComboBox cbMovies;
        private System.Windows.Forms.ComboBox cbChannels;
        private System.Windows.Forms.Button cmdInitChannel;
        private System.Windows.Forms.Button cmdPlayInVlc;
        private System.Windows.Forms.CheckBox cbStartAutoPlayback;
        private System.Windows.Forms.ComboBox cbLanguage;
        private System.Windows.Forms.ComboBox cbProfiles;
        private System.Windows.Forms.ComboBox cbSubtitle;
        private System.Windows.Forms.Button cmdInitIdTypeStreaming;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtItemId;
        private System.Windows.Forms.ComboBox cbItemType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtProvider;
    }
}

