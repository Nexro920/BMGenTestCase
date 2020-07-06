using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BMGenTool.Info;
using System.Collections;
//using Summer.System.IO;
using MetaFly.Summer.IO;
//using TOOLCommon.Trace;
using MetaFly.Summer.Generic;

namespace BMGenTool.Info
{
    public class ObjOverlap
    {
        private GENERIC_SYSTEM_PARAMETERS.OVERLAPS.OVERLAP m_Ol;
        private GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL m_sig;

        private string m_switchPos;

        public List<PathInfo> m_pathLst;

        //overlap type -> switch position
        public ObjOverlap(GENERIC_SYSTEM_PARAMETERS.OVERLAPS.OVERLAP overlap, GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL sig)
        {
            m_Ol = overlap;
            //BMGR-0066
            m_switchPos = GetSwitchPos();

            m_pathLst = new List<PathInfo>();

            m_sig = sig;
        }
        public string GetSigname()
        {
            return NodeApi.getNameNullSafe(m_sig);
        }

        public int GetDistance()
        {
            if (null != m_Ol)
            {
                return m_Ol.D_Overlap;
            }
            return 0;
        }
        public string Info
        {
            get
            {
                if (null == m_sig)
                {
                    return $"SyDBOverlap ID[{m_Ol.ID}] Name[{m_Ol.Name}] relate no Signal";
                }
                
                return $"SyDBOverlap ID[{m_Ol.ID}] Name[{m_Ol.Name}] relate Signal[{m_sig.Name}]";
            }
        }
        //BMGR-0031
        public bool CalVariants(List<Variant> vList)
        {
            string pos = Sys.Reverse;
            if (Sys.Normal == m_switchPos)
            {
                pos = Sys.Normal;
            }

            List<int> allPointID = new List<int>();

            foreach (PathInfo path in m_pathLst)
            {
                foreach (PointInfo p in path.pointList)
                {
                    if (false == allPointID.Contains(p.Point.ID))
                    {
                        allPointID.Add(p.Point.ID);
                        p.CalVariants(vList, pos);
                    }
                }
            }
            return true;
        }
        //BMGR-0069 cal ol points info by block link
        private void GeneratePathPtList(GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK curB, string dir, string pos, ref List<PointInfo> ptList)
        {
            int nextID = SyDB.GetNextBlkID(curB, dir, pos);

            if (nextID == curB.ID)//there is a coo, then change dir
            {
                dir = SyDB.GetReverseDir(dir);
                nextID = SyDB.GetNextBlkID(curB, dir, pos);
            }

            if (-1 != m_Ol.Overlap_Block_ID_List.Block_ID.FindIndex(x => nextID == Convert.ToInt32(x.Value)))
            {
                var nextB = (GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK)Sys.GetNode(nextID, SyDB.GetInstance().blockInfoList.Cast<Node>().ToList());
                if (null != curB.Point_ID && null != nextB.Point_ID && curB.Point_ID == nextB.Point_ID)
                {//get a valid point
                    var pt = (GENERIC_SYSTEM_PARAMETERS.POINTS.POINT)Sys.GetNode((int)curB.Point_ID, SyDB.GetInstance().pointInfoList.Cast<Node>().ToList());
                    PointInfo ptInfo = new PointInfo(pt, SyDB.GetPosByBlocks(curB, nextB), SyDB.GetOrientByBlocks(curB, nextB), PointLocation.Overlap,this.m_Ol);
                    ptList.Add(ptInfo);
                }
                //go on search next
                GeneratePathPtList(nextB, dir, pos, ref ptList);
            }
        }
        //BMGR-0067
        //dst sig -> start blk
        //start blk, blk list, end overlap blk lst -> path list
        public bool GeneratePath(GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON input)
        {
            if ("" == m_switchPos)//no path will generate
            {
                return true;
            }

            if (0 == m_Ol.Overlap_Switch_ID_List.Switch_ID.Count)
            {
                //this ol has no point, so only 1 path with no point.
                PathInfo path = new PathInfo(m_sig);
                m_pathLst.Add(path);
            }
            else
            {
                if (null == m_sig|| false == m_sig.checkDirection())
                {
                    TraceMethod.RecordInfo($"GeneratePath error, {Info} sig[{GetSigname()}] is invalid !");
                    return false;
                }
                if (0 == m_Ol.Overlap_Block_ID_List.Block_ID.Count)
                {
                    TraceMethod.RecordInfo($"GeneratePath error, {Info} Overlap_Block_ID_List is empty!");
                    return false;
                }
                
                foreach (string pos in Sys.PointPositions)
                {
                    if (m_switchPos == pos || "Either" == m_switchPos)
                    {
                        var startB = (GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK)Sys.GetNode((int)m_Ol.Overlap_Block_ID_List.Block_ID[0], SyDB.GetInstance().blockInfoList.Cast<Node>().ToList());
                        List<PointInfo> pathPtList = new List<PointInfo>();
                        GeneratePathPtList(startB, m_sig.Direction, pos, ref pathPtList);
                        if (0 == pathPtList.Count)
                        {
                            PathInfo new1Path = new PathInfo(m_sig);
                            m_pathLst.Add(new1Path);
                            break;//get no point, then won't be able to has 2 path
                        }
                        else
                        {
                            PathInfo newPath = new PathInfo(m_sig, pathPtList);
                            m_pathLst.Add(newPath);
                        }
                    }
                }
                if ("Either" == m_switchPos && 2 == m_pathLst.Count())
                {
                    foreach (PathInfo path in m_pathLst)
                    {
                        foreach (PointInfo pt in path.pointList)
                        {
                            if (pt.Orientation == Sys.Divergent)
                            {//check the point-pos of first divergent point
                                if (-1 == input.getInputRank(pt.Point.Name, pt.Position))
                                {//point-pos not in IBBM, then only reverse path
                                    m_pathLst.RemoveAt(0);
                                }
                                break;
                            }
                        }
                        break;
                    }

                    if (2 == m_pathLst.Count())
                    {
                        if (m_pathLst[0].GetOverlapPathName() == m_pathLst[1].GetOverlapPathName())
                        {
                            m_pathLst.RemoveAt(1);
                        }
                    }
                }
            }

            return true;
        }
        
        public XmlVisitor GetXmlNode()
        {
            XmlVisitor node = XmlVisitor.Create("Overlap", null);

            {//BMGR-0065
                node.UpdateAttribute("ID", m_Ol.ID);
                node.AppendChild("Distance", Sys.Cm2Meter(m_Ol.D_Overlap, 3).ToString(Sys.StrFormat));
            }

            //BMGR-0066 BMGR-0067
            if ("" != m_switchPos)
            {
                node.AppendChild("Switch_position", m_switchPos);
                
                foreach (PathInfo path in m_pathLst)
                {
                    node.AppendChild(path.GetXmlNode());
                }
            }

            return node;
        }

        //BMGR-0066
        private string GetSwitchPos()
        {
            string sp = "";
            if (null == m_Ol)
            {
                return sp;
            }
            switch ((string)m_Ol.Overlap_Type)
            {
                case "Not Interlocked":
                case "Reduce":
                case "":
                    break;
                case "CBI Critical Normal":
                    sp = "Normal";
                    break;
                case "CBI Critical Reverse":
                    sp = "Reverse";
                    break;
                case "CBI Preferred Normal":
                case "CBI Preferred Reverse":
                    sp = "Either";
                    break;
                default:
                    TraceMethod.Record(TraceMethod.TraceKind.WARNING, $"Invalid OverlapType in sydb [{m_Ol.Overlap_Type}]");
                    break;
            }
            return sp;
        }

    }
}
