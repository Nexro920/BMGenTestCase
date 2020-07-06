using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Summer.System.IO;
using BMGenTest;
using BMGenTest.Log;
using BMGenTool.Info;
using BMGenTool.Common;
using System.Diagnostics;
using BMGenTool.Generate;
using System.IO;

namespace BMGenTool
{
    class BMGenToolConfig
    {
        /// <summary>
        /// SYDB数据
        /// </summary>
        XmlVisitor xmlSyDBInfo;

        /// <summary>
        /// Beacon layout XML数据
        /// </summary>
        XmlVisitor xmlBeaconInfo;

        /// <summary>
        /// Block mode variant file数据
        /// </summary>
        XmlVisitor xmlBMVInfo;

        /// <summary>
        /// LEU Result Filtered Value File数据
        /// </summary>
        XmlVisitor xmlLEURFInfo;

        /// <summary>
        /// LEU XML Template File数据
        /// </summary>
        XmlVisitor xmlLEUTFInfo;

        /// <summary>
        /// beacon layout中beacon列表
        /// </summary>
        List<BeaconLayout> BeaconInfoList = new List<BeaconLayout>();

        /// <summary>
        /// block mode variant文件中的beacon列表
        /// </summary>
        List<BMBeacon> BmvInfoList = new List<BMBeacon>();

        /// <summary>
        /// LEU Result Filtered Values中LEU列表
        /// </summary>
        List<LEURF> LeuInfoList = new List<LEURF>();

        /// <summary>
        /// GID-Table中伪随机数列表
        /// </summary>
        List<GID> GidInfoList = new List<GID>();

        /// <summary>
        /// SYDB信息
        /// </summary>
        SyDB sydb = new SyDB();

        //选择生成数据，1为为beacon layout，2、3、4分别为LEU的step1、2、3
        Option option;
        bool isITC;
        //文件中读取的LINE ID
        int lineID;
        string balComPath;
        string leuComPath;
        string leuFilesDir;
        string logMsg = "";

        public delegate void GenProess(int total, int current);
        public delegate void UpdateLog(string msg, LogManager.Level level);
        public event GenProess genPro;
        public event UpdateLog upLog;

        public BMGenToolConfig(Option option, bool isITC)
        {
            this.option = option;
            this.isITC = isITC;
        }

        public bool Init(string file1, string file2)
        {
            if (Option.BEACON == this.option)
            {
                try
                {
                    XmlFileHelper xmlfile = XmlFileHelper.CreateFromFile(file1);
                    XmlVisitor root = xmlfile.GetRoot();
                    xmlBeaconInfo = root.FirstChildByPath("Beacons");
                }
                catch (System.Exception ex)
                {
                    logMsg = "Read beacon layout file error, please check!";
                    WriteLog(logMsg, LogManager.Level.Error);
                    return false;
                }
                this.balComPath = file2;
                return true;
            }
            else if (Option.BMV == this.option)
            {
                XmlFileHelper xmlfile = null;
                try
                {
                    xmlfile = XmlFileHelper.CreateFromFile(file1);
                    XmlVisitor root = xmlfile.GetRoot();
                    xmlBeaconInfo = root.FirstChildByPath("Beacons");
                }
                catch (System.Exception ex)
                {
                    logMsg = "Read beacon layout file error, please check!";
                    WriteLog(logMsg, LogManager.Level.Error);
                    return false;
                }
                try
                {
                    xmlfile = XmlFileHelper.CreateFromFile(file2);
                    xmlSyDBInfo = xmlfile.GetRoot();
                }
                catch (System.Exception ex)
                {
                    logMsg = "Read system database file error, please check!";
                    WriteLog(logMsg, LogManager.Level.Error);
                    return false;
                }                
                return true;
            }
            else if (Option.LEUXML == this.option)
            {
                XmlFileHelper xmlfile = null;
                try
                {
                    xmlfile = XmlFileHelper.CreateFromFile(file1);
                    xmlLEURFInfo = xmlfile.GetRoot();
                }
                catch (System.Exception ex)
                {
                    logMsg = "Read LEU Result Filtered Value file error, please check!";
                    WriteLog(logMsg, LogManager.Level.Error);
                    return false;
                }
                try
                {
                    xmlfile = XmlFileHelper.CreateFromFile(file2);
                    xmlLEUTFInfo = xmlfile.GetRoot();
                }
                catch (System.Exception ex)
                {
                    logMsg = "Read LEU XML Template file error, please check!";
                    WriteLog(logMsg, LogManager.Level.Error);
                    return false;
                }                
                return true;
            }
            else if (Option.LEUBIN == this.option)
            {
                this.leuFilesDir = file1;
                this.leuComPath = file2;
                return true;
            }
            else
            {
                logMsg = "The option does not match the input files, please check!";
                WriteLog(logMsg, LogManager.Level.Error);
                return false;
            }
        }

