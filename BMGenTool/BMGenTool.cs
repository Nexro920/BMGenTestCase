using System;
using System.Windows.Forms;
using BMGenTest.UI;
using System.IO;
using BMGenTool;
//using Summer.System.IO;
using MetaFly.Summer.IO;
using System.Threading;
using BMGenTool.Generate;
using BMGenTool.Info;
//using TOOLCommon.Trace;
using MetaFly.Summer.Generic;
using System.Collections.Generic;
using MetaFly.Serialization;
using System.Reflection;
using BMGenTool.Common;

namespace BMGenTest
{
    //for the main Form
    //load the old config and save new config
    //deal with the log and main process
    public partial class BMGenTool : Form
    {
        private int listBoxMaxWidth = 0;

        private string logFile = string.Empty;
        private string logMsg = string.Empty;

        private string configFile = string.Empty;

        private List<BEACON> beaconList;
        private List<LEU> leuList;

        private string currentRunDir;
        private bool IsBusy = false;

        public BMGenTool()
        {
            InitializeComponent();
            
            beaconList = new List<BEACON>();
            leuList = new List<LEU>();

            //cur dir should be the exe run dir
            //currentRunDir = System.IO.Directory.GetCurrentDirectory();
            string exeFullName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            currentRunDir = System.Windows.Forms.Application.StartupPath;

            {//set all the log shows in the logbox
                TraceMethod.SetHMITraceTextBox(TraceMethod.TraceKind.DEBUG, listBoxLog);
                TraceMethod.SetHMITraceTextBox(TraceMethod.TraceKind.INFO, listBoxLog);
                TraceMethod.SetHMITraceTextBox(TraceMethod.TraceKind.ERROR, listBoxLog);
                TraceMethod.SetHMITraceTextBox(TraceMethod.TraceKind.WARNING, listBoxLog);

                TraceMethod.SetLogTraceFileProperties(true, "");
            }
            Init();
        }

        private void BMGen_FormClosing(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }
        private void Init()
        {
            logFile = currentRunDir + "\\Log\\BMGen.log";
            logMsg = "BMGen started.";
            TraceMethod.RecordInfo(logMsg);

            //deal the config.xml which used to save the configuration
            configFile = currentRunDir + "\\Config\\config.xml";

            if (!File.Exists(configFile))
            {//create a new config file, which is empty
                XmlFileHelper xmlFile = XmlFileHelper.CreateFromString(null);
                xmlFile.SetRoot("config", null);
                if (!Directory.Exists(currentRunDir + "\\Config\\"))
                {
                    Directory.CreateDirectory(currentRunDir + "\\Config\\");
                }
                xmlFile.Save2File(configFile);
            }
            else
            {
                //set the input file path
                setTextBox(textBoxSyDB, "BeaconLayout_SYDB");
                setTextBox(textBoxLayout, "Layout_beacons");
                setTextBox(textBoxBoundaryBeacon, "Boundary_beacons");
                //set the output path
                setTextBox(textBoxOutput, "Output");
                setTextBox(textBox_UpstreamFile, "Upstream_File");
            }

            if(BMGenTest.Program.GenerateTJFormat)
            {
                Assembly ass = Assembly.LoadFile(Path.GetFullPath(".//MetaFly.dll"));
                Version ver = ass.GetName().Version;
                TraceMethod.RecordInfo("MetaFly.dll "+ ver.ToString());
            }

            {
                this.Text = "Bcode_iTRNV-BMGenTool2_V1.0.2_Build_20200409_debug";
                //set the project 
                radioButtoniTC.Checked = false;
                Upstream_path_considered.Checked = false;
                radioButtonInteroperable.Checked = true;
                //set if choose generate bin files
                checkBoxGenBin.Checked = false;
                UpdateUpstreamPage();
            }

            if (Program.AUTOTEST)
            {
                object nullobj = new object();
                EventArgs nulle = new EventArgs();
                progressBar1_Click(nullobj, nulle);
            }
        }

        private void buttonBoundary_Click(object sender, EventArgs e)
        {
            String filter = "XML文件|*.xml";
            buttonFileClick(filter, this.textBoxBoundaryBeacon, "Boundary_beacons");
        }
        private void buttonlayout_Click(object sender, EventArgs e)
        {
            String filter = "CSV文件|*.csv";
            buttonFileClick(filter, this.textBoxLayout, "Layout_beacons");
        }

