using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BMGenTool.Info;
//using Summer.System.IO;
using MetaFly.Summer.IO;

using BMGenTool.Common;
using System.IO;
//using TOOLCommon.Trace;
using MetaFly.Summer.Generic;
using MetaFly.Datum.Figure;

namespace BMGenTool.Generate
{
    public class BMVFGen : IDataGen
    {
        /// <summary>
        /// SYDB信息
        /// </summary>
        private SyDB sydb = SyDB.GetInstance();

        private bool isITCUpstream;
        private string m_upstreamFile = "";

        private List<BEACON> beaconList;
        private List<LEU> LEUList;

        internal IEnumerable<BEACON> GetValidBeaconList()
        {
            foreach (BEACON b in beaconList)
            {
                yield return b;
            }
        }
        public BMVFGen(bool isITCUpstream, ref List<BEACON> BLst, ref List<LEU> LEULst, string upstreamFile = "")
        {
            this.isITCUpstream = isITCUpstream;

            beaconList = BLst;
            LEUList = LEULst;
            m_upstreamFile = upstreamFile;
        }

        public override bool Generate(object outputpath)
        {
            //chapter 4
            string path = (string)outputpath + "\\BMV";
            Sys.NewEmptyPath(path);

            if (!GenrateDeviceByIBBM())
            {
                TraceMethod.RecordInfo("GenrateDeviceByIBBM error!");
                return false;
            }

            if (!GenerateBeaconData())
            {
                TraceMethod.RecordInfo("GenerateBeaconData error!");
                return false;
            }

            //BMGR-0014 generate block_mode_variants_file for all beacons in a line
            string filename = string.Format("{0}\\block_mode_variants_file.xml", path);
            if (!GenerateBMVFile(filename))
            {
                TraceMethod.RecordInfo("Generate block_mode_variants_file.xml error!");
                return false;
            }

            TraceMethod.RecordInfo("[Steps 4-2]:BMGenTool BMVFGen run completed!");
            return true;
        }

        /// <summary>
        /// in this function will check and load beacon info from sydb.IBBM
        /// all LEU and Beacon from sydb.IBBM will be generate in this function
        /// </summary>
        /// <returns></returns>
        private bool GenrateDeviceByIBBM()
        {
            bool rt = true;
            //BMGR-0016 LEUID start from 1
            int LEUID = 1;
            //clear leu and beacon before add new data to them
            LEUList.Clear();
            beaconList.Clear();

            foreach (GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON ibbm in sydb.ibbmInfoList)//search sydb.ibbm, LEUID should be same with this order
            {
                IBeaconInfo layout = sydb.GetBeacons().Find(x => x.Name == ibbm.Name);

                //judge the beacon first, if this beacon is not exist in Beacons, then ignore this beacon and LEU
                if (null == layout)
                {
                    TraceMethod.Record(TraceMethod.TraceKind.WARNING,$"Beacon[{ibbm.Name}] exist in Implementation_Beacon_Block_Mode but not exist in Beacons.");
                    continue;
                }

                if (false == layout.IsVariantBeacon())
                {
                    TraceMethod.Record(TraceMethod.TraceKind.WARNING, $"Beacon[{ibbm.Name}] exist in Implementation_Beacon_Block_Mode but is Fixed Beacon.");
                    continue;
                }

                int LEUidx = LEUList.FindIndex(x => x.Name == ibbm.LEU.LEU_Name);
                if (-1 == LEUidx)//if the LEU not exist, then create a new one
                {
                    //BMGR-0016 set LEUID for each new LEU in IBBM order
                    LEU newLEU = new LEU(ibbm.LEU.LEU_Name, LEUID, ibbm.CI_Name);
                    ++LEUID;//LEU ID start from 1, add 1 for each diff one

                    LEUList.Add(newLEU);
                    LEUidx = LEUList.Count() - 1;
                }

                { //check LEU.BeaconOutNum
                    if (false == LEUList[LEUidx].AddBeacon(ibbm.LEU.Beacon_Output_number, ibbm.Name))
                    {
                        TraceMethod.Record(TraceMethod.TraceKind.WARNING, $"Beacon {ibbm.Info} will link to LEU {ibbm.LEU.Info} faild.");
                    }
                }

                BEACON newbeacon = new BEACON(layout, ibbm);
                beaconList.Add(newbeacon);
            }

            if (null == beaconList || beaconList.Count() <= 0)
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, "GenrateDeviceByIBBM, after check IBBM and Beacons, no valid beacon can used to generate data");
                return false;
            }

