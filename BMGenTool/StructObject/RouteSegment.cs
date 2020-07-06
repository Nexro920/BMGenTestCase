using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Summer.System.IO;
using MetaFly.Summer.IO;
using MetaFly.Summer.Generic;


namespace BMGenTool.Info
{
    public class RouteSegment
    {
        //表明此RouteSegment是由哪个route转化来的
        private GENERIC_SYSTEM_PARAMETERS.ROUTES.ROUTE RouteInfo;

        public GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL m_OrgSig
        {
            get
            {
                return OrgSig;
            }
        }

        private GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL OrgSig;

        private GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL DstSig;

        public List<GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK> m_BlkLst;
        public List<PointInfo> m_PtLst;

        public ObjOverlap m_overlap;

        public string GetDstSignalName()
        {
            return NodeApi.getNameNullSafe(DstSig);
        }
        //BMGR-0027
        public bool CalVariants(List<Variant> vList, bool includeOverlap)
        {
            foreach (PointInfo pt in m_PtLst)
            {
                pt.CalVariants(vList);
            }
            if (null != m_overlap && includeOverlap)
            {
                m_overlap.CalVariants(vList);
            }
            return true;
        }

        public RouteSegment DeepClone()
        {
            RouteSegment newR = new RouteSegment(this.OrgSig.ID, this.DstSig.ID, this.m_BlkLst, this.RouteInfo);
            return newR;
        }

        public string Info
        {
            get
            {
                return $"OrgSig[{OrgSig.Info}] DstSig[{DstSig.Info}] from SyDBRoute[{RouteInfo.Info}]";
            }
        }

        private void SetRouteSegment(int orgID, int dstID, GENERIC_SYSTEM_PARAMETERS.ROUTES.ROUTE route)
        {
            this.RouteInfo = route;
            GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL orgSig = SyDB.GetInstance().signalInfoList.Find(x => x.ID == orgID);
            GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL dstSig = SyDB.GetInstance().signalInfoList.Find(x => x.ID == dstID);

            if(null == orgSig || null == dstSig)
            {
                throw new Exception($"sydb error:{route.Info} create RouteSegment error. can't find orgsig[{orgID}] or dstsig[{dstID}] in sydb.Signal");
            }

            OrgSig = orgSig;
            DstSig = dstSig;

            if (0 == m_BlkLst.Count())
            {
                throw new Exception($"sydb error:{Info} error. has no block in route");
            }
            m_PtLst = new List<PointInfo>();
            GetPointLstFromBlkLst();
        }
        public RouteSegment(GENERIC_SYSTEM_PARAMETERS.ROUTES.ROUTE route)
        {
            m_BlkLst = new List<GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK>();
            foreach (int blkID in route.Block_ID_List.Block_ID)
            {
                var blk = (GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK)Sys.GetNode(blkID, SyDB.GetInstance().blockInfoList.Cast<Node>().ToList());
                m_BlkLst.Add(blk);
            }

            SetRouteSegment(route.Origin_Signal_ID, route.Destination_Signal_ID, route);
        }

        public RouteSegment(int orgID, int dstID, List<GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK> blkList, GENERIC_SYSTEM_PARAMETERS.ROUTES.ROUTE route)
        {
            m_BlkLst = new List<GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK>();
            m_BlkLst.AddRange(blkList);
            SetRouteSegment(orgID, dstID, route);
        }

        public bool CalOverlap(GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON ibbm)
        {
            //BMGR-0026
            if (null != DstSig.Overlap_ID)//get overlap
            {
                var ol = (GENERIC_SYSTEM_PARAMETERS.OVERLAPS.OVERLAP)Sys.GetNode((int)DstSig.Overlap_ID, SyDB.GetInstance().overlapInfoList.Cast<Node>().ToList());
                m_overlap = new ObjOverlap(ol, DstSig);
                //BMGR-0067
                if (false == m_overlap.GeneratePath(ibbm))
                {
                    TraceMethod.RecordInfo($"RouteSegment {Info} generate overlap path error!");
                    return false;
                }
            }
            else
            {
                m_overlap = null;
            }
            return true;
        }

        //BMGR-0024
        //the blockList is in order of direction
        public bool GetPointLstFromBlkLst()
        {
            m_PtLst.Clear();
            if (0 == m_BlkLst.Count)
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, $"route[{Info}] include no block, original route[{RouteInfo.Info}]\n");
                return false;
            }