        private void buttonUpstreamFile_Click(object sender, EventArgs e)
        {
            String filter = "XML文件|*.xml";
            buttonFileClick(filter, this.textBox_UpstreamFile, "Upstream_File");
        }

        private void buttonSyDB_Click(object sender, EventArgs e)
        {
            String filter = "XML文件|*.xml";
            buttonFileClick(filter, this.textBoxSyDB, "BeaconLayout_SYDB");
        }
        private void buttonOutput_Click(object sender, EventArgs e)
        {
            buttonFolderClick(this.textBoxOutput, "Output");
        }

        private bool clearOutputDir()
        {
            if ("" == this.textBoxOutput.Text)
            {
                TraceMethod.RecordInfo($"Error: output is null, please set output dir");
                return false;
            }

            //if output dir exist the generate output files, delete them to avoid the old data used as new data
            string outdir = this.textBoxOutput.Text;
            if (Directory.Exists(outdir))
            {
                string[] outFolders = { "\\Beacon", "\\BMV", "\\LEU", "\\LEUBinary" };
                foreach (string folder in outFolders)
                {
                    if (Directory.Exists(outdir + folder))
                    {
                        Directory.Delete(outdir + folder, true);
                    }
                }
            }
            return true;
        }

        private void Generate()
        {
            try
            {
                beaconList.Clear();
                leuList.Clear();
                SyDB.GetInstance().clear();

                if (false == clearOutputDir())
                {
                    return;
                }
                //log is updating
                {
                    IDataGen.toolVer = this.Text;
                    GENERIC_SYSTEM_PARAMETERS sydb = FileLoader.Load<GENERIC_SYSTEM_PARAMETERS>(this.textBoxSyDB.Text);
                    SyDB.GetInstance().LoadData(sydb);

                    IDataGen.sydbFile = this.textBoxSyDB.Text;//todo will delete
                }

                {
                    IDataGen gen = null;
                    string compilepath = currentRunDir + "\\compiler\\CompilerBaliseV4000\\main\\compile.exe";
                    gen = new BFGen(this.textBoxLayout.Text, this.textBoxBoundaryBeacon.Text, compilepath, this.radioButtoniTC.Checked, this.checkBoxGenBin.Checked);
                    ((BFGen)gen).genPro += new BFGen.GenProess(GenProess);
                    if (false == gen.Generate(this.textBoxOutput.Text))
                    {
                        return;
                    }
                }

                {
                    IDataGen gen = null;
                    gen = new BMVFGen(this.radioButtoniTC.Checked && this.Upstream_path_considered.Checked,
                        ref beaconList,
                        ref leuList,
                        this.textBox_UpstreamFile.Text);

                    if (false == gen.Generate(this.textBoxOutput.Text))
                    {
                        return;
                    }
                }

                {
                    IDataGen gen = null;
                    gen = new LEURFGen(ref beaconList, ref leuList);
                    if (false == gen.Generate(this.textBoxOutput.Text))
                    {
                        return;
                    }
                }

                {
                    IDataGen gen = null;
                    string LEURFFile = this.textBoxOutput.Text + "\\LEU\\LEU_Result_Filtered_Values.xml";
                    
                    gen = new LEUXmlGen(leuList, LEURFFile, currentRunDir, this.radioButtoniTC.Checked, this.checkBoxGenBin.Checked);
                    ((LEUXmlGen)gen).genPro += new LEUXmlGen.GenProess(GenProess);
                    if (false == gen.Generate(this.textBoxOutput.Text))
                    {
                        return;
                    }
                }
                
            }
            finally
            {
                IsBusy = false;
            }

        }

        /// <summary>
        /// 生成数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void progressBar1_Click(object sender, EventArgs e)
        {
            if (true == IsBusy)
            {
                return;
            }
            IsBusy = true;
            //show the log tab
            this.tags.SelectedTab = tabPageLog;

            //update the interface
            GenProess(100, 0);
            TraceMethod.RecordInfo("Start to generate, please wait....");
            
            if (Program.AUTOTEST)
            {//if for test, do it directly
                Generate();
            }
            else
            {
                //create a new thread to do the generate
                Thread doGenerate = new Thread(Generate);
                doGenerate.Start();
            }
        }  

        private void setTextBox(TextBox textBox, string name)
        {
            XmlVisitor root = XmlFileHelper.CreateFromFile(configFile).GetRoot();
            XmlVisitor node = root.FirstChildByPath(name);
            if (null != node)
            {
                textBox.Text = node.Value;
            }
        }

