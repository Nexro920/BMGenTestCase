namespace BMGenTest
{
    public partial class BMGenTool
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BMGenTool));
            this.radioButtoniTC = new System.Windows.Forms.RadioButton();
            this.radioButtonInteroperable = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonOutput = new System.Windows.Forms.Button();
            this.label19 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelGen = new System.Windows.Forms.Label();
            this.checkBoxGenBin = new System.Windows.Forms.CheckBox();
            this.tabPageLog = new System.Windows.Forms.TabPage();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.tabPageBeacon = new System.Windows.Forms.TabPage();
            this.textBoxLayout = new System.Windows.Forms.TextBox();
            this.buttonlayout = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonBoundary = new System.Windows.Forms.Button();
            this.textBoxBoundaryBeacon = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxSyDB = new System.Windows.Forms.TextBox();
            this.buttonSyDB = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.tags = new System.Windows.Forms.TabControl();
            this.tabPageUpstream = new System.Windows.Forms.TabPage();
            this.buttonUpstreamFile = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_UpstreamFile = new System.Windows.Forms.TextBox();
            this.Upstream_path_considered = new System.Windows.Forms.CheckBox();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.tabPageLog.SuspendLayout();
            this.tabPageBeacon.SuspendLayout();
            this.tags.SuspendLayout();
            this.tabPageUpstream.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioButtoniTC
            // 
            this.radioButtoniTC.AutoSize = true;
            this.radioButtoniTC.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtoniTC.Location = new System.Drawing.Point(138, 20);
            this.radioButtoniTC.Name = "radioButtoniTC";
            this.radioButtoniTC.Size = new System.Drawing.Size(46, 19);
            this.radioButtoniTC.TabIndex = 2;
            this.radioButtoniTC.Text = "iTC";
            this.radioButtoniTC.UseVisualStyleBackColor = true;
            this.radioButtoniTC.Visible = false;
            this.radioButtoniTC.CheckedChanged += new System.EventHandler(this.radioButtoniTC_CheckedChanged);
            // 
            // radioButtonInteroperable
            // 
            this.radioButtonInteroperable.AutoSize = true;
            this.radioButtonInteroperable.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonInteroperable.Location = new System.Drawing.Point(186, 20);
            this.radioButtonInteroperable.Name = "radioButtonInteroperable";
            this.radioButtonInteroperable.Size = new System.Drawing.Size(111, 19);
            this.radioButtonInteroperable.TabIndex = 3;
            this.radioButtonInteroperable.Text = "Interoperable";
            this.radioButtonInteroperable.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(12, 543);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(214, 15);
            this.label4.TabIndex = 11;
            this.label4.Text = "Generated file(s) output directory path:";
            // 
            // buttonOutput
            // 
            this.buttonOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOutput.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOutput.Image = ((System.Drawing.Image)(resources.GetObject("buttonOutput.Image")));
            this.buttonOutput.Location = new System.Drawing.Point(758, 562);
            this.buttonOutput.Name = "buttonOutput";
            this.buttonOutput.Size = new System.Drawing.Size(25, 25);
            this.buttonOutput.TabIndex = 13;
            this.buttonOutput.UseVisualStyleBackColor = true;
            this.buttonOutput.Click += new System.EventHandler(this.buttonOutput_Click);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label19.Location = new System.Drawing.Point(11, 22);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(96, 15);
            this.label19.TabIndex = 14;
            this.label19.Text = "Project mode:";
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBar1.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.progressBar1.Location = new System.Drawing.Point(15, 637);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(737, 23);
            this.progressBar1.TabIndex = 19;
            this.progressBar1.Click += new System.EventHandler(this.progressBar1_Click);
            // 
            // labelGen
            // 
            this.labelGen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelGen.AutoSize = true;
            this.labelGen.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelGen.Location = new System.Drawing.Point(15, 613);
            this.labelGen.Name = "labelGen";
            this.labelGen.Size = new System.Drawing.Size(183, 14);
            this.labelGen.TabIndex = 20;
            this.labelGen.Text = "Generate (click here):";
            // 
            // checkBoxGenBin
            // 
            this.checkBoxGenBin.AutoSize = true;
            this.checkBoxGenBin.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.checkBoxGenBin.Location = new System.Drawing.Point(16, 73);
            this.checkBoxGenBin.Name = "checkBoxGenBin";
            this.checkBoxGenBin.Size = new System.Drawing.Size(145, 19);
            this.checkBoxGenBin.TabIndex = 21;
            this.checkBoxGenBin.Text = "Generate Bin Files";
            this.checkBoxGenBin.UseVisualStyleBackColor = true;
            // 
            // tabPageLog
            // 
            this.tabPageLog.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.tabPageLog.Controls.Add(this.listBoxLog);
            this.tabPageLog.Location = new System.Drawing.Point(4, 24);
            this.tabPageLog.Name = "tabPageLog";
            this.tabPageLog.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLog.Size = new System.Drawing.Size(786, 361);
            this.tabPageLog.TabIndex = 3;
            this.tabPageLog.Text = "Logs";
            // 
            // listBoxLog
            // 
            this.listBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxLog.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.listBoxLog.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.HorizontalScrollbar = true;
            this.listBoxLog.ItemHeight = 15;
            this.listBoxLog.Location = new System.Drawing.Point(3, 3);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(778, 349);
            this.listBoxLog.TabIndex = 0;
            this.listBoxLog.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.listBoxLog_DrawItem);
            // 
            // tabPageBeacon
            // 
            this.tabPageBeacon.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.tabPageBeacon.Controls.Add(this.textBoxLayout);
            this.tabPageBeacon.Controls.Add(this.buttonlayout);
            this.tabPageBeacon.Controls.Add(this.label3);
            this.tabPageBeacon.Controls.Add(this.buttonBoundary);
            this.tabPageBeacon.Controls.Add(this.textBoxBoundaryBeacon);
            this.tabPageBeacon.Controls.Add(this.label1);
            this.tabPageBeacon.Controls.Add(this.textBoxSyDB);
            this.tabPageBeacon.Controls.Add(this.buttonSyDB);
            this.tabPageBeacon.Controls.Add(this.label5);
            this.tabPageBeacon.Location = new System.Drawing.Point(4, 24);
            this.tabPageBeacon.Name = "tabPageBeacon";
            this.tabPageBeacon.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageBeacon.Size = new System.Drawing.Size(786, 361);
            this.tabPageBeacon.TabIndex = 1;
            this.tabPageBeacon.Text = "Input Data Files";
            // 
            // textBoxLayout
            // 
            this.textBoxLayout.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.textBoxLayout.Location = new System.Drawing.Point(20, 169);
            this.textBoxLayout.Name = "textBoxLayout";
            this.textBoxLayout.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxLayout.Size = new System.Drawing.Size(716, 21);
            this.textBoxLayout.TabIndex = 29;
            this.textBoxLayout.TextChanged += new System.EventHandler(this.textBoxLayout_TextChanged);
            // 
            // buttonlayout
            // 
            this.buttonlayout.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonlayout.Image = ((System.Drawing.Image)(resources.GetObject("buttonlayout.Image")));
            this.buttonlayout.Location = new System.Drawing.Point(742, 168);
            this.buttonlayout.Name = "buttonlayout";
            this.buttonlayout.Size = new System.Drawing.Size(25, 25);
            this.buttonlayout.TabIndex = 30;
            this.buttonlayout.UseVisualStyleBackColor = true;
            this.buttonlayout.Click += new System.EventHandler(this.buttonlayout_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 142);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(153, 15);
            this.label3.TabIndex = 28;
            this.label3.Text = "Beacon layout csv file path:";
            // 
            // buttonBoundary
            // 
            this.buttonBoundary.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonBoundary.Image = ((System.Drawing.Image)(resources.GetObject("buttonBoundary.Image")));
            this.buttonBoundary.Location = new System.Drawing.Point(742, 251);
            this.buttonBoundary.Name = "buttonBoundary";
            this.buttonBoundary.Size = new System.Drawing.Size(25, 25);
            this.buttonBoundary.TabIndex = 27;
            this.buttonBoundary.UseVisualStyleBackColor = true;
            this.buttonBoundary.Click += new System.EventHandler(this.buttonBoundary_Click);
            // 
            // textBoxBoundaryBeacon
            // 
            this.textBoxBoundaryBeacon.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.textBoxBoundaryBeacon.Location = new System.Drawing.Point(19, 254);
            this.textBoxBoundaryBeacon.Name = "textBoxBoundaryBeacon";
            this.textBoxBoundaryBeacon.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxBoundaryBeacon.Size = new System.Drawing.Size(716, 21);
            this.textBoxBoundaryBeacon.TabIndex = 26;
            this.textBoxBoundaryBeacon.TextChanged += new System.EventHandler(this.textBoxBoundaryBeacon_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 225);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(243, 15);
            this.label1.TabIndex = 25;
            this.label1.Text = "line_boundary_BM_beacons XML file path:";
            // 
            // textBoxSyDB
            // 
            this.textBoxSyDB.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.textBoxSyDB.Location = new System.Drawing.Point(20, 84);
            this.textBoxSyDB.Name = "textBoxSyDB";
            this.textBoxSyDB.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxSyDB.Size = new System.Drawing.Size(716, 21);
            this.textBoxSyDB.TabIndex = 23;
            this.textBoxSyDB.TextChanged += new System.EventHandler(this.textBoxSyDB_TextChanged);
            // 
            // buttonSyDB
            // 
            this.buttonSyDB.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSyDB.Image = ((System.Drawing.Image)(resources.GetObject("buttonSyDB.Image")));
            this.buttonSyDB.Location = new System.Drawing.Point(742, 83);
            this.buttonSyDB.Name = "buttonSyDB";
            this.buttonSyDB.Size = new System.Drawing.Size(25, 25);
            this.buttonSyDB.TabIndex = 24;
            this.buttonSyDB.UseVisualStyleBackColor = true;
            this.buttonSyDB.Click += new System.EventHandler(this.buttonSyDB_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 62);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(115, 15);
            this.label5.TabIndex = 22;
            this.label5.Text = "SyDB XML file path:";
            // 
            // tags
            // 
            this.tags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tags.Controls.Add(this.tabPageBeacon);
            this.tags.Controls.Add(this.tabPageLog);
            this.tags.Controls.Add(this.tabPageUpstream);
            this.tags.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tags.Location = new System.Drawing.Point(12, 119);
            this.tags.Name = "tags";
            this.tags.SelectedIndex = 0;
            this.tags.Size = new System.Drawing.Size(794, 389);
            this.tags.TabIndex = 5;
            // 
            // tabPageUpstream
            // 
            this.tabPageUpstream.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.tabPageUpstream.Controls.Add(this.buttonUpstreamFile);
            this.tabPageUpstream.Controls.Add(this.label2);
            this.tabPageUpstream.Controls.Add(this.textBox_UpstreamFile);
            this.tabPageUpstream.Controls.Add(this.Upstream_path_considered);
            this.tabPageUpstream.Location = new System.Drawing.Point(4, 24);
            this.tabPageUpstream.Name = "tabPageUpstream";
            this.tabPageUpstream.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageUpstream.Size = new System.Drawing.Size(786, 361);
            this.tabPageUpstream.TabIndex = 4;
            this.tabPageUpstream.Text = "iTC_Upstream";
            // 
            // buttonUpstreamFile
            // 
            this.buttonUpstreamFile.Font = new System.Drawing.Font("Times New Roman", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonUpstreamFile.Image = ((System.Drawing.Image)(resources.GetObject("buttonUpstreamFile.Image")));
            this.buttonUpstreamFile.Location = new System.Drawing.Point(483, 171);
            this.buttonUpstreamFile.Name = "buttonUpstreamFile";
            this.buttonUpstreamFile.Size = new System.Drawing.Size(25, 25);
            this.buttonUpstreamFile.TabIndex = 14;
            this.buttonUpstreamFile.UseVisualStyleBackColor = true;
            this.buttonUpstreamFile.Click += new System.EventHandler(this.buttonUpstreamFile_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 144);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(172, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "line boundary BM beacons file";
            // 
            // textBox_UpstreamFile
            // 
            this.textBox_UpstreamFile.Location = new System.Drawing.Point(12, 173);
            this.textBox_UpstreamFile.Name = "textBox_UpstreamFile";
            this.textBox_UpstreamFile.Size = new System.Drawing.Size(462, 21);
            this.textBox_UpstreamFile.TabIndex = 1;
            // 
            // Upstream_path_considered
            // 
            this.Upstream_path_considered.AutoSize = true;
            this.Upstream_path_considered.Location = new System.Drawing.Point(12, 81);
            this.Upstream_path_considered.Name = "Upstream_path_considered";
            this.Upstream_path_considered.Size = new System.Drawing.Size(171, 19);
            this.Upstream_path_considered.TabIndex = 0;
            this.Upstream_path_considered.Text = "Upstream path considered";
            this.Upstream_path_considered.UseVisualStyleBackColor = true;
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxOutput.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.textBoxOutput.Location = new System.Drawing.Point(12, 566);
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.textBoxOutput.Size = new System.Drawing.Size(740, 21);
            this.textBoxOutput.TabIndex = 24;
            this.textBoxOutput.TextChanged += new System.EventHandler(this.textBoxOutput_TextChanged);
            // 
            // BMGenTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.ClientSize = new System.Drawing.Size(817, 680);
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.checkBoxGenBin);
            this.Controls.Add(this.labelGen);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.buttonOutput);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tags);
            this.Controls.Add(this.radioButtonInteroperable);
            this.Controls.Add(this.radioButtoniTC);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BMGenTool";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BMGen_FormClosing);
            this.tabPageLog.ResumeLayout(false);
            this.tabPageBeacon.ResumeLayout(false);
            this.tabPageBeacon.PerformLayout();
            this.tags.ResumeLayout(false);
            this.tabPageUpstream.ResumeLayout(false);
            this.tabPageUpstream.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButtoniTC;
        private System.Windows.Forms.RadioButton radioButtonInteroperable;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonOutput;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label labelGen;
        private System.Windows.Forms.CheckBox checkBoxGenBin;
        private System.Windows.Forms.TabPage tabPageLog;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.TabPage tabPageBeacon;
        private System.Windows.Forms.TextBox textBoxSyDB;
        private System.Windows.Forms.Button buttonSyDB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabControl tags;
        private System.Windows.Forms.TabPage tabPageUpstream;
        private System.Windows.Forms.Button buttonUpstreamFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_UpstreamFile;
        private System.Windows.Forms.CheckBox Upstream_path_considered;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonBoundary;
        private System.Windows.Forms.TextBox textBoxBoundaryBeacon;
        private System.Windows.Forms.TextBox textBoxLayout;
        private System.Windows.Forms.Button buttonlayout;
        private System.Windows.Forms.Label label3;
    }
}