        public bool Init(string bmvFile)
        {
            if (Option.LEURF == this.option)
            {
                try
                {
                    XmlFileHelper xmlfile = XmlFileHelper.CreateFromFile(bmvFile);
                    xmlBMVInfo = xmlfile.GetRoot();
                }
                catch (System.Exception ex)
                {
                    logMsg = "Read block mode variant file error, please check!";
                    WriteLog(logMsg, LogManager.Level.Error);
                    return false;
                }                
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Configure()
        {
            if (Option.BEACON == option)
            {
                //读取beacon layout文件
                List<XmlVisitor> beaconList = xmlBeaconInfo.Children().ToList();
                if (null != beaconList)
                {
                    foreach (XmlVisitor node in beaconList)
                    {
                        BeaconLayout beacon = new BeaconLayout();
                        if (!beacon.SetBeacon(node))
                        {
                            logMsg = string.Format("Read Beacon.ID = {0}  error, please check!",beacon.ID);
                            WriteLog(logMsg, LogManager.Level.Error);
                            continue;
                        }
                        BeaconInfoList.Add(beacon);
                    }
                }
            }
            else if (Option.BMV == option)
            {
                //读取beacon layout文件
                List<XmlVisitor> beaconList = xmlBeaconInfo.Children().ToList();
                if (null != beaconList)
                {
                    foreach (XmlVisitor node in beaconList)
                    {
                        BeaconLayout beacon = new BeaconLayout();
                        if (!beacon.SetBeacon(node))
                        {
                            logMsg = string.Format("Read Beacon.ID = {0} error in beacon layout file, please check!", beacon.ID);
                            WriteLog(logMsg, LogManager.Level.Error);
                            continue;
                        }
                        BeaconInfoList.Add(beacon);
                    }
                }

                try
                {
                    ReadSyDB readSydb = new ReadSyDB(xmlSyDBInfo);
                    //计算LINE.ID
                    sydb.LineID = readSydb.ReadLineId();
                    //读SYDB的IBBM表
                    sydb.ibbmInfoList = readSydb.ReadIBBM();
                    //读SYDB的route表
                    sydb.routeInfoList = readSydb.ReadRoute();
                    //读取SYDB的signal表
                    sydb.signalInfoList = readSydb.ReadSignal();
                    //读取SYDB的block表
                    sydb.blockInfoList = readSydb.ReadBlock();
                    //读取SYDB的point表
                    sydb.pointInfoList = readSydb.ReadPoint();
                    //读取SYDB的overlap表
                    sydb.overlapInfoList = readSydb.ReadOverlap();
                    //读取SYDB的TFC表
                    sydb.tfcInfoList = readSydb.ReadTFC();
                    //读取SYDB的TFC表
                    sydb.sddbInfoList = readSydb.ReadSDDB();
                }
                catch (System.Exception ex)
                {
                    logMsg = "Read sydb data error, please check!";
                    WriteLog(logMsg, LogManager.Level.Error);
                    return false;
                }                
            }
            else if (Option.LEURF == option)
            {
                //读取BMV文件
                this.lineID = DataOpr.Xmlattr2Int(xmlBMVInfo, "LINE_ID");
                List<XmlVisitor> bmvList = xmlBMVInfo.Children().ToList();
                if (null != bmvList)
                {
                    foreach (XmlVisitor node in bmvList)
                    {
                        BMVBeaconInfo bm = new BMVBeaconInfo(node);
                        BmvInfoList.Add(bm.GetBMBeaconInfo());
                    }
                }
            }
            else if (Option.LEUXML == option)
            {
                this.lineID = DataOpr.Xmlattr2Int(xmlLEURFInfo, "LINE_ID");
                List<XmlVisitor> leuList = xmlLEURFInfo.Children().ToList();
                if (null != leuList)
                {
                    foreach (XmlVisitor node in leuList)
                    {
                        LEURF leu = new LEURF();
                        if (!leu.Read(node))
                        {
                            logMsg = string.Format("Read LEU.ID = {0} error int LEU result filtered file, please check!", leu.leuId);
                            WriteLog(logMsg, LogManager.Level.Error);
                            continue;
                        }
                        LeuInfoList.Add(leu);
                    }
                }
                //根据Config文件夹下GID-Table文件的内容计算伪随机数列表
                string filename = System.IO.Directory.GetCurrentDirectory() + "\\Config\\GID-Table.txt";
                StreamReader sr = new StreamReader(filename);
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    string[] gids = line.Split(',');
                    if (gids.Length < 3)
                    {
                        continue;
                    }
                    GID gid = new GID();
                    gid.ibGid = gids[0];
                    gid.ouGid = gids[1];
                    gid.netGid = gids[2];
                    GidInfoList.Add(gid);
                }
            }
            return true;
        }

        public void Generate(object output)
        {
            string outputpath = (string)output;
            string path = "";
            if (Option.BEACON == option)
            {
                //chapter 3
                path = outputpath + "\\Beacon";
                if (!Directory.Exists(path))
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    di.Create();
                }
                //生成beacon_name.xml文件
                for (int i = 0; i < BeaconInfoList.Count; i++)
                {
                    BeaconLayout beacon = BeaconInfoList[i];
                    logMsg = string.Format("Generating {0}.xml and bin files......", beacon.Name);
                    WriteLog(logMsg, LogManager.Level.Info);
                    //生成beacon_name.xml文件
                    string filename = string.Format("{0}\\{1}.xml", path, beacon.Name);
                    if (!GenBeaconXMLFile(beacon, filename))
                    {
                        logMsg = string.Format("Generate {0}.xml file error!", beacon.Name);
                        WriteLog(logMsg, LogManager.Level.Error);
                        continue;
                    }
                    
                    //调用beacon compiler生成.udf和.tgm
                    Process p = new Process();
                    p.StartInfo.FileName = this.balComPath;
                    if (isITC)
                    {
                        p.StartInfo.Arguments = string.Format("{0} -o {1} -udf -telformat sacem", filename, path);
                    }
                    else
                    {
                        p.StartInfo.Arguments = string.Format("{0} -o {1} -udf -telformat udf", filename, path);
                    }
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.Start();
                    p.WaitForExit();
                    p.Close();

                    //更新进度条状态
                    if (genPro != null)
                    {
                        genPro(BeaconInfoList.Count, i + 1);                        
                    }
                }
            }
            else if (Option.BMV == option)
            {
                //chapter 4
                path = outputpath + "\\BMV";
                if (!Directory.Exists(path))
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    di.Create();
                }
                string filename = string.Format("{0}\\block_mode_variants_file.xml", path);
                if (!GenBMVFile(sydb.ibbmInfoList, filename))
                {
                    logMsg = "Generate block mode variant file error!";
                    WriteLog(logMsg, LogManager.Level.Error);
                    return;
                }
            }
            else if (Option.LEURF == option)
            {
                //chapter 5.1
                path = outputpath + "\\LEU";
                if (!Directory.Exists(path))
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    di.Create();
                }
                string filename = string.Format("{0}\\LEU_Result_Filtered_Values.xml", path);
                if (!GenLEURFFile(this.BmvInfoList, filename))
                {
                    logMsg = "Generate LEU Result Filtered Value file error!";
                    WriteLog(logMsg, LogManager.Level.Error);
                    return;
                }
            }
            else if (Option.LEUXML == option)
            {
                //chapter 5.2
                path = outputpath + "\\LEUBianry";
                if (!Directory.Exists(path))
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    di.Create();
                }
                //读取模板文件
                LEUGlobal leuGlobal = new LEUGlobal();
                if (!leuGlobal.Read(xmlLEUTFInfo))
                {
                    logMsg = "Read LEU Template file error, please check!";
                    WriteLog(logMsg, LogManager.Level.Error);
                    return;
                }
                //伪随机数不够，则返回false
                if (GidInfoList.Count < LeuInfoList.Count)
                {
                    logMsg = "The random data num in GID-Table.txt file is not enough!";
                    WriteLog(logMsg, LogManager.Level.Error);
                    return;
                }
                int i = 0;
                foreach (LEURF leu in LeuInfoList)
                {
                    string filename = string.Format("{0}\\{1}.xml", path, leu.leuName);
                    //生成每一个LEU的LEU Global.xml文件
                    if (!this.GenLEUGlobalFile(leu, leuGlobal, GidInfoList[i++], filename))
                    {
                        logMsg = string.Format("Generate {0}.xml file error!", leu.leuName);
                        WriteLog(logMsg, LogManager.Level.Error);
                        continue;
                    }
                }
            }
            else if (Option.LEUBIN == option)
            {
                //chapter 5.3
                path = outputpath + "\\LEUBianry";
                if (!Directory.Exists(path))
                {
                    DirectoryInfo di = new DirectoryInfo(path);
                    di.Create();
                }
                //调用LEU compiler生成.udf,.tgm和bin文件
                string[] list = Directory.GetFiles(leuFilesDir);
                List<string> files = new List<string>();
                foreach (string name in list)
                {
                    FileInfo fi = new FileInfo(name);
                    if (fi.Extension == ".xml")
                    {
                        files.Add(fi.FullName);
                    }
                }

                foreach (string filename in files)
                {
                    FileInfo fi = new FileInfo(filename);
                    logMsg = string.Format("Generating bin file of {0}......", fi.Name);
                    WriteLog(logMsg, LogManager.Level.Info);

                    Process p = new Process();
                    p.StartInfo.FileName = this.leuComPath;
                    if (isITC)
                    {
                        p.StartInfo.Arguments = string.Format("{0} -o {1} -udf -tgm -telformat sacem", filename, path);
                    }
                    else
                    {
                        p.StartInfo.Arguments = string.Format("{0} -o {1} -udf -tgm -telformat udf", filename, path);
                    }
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.UseShellExecute = false;
                    p.Start();
                    p.WaitForExit();
                    p.Close();
                }
            }
            return;
        }

        private bool GenBeaconXMLFile(BeaconLayout beacon, string filename)
        {
            return true;
        }

        private bool GenBMVFile(List<IBBM> ibbmList, string filename)
        {
            return true;
        }
        private bool GenLEURFFile(List<BMBeacon> bmvList, string filename)
        {
            return true;
        }
        private bool GenLEUGlobalFile(LEURF leurf, LEUGlobal leugb, GID gid, string filename)
        {
            try
            {
                XmlFileHelper xmlFile = XmlFileHelper.CreateFromString(null);
                //根据LEU Result Filtered Values文件的内容修改可变部分的值
                leugb.leuName = leurf.leuName;
                leugb.inBoard.gid = gid.ibGid;
                leugb.outBoard.gid = gid.ouGid;
                //5.2.1生成Output_balise，TBD
                foreach (LEUBeacon beacon in leurf.beaconList)
                {
                    OutBalise ob = new OutBalise();
                    ob.id = beacon.outNum;
                    ob.telegram = "LONG";

                    leugb.obList.Add(ob);
                }
                leugb.encoder.netGid = gid.netGid;

                //根据模板文件格式写入LEU文件
                XmlVisitor leuNode = leugb.Write();
                xmlFile.SetRoot(leuNode);
                xmlFile.Save2File(filename);
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private void WriteLog(string msg, LogManager.Level level)
        {
            if (upLog != null)
            {
                upLog(msg, level);
            }
        }
    }




}
