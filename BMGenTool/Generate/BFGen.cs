using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

using BMGenTool.Info;
using MetaFly.Summer.IO;
using BMGenTool.Common;

using MetaFly.Summer.Generic;
using MetaFly.Serialization;
using MetaFly.Datum.Figure;

namespace BMGenTool.Generate
{
    public class BFGen : IDataGen
    {
        private SyDB sydb = SyDB.GetInstance();

        private string balComPath;
        private bool isITC;
        private bool isBGenBin;

        private string layoutFile;
        private string boundaryFile;
        
        public BFGen(string layoutCsv, string boundaryXml, string compath, bool isITC, bool isGenBin)
        {
            this.balComPath = compath;

            this.isITC = isITC;
            this.isBGenBin = isGenBin;
            boundaryFile = boundaryXml;
            layoutFile = layoutCsv;
        }
        public override bool Generate(object outputpath)
        {
            bool rt = true;
            string logMsg = "";

            string path = (string)outputpath + "\\Beacon";
            Sys.NewEmptyPath(path);

            if (!Init())
            {
                return false;
            }
            
            //generate beacon_name.xml file and refBeaconLst by balise layout file
            int i = 0;
            int count = sydb.GetBeacons().Count();
            foreach (IBeaconInfo beacon in sydb.GetBeacons())
            {
                TraceMethod.RecordInfo($"Creating message file for {beacon.Info}.");

                //BMGR-0001 generate beacon_name.xml
                string filename = string.Format("{0}\\{1}.xml", path, beacon.Name);

                if (!GenBeaconXMLFile(beacon, filename))
                {
                    logMsg = string.Format("Generate {0}.xml file error!", beacon.Name);
                    TraceMethod.RecordInfo(logMsg);
                    rt = false;
                    continue;
                }
                
                if (isBGenBin)
                {
                    if (false == GenBeaconBinFile(path, filename, beacon))
                    {
                        logMsg = string.Format("Generate {0}.tgm and udf file error!", beacon.Name);
                        TraceMethod.RecordInfo(logMsg);
                        rt = false;
                        continue;
                    }
                }
                ++i;
                //更新进度条状态 +30 is for the following other steps
                UpdateProgressBar(count + 30, i);
            }

            if (BMGenTest.Program.GenerateTJFormat)//this output file only for user debug, not record in document
            {
                GenTJFormatFileHead(path);
            }
            
            TraceMethod.RecordInfo("[Steps 4-1]:BMGenTool BFGen end!");
            return rt;
        }
        private bool Init()
        {
            bool rt = false;
            sydb.clear(onlyclearbeacon:true);
            try
            {
                if ("" != boundaryFile && Path.GetExtension(boundaryFile) == ".xml")
                {
                    string xsdfullname = ".//Config//boundarybeacon.xsd";
                    if (File.Exists(xsdfullname) && XsdVerify.Verify(boundaryFile, xsdfullname))
                    {
                        Line_boundary_BM_beacons beacons = FileLoader.Load<Line_boundary_BM_beacons>(boundaryFile);
                        sydb.ReadBoundaryBeacon(beacons);
                        rt = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                TraceMethod.RecordInfo($"Read boundary beacon xml file [{boundaryFile}] error: {ex.Message}, please check!");
            }

            try
            {
                if ("" != layoutFile)
                {
                    if (false == sydb.ReadcsvBeacon(layoutFile))
                    {
                        TraceMethod.RecordInfo($"Read csv file [{layoutFile}] error");
                    }
                    else
                    {
                        rt = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                TraceMethod.RecordInfo($"Read csv file [{layoutFile}] error: {ex.Message}, please check!");
            }
            
            return rt;
        }

        private void GenTJFormatFileHead(string path)
        {
            string logMsg = "";
            XmlVisitor allFileRoot = null;
            string filename = string.Format("{0}\\all_beacons.xml", path);
            XmlFileHelper allxmlFile = XmlFileHelper.CreateFromString(null);

            AddLogHead(ref allxmlFile);

            allxmlFile.SetRoot("Beacons", null);
            allxmlFile.Save2File(filename);
            
            allFileRoot = allxmlFile.GetRoot();
            allFileRoot.UpdateAttribute("NUMBERS", sydb.GetBeacons().Count());

            foreach(IBeaconInfo beacon in sydb.GetBeacons())
            {
                string telValue = "";
                if (isITC)
                {//BMGR-0004
                    TraceMethod.RecordInfo("iTC not support now!");
                    telValue = "iTC not support";
                }
                else
                {
                    BeaconMessage bm = new BeaconMessage();
                    telValue = bm.GenerateMessage(beacon, sydb.LineID);
                }

                byte[] urstel = DataOpr.String2byte(telValue);

                XmlVisitor beaconNode = XmlVisitor.Create("Beacon", null);
                beaconNode.UpdateAttribute("ID", beacon.ID);
                beaconNode.UpdateAttribute("NAME", beacon.Name);                    
                byte[] content = new byte[128];
                bool result = DataOpr.PackCallScram_Tel(urstel, content);
                if (result)
                {
                    string tel1 = DataOpr.Byte2string(content);
                    beaconNode.AppendChild("Telegram0", telValue);
                    beaconNode.AppendChild("Telegram1", tel1);
                }
                else
                {
                    logMsg = string.Format("Encoding Error!");
                    TraceMethod.RecordInfo(logMsg);
                    continue;
                }
                allFileRoot.AppendChild(beaconNode);
            }

            allxmlFile.Save2File(filename);
            logMsg = "Generate basic_beacons.xml file successfully!";
            TraceMethod.RecordInfo(logMsg);
        }

        //in this func use class member BFGen.isITC
        private bool GenBeaconXMLFile(IBeaconInfo beacon, string filename)
        {
            Balise xmlFile = new Balise();

            //BMGR-0002
            xmlFile.name = new StringData(beacon.Name);
            
            //BMGR-0003
            xmlFile.Telegram.type = new StringData("LONG");

            //Telegram get
            if (isITC)
            {//BMGR-0004
                TraceMethod.RecordInfo("error:iTC not support now!");
                xmlFile.Telegram.Value = new StringData("iTC not support");
            }
            else
            {
                BeaconMessage bm = new BeaconMessage();
                xmlFile.Telegram.Value = new StringData(bm.GenerateMessage(beacon, sydb.LineID));
            }

            FileSerializer.Serialize(xmlFile, filename, AddLogHead());
            return true;
        }
        [DllImport("Decode.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Decode(byte[] UserTelegram, byte[] Telegram);
        [MarshalAs(UnmanagedType.LPArray)]
        public static byte[] AfterDecoded;
       
        private bool GenBeaconBinFile(string path, string xmlFile, IBeaconInfo beacon)
        {
            if (!File.Exists(balComPath))
            {
                TraceMethod.RecordInfo($"ERROR:The compile tool [{balComPath}] for bin file is not exist, can't generate *.tgm and *.udf file, please check!");
                return false;
            }
            //调用beacon compiler生成.udf和.tgm
            Process p = new Process();
            p.StartInfo.FileName = this.balComPath;
            if (isITC)
            {//BMGR-0033
                p.StartInfo.Arguments = string.Format("{0} -o {1} -udf -telformat sacem", xmlFile, path);
            }
            else
            {//BMGR-0034
                p.StartInfo.Arguments = string.Format("{0} -o {1} -udf -telformat udf", xmlFile, path);
            }
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            TraceMethod.RecordInfo(string.Format("Converting XML to binary for beacon {0}......", beacon.Name));
            p.Start();
            p.WaitForExit();
            p.Close();
            string fileTGM = xmlFile.Replace(".xml", ".tgm");
            string fileUDF = xmlFile.Replace(".xml", ".udf");
            
            if (File.Exists(fileTGM) && File.Exists(fileUDF))
            {
                TraceMethod.RecordInfo($"TGM and UDF binary file for beacon {beacon.Info} created success.");
                return true;
            }
            else
            {
                TraceMethod.RecordInfo($"call {this.balComPath} error.");
                return false;
            }            
        }

    }
}
