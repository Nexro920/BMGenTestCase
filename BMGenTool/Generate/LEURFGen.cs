using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BMGenTool.Info;
//using Summer.System.IO;
using MetaFly.Summer.IO;
using BMGenTool.Common;
using System.IO;
using System.Runtime.InteropServices;
//using TOOLCommon.Trace;
using MetaFly.Summer.Generic;
using MetaFly.Serialization;
using MetaFly.Datum.Figure;

namespace BMGenTool.Generate
{
    public class LEURFGen : IDataGen
    {
        private List<BEACON> m_BeaconLst;
        private List<LEU> m_LEULst;

        /// <summary>
        /// SYDB信息
        /// </summary>
        private SyDB sydb = SyDB.GetInstance();

        [DllImport("Config\\Scram_Tel.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        extern static public bool ScrambleTel(byte[] usrTel, byte[] tel);

        public LEURFGen(ref List<BEACON> blist, ref List<LEU> leulist)
        {
            m_BeaconLst = blist;
            m_LEULst = leulist;
        }

        public override bool Generate(object outputpath)
        {
            string path = (string)outputpath + "\\LEU";
            Sys.NewEmptyPath(path);

            string filename = string.Format("{0}\\LEU_Result_Filtered_Values.xml", path);
            bool isGen = GenerateLEUResultFilterFile(filename);
            if (!isGen)
            {
                TraceMethod.RecordInfo("Generate LEU Result Filtered Value file error!");
                return false;
            }
            #region [THFDebug]
            if (BMGenTest.Program.GenerateTJFormat)
            {
                if (GenBasciXml(filename, path))
                {
                    TraceMethod.RecordInfo("Generate basic_beacons.xml file successfully!");
                }
                else
                {
                    TraceMethod.RecordInfo("Generate basic_beacons.xml file error!");
                }
            }
            #endregion
            TraceMethod.RecordInfo("[Steps 4-3]:BMGenTool Generate LEU Result Filtered Value file run completed!");
            return true;
        }

        //BMGR-0045
        private bool GenerateLEUInfoNode(LEU leuInfo, ref XmlVisitor leu)
        {
            leu.UpdateAttribute("ID", leuInfo.ID);
            leu.UpdateAttribute("NAME", leuInfo.Name);
            return true;
        }

        //BMGR-0046
        private bool GenerateBeaconInfoNode(BEACON beacon, ref XmlVisitor node, int outnum)
        {
            if (false == beacon.SetBeaconInfoNode_LEURF(ref node, outnum))
            {
                TraceMethod.RecordInfo($"Write SetBeaconInfoNode_LEURF for {beacon.Info} error!");
                return false;
            }
            //BMGR-0047
            node.AppendChild("Variants_inputs", beacon.GetVariantsInputs());

            return true;
        }

        //BMGR-0050 //BMGR-0049
        /// <summary>
        /// generate all message of section info for one beacon, not consider upstream
        /// </summary>
        /// <param name="beacon"></param>
        /// <returns></returns>
        private List<Message> GenerateMessage(BEACON beacon)
        {
            //the order in msgLst should not be changed
            List<Message> msgLst = new List<Message>();
            int msgRankIdx = 0;

            {//BMGR-0050 generate leu default message rank = 0
                Message msg_leudefault = new Message(msgRankIdx++);
                msgLst.Add(msg_leudefault);
            }

            {//rank = 1
                Message msgRed = new Message(msgRankIdx++);
                msgLst.Add(msgRed);
            }

            Message msgBase = new Message();

            if (BeaconType.Approach == beacon.m_ibbmInfo.GetBeaconType())
            {
                #region[only approach]
                //BMGR-0078 only app 
                foreach (OriginSignal appOrg in beacon.m_AppOrgSigLst)
                {
                    foreach (RouteSegment appRS in appOrg.RsList)
                    {
                        Message msgApp = new Message(msgRankIdx++, msgBase);
                        msgApp.apRs = appRS;//BMGR-0079 has app then no overlap

                        msgLst.Add(msgApp);
                    }
                }
                #endregion
            }
            else if (BeaconType.Reopening == beacon.m_ibbmInfo.GetBeaconType())
            {
                #region[only reopen]
                if (null != beacon.m_ReopenOrgSig)
                {
                    //BMGR-0079
                    foreach (RouteSegment rs in beacon.m_ReopenOrgSig.RsList)
                    {
                        msgBase.rpRs = rs;//msg add reopen rs

                        //BMGR-0077 if RSroute has overlap, then generate rs + ol
                        if (null != rs.m_overlap)
                        {
                            if (null != rs.m_overlap.m_pathLst && 0 < rs.m_overlap.m_pathLst.Count())
                            {//reopen rs + overlap path 
                                foreach (PathInfo path in rs.m_overlap.m_pathLst)
                                {
                                    Message msgolp = new Message(msgRankIdx++, msgBase);
                                    msgolp.olPath = path;
                                    msgolp.overlap = rs.m_overlap;

                                    msgLst.Add(msgolp);
                                }
                            }
                            else
                            {//reopen rs + overlap with no path
                                Message msgol = new Message(msgRankIdx++, msgBase);
                                msgol.overlap = rs.m_overlap;

                                msgLst.Add(msgol);
                            }
                        }
                        else//BMGR-0077 if RSroute has no overlap and no appRoute, then generate rs
                        {
                            Message msgrs = new Message(msgRankIdx++, msgBase);
                            msgLst.Add(msgrs);
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region[reopen and approach]
                if (null != beacon.m_ReopenOrgSig)
                {
                    foreach (RouteSegment rs in beacon.m_ReopenOrgSig.RsList)
                    {
                        msgBase.rpRs = rs;//msg add reopen rs

                        //BMGR-0077 if RSroute has overlap, then generate rs + ol
                        if (null != rs.m_overlap)
                        {
                            if (null != rs.m_overlap.m_pathLst && 0 < rs.m_overlap.m_pathLst.Count())
                            {//reopen rs + overlap path 
                                foreach (PathInfo path in rs.m_overlap.m_pathLst)
                                {
                                    Message msgolp = new Message(msgRankIdx++, msgBase);
                                    msgolp.olPath = path;
                                    msgolp.overlap = rs.m_overlap;

                                    msgLst.Add(msgolp);
                                }
                            }
                            else
                            {//reopen rs + overlap with no path
                                Message msgol = new Message(msgRankIdx++, msgBase);
                                msgol.overlap = rs.m_overlap;

                                msgLst.Add(msgol);
                            }
                        }
                        else//BMGR-0077 if RSroute has no overlap, then generate rs
                        {
                            Message msgrs = new Message(msgRankIdx++, msgBase);
                            msgLst.Add(msgrs);
                        }

                        //BMGR-0078 rs+ol first now rs+app rs.dest sig == app.org sig
                        OriginSignal appOrg = beacon.GetOriginSignalBySignalName(rs.GetDstSignalName());
                        if (null != appOrg && null != appOrg.RsList)
                        {
                            foreach (RouteSegment appRS in appOrg.RsList)
                            {
                                Message msgApp = new Message(msgRankIdx++, msgBase);
                                msgApp.apRs = appRS;

                                msgLst.Add(msgApp);
                            }
                        }
                    }
                }
                #endregion
            }

            if (msgRankIdx != msgLst.Count())
            {
                TraceMethod.RecordInfo(string.Format("Beacon[{0}] GenerateMessage run error msgRankIdx[{1}] != msgLst.Count[{2}]",
                            beacon.Name,
                            msgRankIdx,
                            msgLst.Count()));
                return null;
            }
            
            return msgLst;
        }

        //BMGR-0051. the if should obey the order from 0051
        //msg has but open =1
        //msg has but close =0
        //msg not has but linked signal =0
        //msg not has but input rank = S P
        //msg not has and not input rank = 0
        private string GetMsgVar(Message msg, BEACON beacon)
        {
            int rank = msg.GetRank();
            if (-1 == rank)
            {
                TraceMethod.RecordInfo(string.Format("Beacon[{0}] GetMsgVar run error msg rank is -1",
                    beacon.Name));
                return "";
            }
            if (0 == rank)
            {
                return "".PadLeft(BEACON.MAXVARNUM, '0');
            }

            //string[] strVar = new string[BEACON.MAXVARNUM] { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
            string[] strVar = new string[BEACON.MAXVARNUM];
            for (int i = 0; i < strVar.Count(); i++)
            {
                strVar[i] = "0";
            }

            foreach (Variant var in beacon.m_variantLst)
            {
                if (var.m_Idx > BEACON.MAXVARNUM)
                {
                    continue;
                }
                strVar[var.m_Idx - 1] = msg.GetVariantState(var).ToString();

                if ("-1" == strVar[var.m_Idx - 1])
                {
                    if (beacon.GetLindedSignalName() == var.GetName())
                    {
                        strVar[var.m_Idx - 1] = "0";
                    }
                    else if (-1 != var.InputRank)
                    {//signal or point has input rank and not exist in message combine section
                        if (VAR_TYPE.E_SIGNAL == var.GetVarSrc())
                        {
                            strVar[var.m_Idx - 1] = "S";
                        }
                        else
                        {
                            strVar[var.m_Idx - 1] = "P";
                        }
                    }
                    else//else is set 0
                    {
                        strVar[var.m_Idx - 1] = "0";
                    }
                }
            }

            string varState = "";
            for (int i = 0; i < strVar.Count(); i++)
            {
                varState += strVar[i];
            }

            return varState;
        }

        private bool GenerateLEUResultFilterFile(string filename)
        {
            XmlFileHelper xmlFile = XmlFileHelper.CreateFromString(null);
            AddLogHead(ref xmlFile);

            //BMGR-0044
            xmlFile.SetRoot("LEU_filtered_values", null);
            xmlFile.Save2File(filename);

            XmlVisitor root = xmlFile.GetRoot();
            //BMGR-0044
            root.UpdateAttribute("LINE_ID", sydb.LineID);

            //BMGR-0045
            List<LEU> orderLEULst = m_LEULst.OrderBy(o => o.ID).ToList();
            foreach (LEU leu in orderLEULst)
            {
                XmlVisitor LEUnode = XmlVisitor.Create("LEU", null);

                //set LEU info
                if (false == GenerateLEUInfoNode(leu, ref LEUnode))
                {
                    return false;
                }

                string[] bNames = leu.GetBeaconNames();
                //BMGR-0046
                for (int i = 0; i < bNames.Count(); ++i)
                {
                    string bName = bNames[i];
                    if ("" == bName)
                    {
                        continue;
                    }
                    int Bidx = m_BeaconLst.FindIndex(x => (x.Name == bName));
                    if (-1 == Bidx)
                    {
                        TraceMethod.RecordInfo($"Warning:LEU[{leu.Name}] contain beacon[{bName}] which not in valid beacon List, this beacon will be ignore!");
                        continue;
                    }

                    XmlVisitor beaconnode = XmlVisitor.Create("Beacon", null);
                    if (false == GenerateBeaconInfoNode(m_BeaconLst[Bidx], ref beaconnode, i + 1))
                    {
                        //
                    }

                    m_BeaconLst[Bidx].m_MsgLst = GenerateMessage(m_BeaconLst[Bidx]);

                    if (m_BeaconLst[Bidx].m_MsgLst.Count() > 128)
                    {//BMGR-0050
                        TraceMethod.RecordInfo($"LEU[{leu.Info} {m_BeaconLst[Bidx].Info}] generate messages {m_BeaconLst[Bidx].m_MsgLst.Count()} > 128");
                    }

                    foreach (Message msg in m_BeaconLst[Bidx].m_MsgLst)
                    {
                        //mes and combine_sections
                        XmlVisitor msgNode = msg.GetXmlNode();

                        //BMGR-0051 Variant_state
                        string varstate = GetMsgVar(msg, m_BeaconLst[Bidx]);
                        if (BEACON.MAXVARNUM != varstate.Length)
                        {
                            TraceMethod.RecordInfo($"Beacon[{m_BeaconLst[Bidx].Name}] Variant_state[{varstate}] length != {BEACON.MAXVARNUM}");
                        }
                        msgNode.AppendChild("Variant_state", varstate);

                        string msgBuff = "";
                        //Urbalis_iTC
                        if (0 == msg.GetRank())
                        {//BMGR-0004 leu default msg for input beacon
                            varstate = "";
                        }

                        //don't output Urbalis_iTC for BMGen tool this time
#if itcsupport
                        msgBuff = m_BeaconLst[Bidx].GenMsgItc(varstate);
                        if(86 != msgBuff.Length )
                        {
                            TraceMethod.RecordInfo(string.Format("Beacon[{0}] Urbalis_iTC message length != 16",
                                        m_BeaconLst[Bidx].Name));
                            return false;
                        }
                        
                        msgNode.AppendChild("Urbalis_iTC", msgBuff);
#endif
                        //Interoperable
                        BeaconMessage bm = new BeaconMessage();
                        msgBuff = bm.GenerateMessage(m_BeaconLst[Bidx], sydb.LineID, msg);

                        if (311 != msgBuff.Length)
                        {
                            TraceMethod.RecordInfo($"{m_BeaconLst[Bidx].Info} Interoperable message length != 311");
                        }
                        msgNode.AppendChild("Interoperable", msgBuff);

                        beaconnode.AppendChild(msgNode);
                    }

                    LEUnode.AppendChild(beaconnode);
                }

                root.AppendChild(LEUnode);
            }
            xmlFile.Save2File(filename);
            return true;
        }

        //for tj format
        private bool GenBasciXml(string leuFile, string outputPath)
        {
            LEU_filtered_values leurfxml = FileLoader.Load<LEU_filtered_values>(leuFile);

            List<BasicBeacon> BeaconInfoList = new List<BasicBeacon>();
            foreach (LEU_filtered_values.leu leurf in leurfxml.LEU)
            {
                foreach (LEU_filtered_values.leu.BEACON leuBeacon in leurf.Beacon)
                {
                    BasicBeacon baBeacon = new BasicBeacon(leuBeacon);
                    var signal = (GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL)Sys.GetNode((string)leuBeacon.LINKED_SIGNAL, sydb.signalInfoList.Cast<Node>().ToList());
                    if (null == signal)
                    {
                        continue;
                    }
                    baBeacon.SignalId = signal.ID;
                    baBeacon.SignalName = signal.Name;
                    baBeacon.MsgList = new List<MsgRank>();
                    foreach (LEU_filtered_values.leu.BEACON.MESSAGE msg in leuBeacon.msgList)
                    {
                        MsgRank msgRk = new MsgRank();
                        msgRk.routeInfo = new List<string>();
                        //按照Upstream_section，Reopening_section，Approach_section，Overlap_section的顺序将进路上的道岔和信号机依次取出
                        //先算道岔，再算信号机
                        List<string> ptList = new List<string>();
                        List<string> sigList = new List<string>();
                        if (null != msg.Combined_sections)
                        {
                            JudgeSection(msg.Combined_sections.Upstream_section, ptList, sigList);
                            JudgeSection(msg.Combined_sections.Reopening_section, ptList, sigList);
                            JudgeSection(msg.Combined_sections.Approach_section, ptList, sigList);
                            JudgeSection(msg.Combined_sections.Overlap_section, ptList, sigList);
                        }

                        msgRk.routeInfo.AddRange(ptList);
                        msgRk.routeInfo.AddRange(sigList);

                        //计算tel0和tel1
                        msgRk.Tel0 = msg.Interoperable;
                        byte[] content = new byte[128];
                        byte[] telValue = DataOpr.String2byte(msgRk.Tel0);
                        bool result = ScrambleTel(telValue, content);
                        if (result)
                        {
                            msgRk.Tel1 = DataOpr.Byte2string(content);
                        }
                        else
                        {
                            TraceMethod.RecordInfo("Encoding Error!");
                            continue;
                        }
                        baBeacon.MsgList.Add(msgRk);
                    }
                    BeaconInfoList.Add(baBeacon);
                }
            }

            //写入可变报文配置文件
            string allFileName = string.Format("{0}\\basic_beacons.xml", outputPath);
            XmlFileHelper allxmlFile = XmlFileHelper.CreateFromString(null);
            AddLogHead(ref allxmlFile);
            allxmlFile.SetRoot("Beacons", null);
            allxmlFile.Save2File(allFileName);

            XmlVisitor allFileRoot = allxmlFile.GetRoot();
            allFileRoot.UpdateAttribute("NUMBERS", BeaconInfoList.Count);

            foreach (BasicBeacon basBeacon in BeaconInfoList)
            {
                XmlVisitor beaconNode = XmlVisitor.Create("Beacon", null);
                beaconNode.UpdateAttribute("ID", basBeacon.ID);
                beaconNode.UpdateAttribute("NAME", basBeacon.Name);
                beaconNode.UpdateAttribute("RANKS", basBeacon.MsgList.Count());
                beaconNode.UpdateAttribute("TYPE", basBeacon.Type);
                beaconNode.UpdateAttribute("LINKED_SIGNALID", basBeacon.SignalId);
                beaconNode.UpdateAttribute("LINKED_SIGNALName", basBeacon.SignalName);
                for (int i = 0; i < basBeacon.MsgList.Count(); i++)
                {
                    XmlVisitor rankNode = XmlVisitor.Create("Message", null);
                    rankNode.UpdateAttribute("Rank", i);
                    string route = "";
                    foreach (string info in basBeacon.MsgList[i].routeInfo)
                    {
                        route += info + "|";
                    }
                    if (route.EndsWith('|'.ToString()))
                    {
                        route = route.Remove(route.LastIndexOf('|'));
                    }
                    if ("" != route)
                    {
                        rankNode.AppendChild("Route", route);
                    }
                    rankNode.AppendChild("Telegram0", basBeacon.MsgList[i].Tel0);
                    rankNode.AppendChild("Telegram1", basBeacon.MsgList[i].Tel1);
                    beaconNode.AppendChild(rankNode);
                }
                allFileRoot.AppendChild(beaconNode);
            }
            allxmlFile.Save2File(allFileName);
            return true;
        }

        private void JudgeSection(StringData orgsection, List<string> ptList, List<string> sigList)
        {
            if (null == orgsection)
            {
                return;
            }
            string section = orgsection.ToString();
            if (!string.IsNullOrEmpty(section))
            {
                List<string> splits = section.Split('|').ToList();
                foreach (string node in splits)
                {
                    //道岔定位
                    if (node.EndsWith("-N"))
                    {
                        string name = node.Substring(0, node.IndexOf("-N"));
                        GENERIC_SYSTEM_PARAMETERS.POINTS.POINT point = (GENERIC_SYSTEM_PARAMETERS.POINTS.POINT)Sys.GetNode(name, sydb.pointInfoList.Cast<Node>().ToList());
                        if (null != point)
                        {
                            string idName = point.ID + "-N";
                            if (!ptList.Contains(idName))
                            {
                                ptList.Add(idName);
                            }
                        }
                    }
                    else if (node.EndsWith("-R"))//道岔范围
                    {
                        string name = node.Substring(0, node.IndexOf("-R"));
                        GENERIC_SYSTEM_PARAMETERS.POINTS.POINT point = (GENERIC_SYSTEM_PARAMETERS.POINTS.POINT)Sys.GetNode(name, sydb.pointInfoList.Cast<Node>().ToList());
                        if (null != point)
                        {
                            string idName = point.ID + "-R";
                            if (!ptList.Contains(idName))
                            {
                                ptList.Add(idName);
                            }
                        }
                    }
                    else
                    {
                        GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL signal = sydb.signalInfoList.Find(x => x.Name == node);
                        if (null != signal)
                        {
                            if (!sigList.Contains(signal.ID.ToString()))
                            {
                                sigList.Add(signal.ID.ToString());
                            }
                        }
                    }
                }
            }
        }
        //for tj format
    }

    public class BasicBeacon
    {
        public BasicBeacon(LEU_filtered_values.leu.BEACON b)
        {
            ID = b.ID;
            Name = b.NAME;
            Type = b.TYPE;
        }
        public int ID;
        public string Name;
        public string Type;
        public int SignalId;
        public string SignalName;
        public List<MsgRank> MsgList;
    }
    public class MsgRank
    {
        public string Tel0;
        public string Tel1;
        public List<string> routeInfo;
    }
}
