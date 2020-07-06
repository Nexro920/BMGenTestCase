using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using BMGenTool.Info;
using BMGenTool.Common;

using MetaFly.Summer.Generic;
using MetaFly.Summer.IO;
using MetaFly.Serialization;
using MetaFly.Datum.Figure;

namespace BMGenTool.Generate
{
    public class LEUXmlGen : IDataGen
    {
        /// <summary>
        /// LEU Result Filtered Values中LEU列表
        /// </summary>
        private List<LEU_filtered_values.leu> LeuInfoList = new List<LEU_filtered_values.leu>();

        /// <summary>
        /// 模板文件内容
        /// </summary>
        //private LEUGlobal leuGlobal = new LEUGlobal();
        private LEUXML.LEU leuXmlTemplate = null;
        /// <summary>
        /// GID-Table中伪随机数列表
        /// </summary>
        private List<GID> GidInfoList = new List<GID>();

        private string leuComPath;
        private string leuRF;
        private string leuTF;
        private bool isITC;
        private bool isBin;
        private string gidTable;
        private string CIReportTmplt = "";
        private List<LEU> leulist;

        public LEUXmlGen(List<LEU> leus,string leurf, string currentRunDir, bool isITC, bool isBin)
        {
            this.leulist = leus;
            this.leuRF = leurf;
            this.leuTF = currentRunDir + "\\Config\\LEUXMLTemplateExample.xml";
            this.gidTable = currentRunDir + "\\Config\\GID-Table.txt";
            this.leuComPath = currentRunDir + "\\compiler\\CompilerLEUV6001\\main\\compile.exe";
            this.isITC = isITC;
            this.isBin = isBin;
            this.CIReportTmplt = currentRunDir + "\\Config\\CI-LEU一致性测试报告 CI-LEU correspondence test report.xlsx";            
        }

        public override bool Generate(object outputpath)
        {
            if (!Init())
            {
                return false;
            }
            //chapter 5.2
            string path = (string)outputpath + "\\LEUBinary";
            Sys.NewEmptyPath(path);
            

            int i = 0;
            foreach (LEU_filtered_values.leu leu in LeuInfoList)
            {
                string filePath = string.Format("{0}\\{1}", path, leu.NAME);
                Sys.NewEmptyPath(filePath);

                //BMGR-0060
                string filename = string.Format("{0}\\{1}.xml", filePath, leu.NAME);
                //生成每一个LEU的LEU Global.xml文件
                if (!this.GenLEUXmlFile(leu, GidInfoList[i++], filename))
                {
                    TraceMethod.RecordInfo($"Generate {leu.NAME}.xml file error!");
                    return false;
                }

                if (true == isBin && !GenerateBin(filename, filePath, leu.beaconList))
                {
                    TraceMethod.RecordInfo($"call {leuComPath} for {leu.NAME} file error!");
                    return false;
                }

                UpdateProgressBar(70 + LeuInfoList.Count, 70 + i);
            }
            //CI report need compiled msg, so call this after GenerateBin
            //if GenerateBin is not called, the report compiled msg will be empty
            CIReportExcel tt = new CIReportExcel(CIReportTmplt, path, leulist, LeuInfoList, isBin);

            TraceMethod.RecordInfo("[Steps 4-4]:BMGenTool generate LEU files completed! All progress is OK!");
            return true;
        }