            for (int i = 0; i < m_BlkLst.Count - 1; ++i)
            {
                GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK preBlock = m_BlkLst[i];
                GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK nextBlock = m_BlkLst[i + 1];
                if (preBlock.Point_ID == nextBlock.Point_ID && null != preBlock.Point_ID)
                {//BMGR-0025 cal point info by block link 
                    var point = (GENERIC_SYSTEM_PARAMETERS.POINTS.POINT)Sys.GetNode((int)preBlock.Point_ID, SyDB.GetInstance().pointInfoList.Cast<Node>().ToList());

                    PointInfo objpt = new PointInfo(point,
                        SyDB.GetPosByBlocks(preBlock, nextBlock),
                        SyDB.GetOrientByBlocks(preBlock, nextBlock),
                        PointLocation.Route,
                        this.RouteInfo);

                    m_PtLst.Add(objpt);
                }
            }
            if (10 < m_PtLst.Count())
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, $"points in route[{Info}] is more than 10.\n");
                foreach (PointInfo pt in m_PtLst)
                {
                    TraceMethod.RecordInfo(pt.Info);
                }
                return false;
            }
            return true;
        }

        public XmlVisitor GetDstSigXmlNode(bool bOverLap)
        {
            XmlVisitor dstSig = XmlVisitor.Create("Destination_signal", null);
            { //BMGR-0036
                dstSig.UpdateAttribute("NAME", GetDstSignalName());
                dstSig.AppendChild("Id", NodeApi.getIDNullSafe(DstSig));
            }

            //BMGR-0026
            if (bOverLap && null != m_overlap)
            {
                XmlVisitor overlop = XmlVisitor.Create("Overlap", null);
                overlop = m_overlap.GetXmlNode();
                dstSig.AppendChild(overlop);
            }

            return dstSig;
        }

        //BMGR-0023
        public string GetName()
        {
            string name = "";

            name = string.Format("{0}", NodeApi.getNameNullSafe(OrgSig));
            foreach (PointInfo pt in m_PtLst)
            {
                name += "|" + pt.getNamePosStr();
            }
            name += "|" + GetDstSignalName();
            
            return name;
        }

        /// <summary>
        /// route_segment length, based on blk_list, check sddb
        /// </summary>//0035
        /// <param name="blockList"></param>
        /// <returns></returns>
        /// //BMGR-0035
        public int GetLength()
        {
            int len = 0;
            int startBIdx = 0;
            for (int i = 0; i < m_BlkLst.Count; ++i)
            {
                if (OrgSig.SDDId == m_BlkLst[i].Secondary_Detection_Device_ID
                    && 0 != m_BlkLst[i].Secondary_Detection_Device_ID)
                {
                    startBIdx = i;
                    break;
                }
            }

            int endBIdx = m_BlkLst.Count - 1;
            
            if (m_BlkLst.Count() > 1)
            {
                //find signal located block
                GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK blk = SyDB.GetLocatedBlock(DstSig.Kp, DstSig.Track_ID);
                if (blk == null)
                {
                    TraceMethod.Record(TraceMethod.TraceKind.ERROR, $"dstSignal{DstSig.Info} can't find located block");
                    return -1;
                }
                if (SyDB.IsLocatedOnBlockBeginOrEnd(DstSig.Kp, DstSig.Track_ID, blk))
                {
                    if (m_BlkLst.Exists(s => s.ID == blk.ID) == false)
                    {
                        //the last one is the endBIdx, so donothing
                    }
                    else
                    {
                        endBIdx = m_BlkLst.FindIndex(s => s.ID == blk.ID);
                        var preBlk = (GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK)Sys.GetNode((int)(m_BlkLst[endBIdx-1].ID), SyDB.GetInstance().blockInfoList.Cast<Node>().ToList());
                        if (SyDB.IsLocatedOnBlockBeginOrEnd(DstSig.Kp, DstSig.Track_ID, preBlk))
                        {
                            endBIdx = endBIdx - 1;//route length end with preblk
                        }
                        else
                        {
                            //the located one is the endBIdx, so donothing
                        }
                    }
                }
                else
                {//the local block is the end one
                    if (m_BlkLst.Exists(s => s.ID == blk.ID) == false)
                    {
                        m_BlkLst.Add(blk);
                        ++endBIdx;
                    }
                    else
                    {
                        endBIdx = m_BlkLst.FindIndex(s => s.ID == blk.ID);
                    }
                }
            }

            string log = "";
            for (int i = startBIdx; i <= endBIdx; ++i)
            {
                len += m_BlkLst[i].GetBlockLen();
                log += "[" + m_BlkLst[i].Info + "]";
            }
            //TraceMethod.RecordInfo($"RouteSegment {Info} has blocks {log}");

            return len;
        }

    }

}