        private void buttonFileClick(string filter, TextBox textBox, string nodeName)
        {
            string path = currentRunDir;
            if ("" != textBox.Text)
            {
                string folder = System.IO.Path.GetDirectoryName(textBox.Text);
                if (Directory.Exists(folder))
                {
                    path = folder;
                }
            }
            OpenFile file = new OpenFile(path, filter);
            file.Show(textBox);
            Save2Config(textBox, nodeName);
        }
        private void buttonFolderClick(TextBox textBox, string nodeName)
        {
            String path = currentRunDir;
            OpenFolder folder = new OpenFolder(path);
            folder.Show(textBox);
            Save2Config(textBox, nodeName);
        }
        private bool Save2Config(TextBox textBox, string nodeName)
        {
            XmlFileHelper configInfo = XmlFileHelper.CreateFromFile(configFile);
            XmlVisitor root = configInfo.GetRoot();
            XmlVisitor compiler = root.FirstChildByPath(nodeName);
            if (null == compiler)
            {
                root.AppendChild(nodeName, textBox.Text);
            }
            else
            {
                compiler.Value = textBox.Text;
            }
            configInfo.Save2File(configFile);
            return true;
        }
        public delegate void ShowGenProessDelegate(int total, int current);
        public void GenProess(int total, int current)
        {
            if (this.InvokeRequired)
            {//when other thread call this func
                ShowGenProessDelegate show = new ShowGenProessDelegate(GenProess);
                this.BeginInvoke(show, new object[] { total, current });
            }
            else
            {//when current thread call this func
                if (current > total)
                {
                    current = total;
                }
                this.progressBar1.Maximum = total;
                this.progressBar1.Value = current;
                double percent = Convert.ToDouble(current) / Convert.ToDouble(total);
            }
        }

        private void UpdateUpstreamPage()
        {
            if (this.radioButtoniTC.Checked)
            {
                Upstream_path_considered.Checked = true;
                if (false == tags.TabPages.Contains(tabPageUpstream))
                {
                    tags.TabPages.Add(tabPageUpstream);
                }
                tags.SelectedIndex = tags.TabPages.IndexOf(tabPageUpstream);
            }
            else
            {
                Upstream_path_considered.Checked = false;
                if (tags.TabPages.Contains(tabPageUpstream))
                {
                    tags.TabPages.Remove(tabPageUpstream);
                }
                tags.SelectedIndex = 0;
            }
        }
        private void radioButtoniTC_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUpstreamPage();
        }

        private void listBoxLog_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            if (e.Index < 0)
            {
                return;
            }

            if (listBoxLog.Items[e.Index].ToString().ToLower().Contains("error"))
            {
                e.Graphics.DrawString(listBoxLog.Items[e.Index].ToString(), e.Font, System.Drawing.Brushes.Red, e.Bounds);
            }
            else if (listBoxLog.Items[e.Index].ToString().ToLower().Contains("warning"))
            {
                e.Graphics.DrawString(listBoxLog.Items[e.Index].ToString(), e.Font, System.Drawing.Brushes.Blue, e.Bounds);
            }
            else
            {
                e.Graphics.DrawString(listBoxLog.Items[e.Index].ToString(), e.Font, System.Drawing.Brushes.Black, e.Bounds);
            }

            if (listBoxMaxWidth < listBoxLog.Items[e.Index].ToString().Length)
            {
                listBoxMaxWidth = listBoxLog.Items[e.Index].ToString().Length;
                {
                    System.Drawing.Graphics g = listBoxLog.CreateGraphics();
                    int hzsize = (int)g.MeasureString(listBoxLog.Items[e.Index].ToString(), listBoxLog.Font).Width;
                    listBoxLog.HorizontalExtent = hzsize;
                }
            }
        }

        private void textBoxSyDB_TextChanged(object sender, EventArgs e)
        {
            Save2Config(this.textBoxSyDB, "BeaconLayout_SYDB");
        }

        private void textBoxOutput_TextChanged(object sender, EventArgs e)
        {
            Save2Config(this.textBoxOutput, "Output");
        }

        private void textBoxBoundaryBeacon_TextChanged(object sender, EventArgs e)
        {
            Save2Config(this.textBoxBoundaryBeacon, "Boundary_beacons");
        }

        private void textBoxLayout_TextChanged(object sender, EventArgs e)
        {
            Save2Config(this.textBoxLayout, "Layout_beacons");
        }
    }
}