        private bool GenerateBin(string filename, string outDir, List<LEU_filtered_values.leu.BEACON> list)
        {
            FileInfo fi = new FileInfo(filename);
            string leuname = fi.Name.Replace(".xml", "");
            Process p = new Process();
            p.StartInfo.FileName = this.leuComPath;
            if (isITC)
            {//BMGR-0075
                p.StartInfo.Arguments = string.Format("{0} -o {1} -udf -tgm -telformat sacem", filename, outDir);
            }
            else
            {//BMGR-0076
                p.StartInfo.Arguments = string.Format("{0} -o {1} -udf -tgm -telformat udf", filename, outDir);
            }
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            TraceMethod.RecordInfo($"Generating bin file of {fi.Name}......");
            p.Start();
            p.WaitForExit();
            p.Close();
            TraceMethod.RecordInfo($"TGM and UDF binary file for {fi.Name} created.");

            List<string> genFiles = new List<String>();
            genFiles.Add($"{outDir}//{leuname}_te1.bin");
            genFiles.Add($"{outDir}//{leuname}_tpc.bin");
            genFiles.Add($"{outDir}//{leuname}_tse.bin");

            foreach (LEU_filtered_values.leu.BEACON b in list )
            {
                int i = b.outNum;
                string tgmname = string.Format("\\telgen_{0}.TGM", i);
                string udfname = string.Format("\\TELGEN_{0}.udf", i);

                genFiles.Add(outDir + tgmname);
                genFiles.Add(outDir + udfname);
            }

            foreach (string newFile in genFiles)
            {
                if (!File.Exists(newFile))
                {
                    TraceMethod.RecordInfo(string.Format("call {0} error.", this.leuComPath), TraceMethod.TraceKind.ERROR);
                    return false;
                }
            }

            TraceMethod.RecordInfo(string.Format("GenerateBin for leu {0} created success.", fi.Name));
            return true;
        }

        private bool Init()
        {
            if (isBin)
            {
                if (!File.Exists(leuComPath))
                {
                    string logMsg = string.Format("[{0}] the compile tool for bin file is not exist, please check!", leuComPath);
                    TraceMethod.RecordInfo(logMsg);
                    return false;
                }
            }

            //read all LEU info from LEU_Result_Filtered_Values.xml
            try
            {
                LEU_filtered_values leurfxml = FileLoader.Load<LEU_filtered_values>(leuRF);
                LeuInfoList = leurfxml.LEU;
                
            }
            catch (System.Exception ex)
            {
                TraceMethod.RecordInfo($"Read LEU file [{leuRF}] error {ex.Message}, please check!");
                return false;
            }

            //read LEU template info for each LEU.xml from LEUXMLTemplateExample.xml
            try
            {
                leuXmlTemplate = FileLoader.Load<LEUXML.LEU>(leuTF);
            }
            catch (System.Exception ex)
            {
                TraceMethod.RecordInfo("Read LEU XML Template file error {0}, please check!", ex.Message);
                return false;
            }

            List<string> gidList = new List<string>();
            string gidRepeatLog = "";
            try
            {
                StreamReader sr = new StreamReader(gidTable);
                string line = null;
                while ((line = sr.ReadLine()) != null && gidList.Count < LeuInfoList.Count * 3)
                {
                    //the used GID table, should not be repeated
                    if (gidList.Contains(line))
                    {
                        gidRepeatLog += line + "   ";
                        continue;
                    }

                    if (line.Length == 16)
                    {
                        //add check of gid data
                    }
                    else if ("" == line)
                    {
                        continue;
                    }
                    else
                    {
                        TraceMethod.RecordInfo($"invalid [{line}] in {gidTable}");
                        return false;
                    }

                    gidList.Add(line);
                }

                sr.Close();
            }
            catch (System.Exception ex)
            {
                TraceMethod.RecordInfo($"Read GID file [{gidTable}] error {ex.Message}, please check!");
                return false;
            }

            //read all the GID data from \\Config\\GID-Table.txt
            
            
            //BMGR-0062
            //伪随机数不够，则返回false
            if (gidList.Count < LeuInfoList.Count * 3)
            {
                if ("" != gidRepeatLog)
                {
                    gidRepeatLog = "Repeated gid: " + gidRepeatLog + " ";
                }
                gidRepeatLog += $" The data in GID-Table.txt has {gidList.Count} unique records, should be {LeuInfoList.Count * 3}!";
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, gidRepeatLog);
                return false;
            }

            for (int i = 0; i < LeuInfoList.Count; ++i)
            {
                GID gid = new GID(gidList[i*3],gidList[(i*3)+1],gidList[(i*3)+2]);
                GidInfoList.Add(gid);
            }
            return true;
        }