            //check if exist variant beacon which in beacons but not in Implementation_Beacon_Block_Mode
            foreach (IBeaconInfo beacon in sydb.GetBeacons())
            {
                if (true == beacon.IsVariantBeacon())
                {
                    int Bidx = beaconList.FindIndex(x => x.Name == beacon.Name);

                    if (-1 == Bidx)
                    {
                        TraceMethod.Record(TraceMethod.TraceKind.WARNING, $"{beacon.Info} can't get vaild info from sydb.IBBM. BMVF file will generate no info for this beacon");
                        continue;
                    }
                }
            }
            return rt;
        }

        //BMGR-0027
        private bool GenerateVariantList(BEACON beacon)
        {
            beacon.m_variantLst = new List<Variant>();
            foreach (OriginSignal sig in beacon.GetOrgSignalList())
            {
                sig.CalVariants(beacon.m_variantLst);
            }

            foreach (Variant var in beacon.m_variantLst)
            {
                if (var.GetVarSrc() == VAR_TYPE.E_POINT)
                {
                    VariantPoint pvar = (VariantPoint)var;
                    //BMGR-0031 set inputRank from IBBM
                    var.InputRank = beacon.m_ibbmInfo.getInputRank(var.GetName(), pvar.PointVariantPos);
                    pvar.check(beacon.Name);
                }
            }
            //del repeat by object_name
            beacon.m_variantLst = beacon.m_variantLst.Distinct().ToList();

            //set idx
            int idx = 1;
            foreach (Variant var in beacon.m_variantLst)
            {
                var.SetIdx(idx);
                ++idx;
            }

            if (beacon.m_variantLst.Count() > BEACON.MAXVARNUM)
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, $"{beacon.Info} has {beacon.m_variantLst.Count()} variants more than {BEACON.MAXVARNUM}!");
                foreach (Variant var in beacon.m_variantLst)
                {
                    TraceMethod.RecordInfo($"{var.Info}");
                }
            }
            return true;
        }

        private bool GenerateBeaconData()
        {
            bool rt = true;
            //BMGR-0022 
            //read route info from sydb
            //split the routes into RouteSpacing and Spacing routes
            RouteSegConfig SyDBRouteCfg = new RouteSegConfig(sydb);
            SyDBRouteCfg.generateRouteSegments();

            for (int i = 0; i < beaconList.Count(); ++i)
            {
#region [originsignal]

                foreach (GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON.INPUT_SIGNAL inSig in beaconList[i].m_ibbmInfo.Input_Signal)
                {
                    GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL OrgSig = sydb.signalInfoList.Find(x => x.Name == inSig.Name.ToString());

                    if (null == OrgSig)
                    {
                        TraceMethod.RecordInfo($"Warning:{inSig.Info} can't find in sydb.signals, no data will generate for it!");
                        continue;
                    }

                    //create an OriginSignal by IBBM input and the real signal
                    OriginSignal newOrgSig = new OriginSignal(OrgSig, inSig);

                    if (false == newOrgSig.GetRouteSegments(SyDBRouteCfg, beaconList[i].m_ibbmInfo))
                    {
                        TraceMethod.RecordInfo($"Warning:IBBM {inSig.Info} to {beaconList[i].Info} Generate RouteSegments fail!");
                    }

                    if (Sys.Reopening == inSig.Type)
                    {
                        beaconList[i].m_ReopenOrgSig = newOrgSig;
                    }
                    else if (Sys.Approach == inSig.Type)
                    {
                        beaconList[i].m_AppOrgSigLst.Add(newOrgSig);
                    }
                    else
                    {
                        TraceMethod.RecordInfo($"IBBM {inSig.Info} to {beaconList[i].Info} error, unknown type");
                    }
                }
#endregion

                GenerateVariantList(beaconList[i]);
            }

            return rt;
        }

        private bool GenerateBMVFile(string filename)
        {
            XmlFileHelper xmlFile = XmlFileHelper.CreateFromString(null);
            AddLogHead(ref xmlFile);
            xmlFile.SetRoot("Block_mode_beacons", null);

            XmlVisitor root = xmlFile.GetRoot();
            root.UpdateAttribute("LINE_ID", sydb.LineID);//BMGR-0014 log lineID

            foreach (BEACON beacon in GetValidBeaconList())
            {
                XmlVisitor beaconNode = XmlVisitor.Create("Beacon", null);

                if (false == beacon.SetBeaconInfoNode_BMVF(ref beaconNode)
                    || false == GenerateLEUInfoNode(beacon.m_ibbmInfo, ref beaconNode)
                    || false == GenerateBMBSDDBDisInfoNode(beacon, ref beaconNode))
                {
                    //log error
                    TraceMethod.RecordInfo($"Warning:GenerateBMVFile base info[Beacon][LEU][BMBSDDB_Distance] faild for {beacon.Info}.");
                }

                //then all reopen sig and the approach sig
                foreach (OriginSignal orgSig in beacon.GetOrgSignalList())
                {
                    beaconNode.AppendChild(orgSig.GetXmlNode(beacon.Name));
                }

                beaconNode.AppendChild(CalBMVariantNode(beacon));
                root.AppendChild(beaconNode);
            }

            xmlFile.Save2File(filename);
            return true;
        }


        //BMGR-0016 output LEU info
        private bool GenerateLEUInfoNode(GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON ibbm, ref XmlVisitor node)
        {
            if (false == LEUList.Exists(x => x.Name == ibbm.LEU.LEU_Name))
            {
                TraceMethod.RecordInfo($"input LEU[{ibbm.LEU.LEU_Name}] error, not exist in LEU list");
                return false;
            }
            int idx = LEUList.FindIndex(x => x.Name == ibbm.LEU.LEU_Name);
            //LEU
            XmlVisitor leu = XmlVisitor.Create("LEU", null);
            leu.UpdateAttribute("NAME", LEUList[idx].Name);
            leu.AppendChild("Id", LEUList[idx].ID);
            leu.AppendChild("BM_beacon_output_number", ibbm.LEU.Beacon_Output_number);
            node.AppendChild(leu);
            return true;
        }
        
        /// <summary>
        /// get the distance of beacon and nearest sddb in beacon direction
        /// unit: cm( sydb kp data is cm)
        /// </summary>
        /// <param name="beacon"></param>
        /// <returns></returns>
        private int CalBeacon2SDDB(BEACON beacon)
        {
            GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK beaconBlk = SyDB.GetLocatedBlock(beacon.m_layoutInfo.kp, beacon.m_layoutInfo.TrackID);

            if (null == beaconBlk)
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, $"Beacon {beacon.Info} type={beacon.m_ibbmInfo.GetBeaconType()} find beacon located block get null, can't get BMB_SDDB_distance");
                return 0;
            }
            //calculate length from beacon to beacon located Blk end
            string dir = beacon.m_ibbmInfo.Direction;
            int length = Sys.GetSDDBLenOfLocatedBlock(beacon.m_layoutInfo.kp, beaconBlk, dir);

            if (Sys.GetSDDBPosInLocatedBlock(beaconBlk, dir) == Sys.SddbInBlock.end)
            {
                return length;
            }

            //calculate length from beaconBlk end to SDDB //only sddb and beacon in different blk will do this
            int nextBlkID = -1;
            GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK curBlk = beaconBlk;
            while (true)
            {
                if (curBlk.Is_Direction_Opposite.Equals(true))
                {
                    dir = SyDB.GetReverseDir(dir);
                }
                nextBlkID = SyDB.GetNextBlkID(curBlk, dir);
                if (nextBlkID == -1)
                {
                    TraceMethod.RecordInfo($"{curBlk.Info} search next block in {dir} occer convergent point,Error:Beacon {beacon.Info} type={beacon.m_ibbmInfo.GetBeaconType()} can't get BMB_SDDB_distance.");
                    return 0;
                }
                var nextBlk = (GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK)Sys.GetNode(nextBlkID, sydb.blockInfoList.Cast<Node>().ToList());
                if (null == nextBlk)
                {
                    TraceMethod.RecordInfo($"{curBlk.Info} search next block in {dir} get null,Error:Beacon {beacon.Info} type={beacon.m_ibbmInfo.GetBeaconType()} can't get BMB_SDDB_distance");
                    return 0;
                }

                Sys.SddbInBlock sddbInWalk = Sys.SddbWalkThroughBlock(nextBlk, dir);
                if (Sys.SddbInBlock.none != sddbInWalk)
                {
                    if (Sys.SddbInBlock.end == sddbInWalk)
                    {
                        length += nextBlk.GetBlockLen();
                    }
                    break;
                }
                
                length += nextBlk.GetBlockLen();
                curBlk = nextBlk;
            }
            return length;
        }
        //BMGR-0021
        private bool GenerateBMBSDDBDisInfoNode(BEACON beacon, ref XmlVisitor node)
        {
            if (null == beacon)
            {
                return false;
            }
            try
            {
                //boundary beacon then get the value and return.
                node.AppendChild("BMB_SDDB_distance", Sys.Cm2Meter(beacon.m_layoutInfo.BMB_Distance_cm, 3).ToString("0.000"));
                //node.AppendChild("BMB_SDDB_distance", beacon.m_layoutInfo.BMB_Distance_cm);
                beacon.BMB_Distance_cm = beacon.m_layoutInfo.BMB_Distance_cm;
                return true;
            }
            catch(Exception ex)
            {
                //if not boundary beacon then do next
            }
            BEACON reopeningBeacon = null;

            if (beacon.m_ibbmInfo.GetBeaconType() == BeaconType.Approach)
            {
                //find the reopen beacon
                string sigName = beacon.m_ibbmInfo.getLinkedSigName();
                if (BEACON.SignamReBeaconDic.ContainsKey(sigName))
                {
                    string reopenBeaconName = BEACON.SignamReBeaconDic[sigName];
                    reopeningBeacon = beaconList.Find(x => x.Name == reopenBeaconName);
                }
            }
            else
            {
                reopeningBeacon = beacon;
            }
            
            if (null == reopeningBeacon)
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, $"Beacon {beacon.Info} type={beacon.m_ibbmInfo.GetBeaconType()} find reopen Beacon ERROR, can't get BMB_SDDB_distance");
                return false;
            }

            if (reopeningBeacon.BMB_Distance_cm <= 0)
            {
                int length = CalBeacon2SDDB(reopeningBeacon);
                if (length <= 0)
                {
                    TraceMethod.Record(TraceMethod.TraceKind.ERROR, $"Beacon {beacon.Info} type={beacon.m_ibbmInfo.GetBeaconType()} can't get BMB_SDDB_distance");
                    return false;
                }
                beacon.BMB_Distance_cm = length;
            }
            else
            {
                beacon.BMB_Distance_cm = reopeningBeacon.BMB_Distance_cm;
            }

            try
            {
                node.AppendChild("BMB_SDDB_distance", Sys.Cm2Meter(beacon.BMB_Distance_cm, 3).ToString("0.000"));
                //node.AppendChild("BMB_SDDB_distance", beacon.BMB_Distance_cm);
            }
            catch (Exception ex)
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, $"Beacon {beacon.Info} type={beacon.m_ibbmInfo.GetBeaconType()} calculate BMB_SDDB_distance {ex.Message}.");
                return false;
            }
            return true;
        }
        //BMGR-0027
        private XmlVisitor CalBMVariantNode(BEACON beacon)
        {
            XmlVisitor bmNode = XmlVisitor.Create("BM_variants", null);
            foreach (Variant var in beacon.m_variantLst)
            {
                bmNode.AppendChild(var.GetVariantXmlNode());
            }
            return bmNode;
        }
        //BMGR-0027
        //private bool CalBMVariantByUpstream(BEACON beacon, ref int Idx, ref XmlVisitor bmNode)
        //{
        //    List<int> allPointList = new List<int>();//used to check the repeat one
        //    if (null != beacon.m_UpstreamLst)
        //    {
        //        foreach (PathInfo path in beacon.m_UpstreamLst)
        //        {
        //            throw new Exception("CalBMVariantByUpstream error, upstream is not support now");
        //            //List<PointInfo> orderPointList = path.pointList.OrderBy(o => o.Point.ID).ToList();
        //            //foreach (PointInfo point in orderPointList)
        //            //{
        //            //    //if the point has deal
        //            //    if (allPointList.Contains(point.Point.ID))
        //            //    {
        //            //        continue;
        //            //    }
        //            //    //BMGR-0040 2 VARIANT N-first R-second
        //            //    CalBMVariantByPoint(beacon, point.Position, point.Point, PointLocation.UpSteam, ref Idx, ref bmNode);

        //            //    allPointList.Add(point.Point.ID);
        //            //}
        //        }
        //    }

        //    return true;
        //}

        //private bool CalUpstreamInfoNode(BEACON beacon, ref XmlVisitor beaconNode)
        //{
        //    if (null != beacon.m_UpstreamLst)
        //    {
        //        foreach (PathInfo path in beacon.m_UpstreamLst)
        //        {
        //            throw new Exception("CalUpstreamInfoNode error, upstream is not support now");
        //        //        //若不存在道岔，则不生成upstream path
        //        //        if (0 == path.pointList.Count)
        //        //        {
        //        //            continue;
        //        //        }
        //        //        XmlVisitor usPath = XmlVisitor.Create("Upstream_path", null);

        //        //        //BMGR-0043
        //        //        usPath.UpdateAttribute("NAME", path.GetUpstreamPathName());
        //        //        //BMGR-0038
        //        //        foreach (PointInfo point in path.pointList)
        //        //        {
        //        //            usPath.AppendChild(point.GetXmlNode(beacon.m_ibbmInfo.Direction, PointLocation.UpSteam));
        //        //        }
        //        //        beaconNode.AppendChild(usPath);
        //        }
        //    }

        //    return true;
        //}

        ////BMGR-0037
        //private List<PathInfo> CalUpstream(BEACON beacon)
        //{
        //    throw new Exception("CalUpstream error, upstream is not support now");
        //    //List<PathInfo> upPathList = new List<PathInfo>();

        //    //int maxTL = this.GetMaxTrainLen(sydb);

        //    ////??
        //    //UpstreamPointConfig upConfig = new UpstreamPointConfig(beacon, beacon.m_ibbmInfo.Direction, maxTL, sydb);

        //    //if (false == File.Exists(m_upstreamFile))
        //    //{
        //    //    TraceMethod.RecordInfo(string.Format("Error: line_boundary_BM_beacons[{0}] file is not exist. All upstream info will get from sydb", m_upstreamFile));
        //    //    upPathList = upConfig.GetPathList();
        //    //}
        //    //else
        //    //{
        //    //    upPathList = upConfig.GetPathList(m_upstreamFile);
        //    //}

        //    //return upPathList;
        //}

        //BMGR-0037 MAX_TRAIN_LENGTH
        //private int GetMaxTrainLen(SyDB sydb)
        //{
        //    int len = 0;
        //    foreach (TFC tfc in sydb.tfcInfoList)
        //    {
        //        if (tfc.FormationLen > len)
        //        {
        //            len = tfc.FormationLen;
        //        }
        //    }
        //    return len;
        //}

    }
}