        private bool GenLEUXmlFile(LEU_filtered_values.leu leurf, GID gid, string filename)
        {
            try
            {
                XmlFileHelper xmlFile = XmlFileHelper.CreateFromString(null);
                AddLogHead(ref xmlFile);
                
                //根据LEU Result Filtered Values文件的内容修改可变部分的值
                leuXmlTemplate.name = leurf.NAME;

                //5.2.1生成Output_balise
                leuXmlTemplate.Output_balise.Clear();//先清掉已有数据

                //calculate leugb.obList by leurf.beaconList
                #region[cal oblist]
                foreach (LEU_filtered_values.leu.BEACON beacon in leurf.beaconList)
                {
                    LEUXML.LEU.OUTPUT_BALISE ob = new LEUXML.LEU.OUTPUT_BALISE(beacon.NAME, beacon.NUM);

                    string[] varInputs = beacon.Variants_inputs.ToString().Split(' ');
                    int index = 0;
                    //BMGR-0064
                    foreach (string varInput in varInputs)
                    {
                        if (varInput != "0")
                        {
                            LEUXML.LEU.OUTPUT_BALISE.INPUT input = new LEUXML.LEU.OUTPUT_BALISE.INPUT();
                            input.Channel = new StringData("00");
                            input.Number = new StringData(varInput.PadLeft(2, '0'));
                            input.index = index;
                            ob.Input.Add(input);
                        }
                        ++index;//BMGR-0072 this is the pos index in variant_input and varait_state
                    }
                    ob.Input.Sort((x, y) =>
                        {
                            return x.Number.ToString().CompareTo(y.Number.ToString());//根据intNum重新排序，这个排序影响mask的值
                        });

                    foreach (LEU_filtered_values.leu.BEACON.MESSAGE msg in beacon.msgList)
                    {
                        if (0 != msg.RANK)//BMGR-0071
                        {
                            LEUXML.LEU.OUTPUT_BALISE.ASPECT asp = new LEUXML.LEU.OUTPUT_BALISE.ASPECT();
                            string mask = "";
                            //BMGR-0072
                            //根据Input的顺序确定其在MASK中的位置，根据其num找到其在Variants_inputs中的索引
                            //根据其在Variants_inputs中索引，确定此码位在Variant_state中的索引.[两个索引相等]
                            //判断此索引位置的值，为0则取0，为1则取1，为P或S则取X，然后将MASK不足X至30位
                            foreach (var inNode in ob.Input)
                            {
                                string value = msg.VarState.Substring(inNode.index, 1);
                                if (value == "0")
                                {
                                    mask += '0';
                                }
                                else if (value == "1")
                                {
                                    mask += '1';
                                }
                                else if (value == "P" || value == "S")
                                {
                                    mask += 'X';
                                }
                            }
                            //补足30位
                            for (int i = mask.Length; i < 30; i++)
                            {
                                mask += 'X';
                            }
                            asp.Mask = new StringData(mask);
                            //BMGR-0073
                            if (isITC)
                            {
                                throw new NotImplementedException();
                            }
                            else
                            {
                                asp.Telegram = msg.Interoperable;
                            }
                            ob.CheckAspect(asp);
                            ob.Aspect.Add(asp);
                        }
                    }
                    //BMGR-0074
                    LEU_filtered_values.leu.BEACON.MESSAGE dftMsg = beacon.msgList.Find(x => (int)x.RANK == 0);
                    if (isITC)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        ob.Default_telegram = dftMsg.Interoperable;
                    }
                    leuXmlTemplate.Output_balise.Add(ob);
                }
                #endregion

                //GMBR-0060
                leuXmlTemplate.updateGid(gid);

                FileSerializer.Serialize(leuXmlTemplate, filename, AddLogHead());
                
                return true;
            }
            catch (System.Exception ex)
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, ex.Message);
                return false;
            }
        }

    }

    public struct GID
    {
        public string ibGid;
        public string ouGid;
        public string netGid;

        public GID(string input, string output, string net)
        {
            ibGid = input;
            ouGid = output;
            netGid = net;
        }
    }
}
