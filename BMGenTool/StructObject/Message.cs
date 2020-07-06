using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Summer.System.IO;
using MetaFly.Summer.IO;
//using TOOLCommon.Trace;
using MetaFly.Summer.Generic;

namespace BMGenTool.Info
{
    public class Message
    {
	    public string comSection = "";//定义是否有组合消息，用于区分默认报文和红灯报文
        public string UpSection = "";
        public string RpSection = "";
        public string ApSection = "";
        public string OlSection = "";
        public int Rank;

        public PathInfo upPath;//与UpSection对应的upstream_path
        public RouteSegment rpRs;//与RpSection对应的Routesegment
        public RouteSegment apRs;//与ApSection对应的Routesegment
        public PathInfo olPath;//与OlSection对应的path
        public ObjOverlap overlap;//THE OL OF OLPATH, when OL has no path, use this.
        
        public string VarState;
        public string UbiTC;
        public string InterOper;

        private string m_combinedsectionsBuffer = "";

        public List<PointInfo> GetAllPoints()
        {
            List<PointInfo> list = new List<PointInfo>();

            if (null != upPath)
            {
                list.AddRange(upPath.pointList);
            }

            if (null != rpRs)
            {
                list.AddRange(rpRs.m_PtLst);
            }

            if (null != apRs)
            {
                list.AddRange(apRs.m_PtLst);
            }

            if (null != olPath)
            {
                list.AddRange(olPath.pointList);
            }

            return list;
        }

        public Message()
        {
            upPath = null;
            rpRs = null;
            apRs = null;
            olPath = null;
            overlap = null;
        }

        public Message(int rank, Message basem = null)
        {
            Rank = rank;
            if (null != basem)
            {
                upPath = basem.upPath;
                rpRs = basem.rpRs;
                apRs = basem.apRs;
                olPath = basem.olPath;

                overlap = basem.overlap;
            }
        }
        
        public int GetRank()
        {
            if (0 == Rank)
            {
                if (null != upPath
                    || null != rpRs
                    || null != apRs)
                {
                    return -1;
                }
            }

            return Rank;
        }
        /// <summary>
        /// Judge if contain signal
        /// Judge if contain point and pointvar pos
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        public bool IsContainVar(Variant var)
        {
            //if CombinSection exist the signal
            if (VAR_TYPE.E_SIGNAL == var.GetVarSrc())
            {
                if (-1 != m_combinedsectionsBuffer.IndexOf(var.GetName()))
                {
                    return true;
                }
            }
            else if (VAR_TYPE.E_POINT == var.GetVarSrc())
            {//BMGR-0051 if CombinSection exist the point-position
                string buff = var.GetName() + "-" + ((VariantPoint)var).GetPointVarPos();
                if (-1 != m_combinedsectionsBuffer.IndexOf(buff))
                {
                    return true;
                }
                buff = var.GetName() + "_" + ((VariantPoint)var).GetPointVarPos();
                if (-1 != m_combinedsectionsBuffer.IndexOf(buff))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// judge if contain signalvar's signale name
        /// judge if contian pointvar's point name
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        public bool IsContainVarDevice(Variant var)
        {//BMGR-0051 if CombinSection exist the point or signal name. [no position info]
            string name = "xxx";
            name = var.GetName();
            
            if (-1 != m_combinedsectionsBuffer.IndexOf(name))
            {
                return true;
            }

            return false;
        }
        //return the signal state in the message
        //if signal should open ,return 1
        //if signal include but closed, return 0
        //if signal not include,return -1
        public int GetSignalState(string sigName)
        {
            if (0 == RpSection.IndexOf(sigName))
            {//signal is reopen route start signal
                return 1;
            }
            else if (0 == ApSection.IndexOf(sigName))
            {//signal is app route start signal
                return 1;
            }
            else if (RpSection.EndsWith(sigName))
            {//in reopen section and has no app, signal close
                if ("" == ApSection)
                {
                    return 0;
                }
            }
            else if (ApSection.EndsWith(sigName))
            {
                return 0;
            }
            return -1;
        }

        //return the variant state in the message
        //if signal should open ,return 1
        //if signal include but closed, return 0
        //if signal not include,return -1
        //if point not include, return -1
        //if point-var include, return 1, else 0
        public int GetVariantState(Variant var)
        {
            if (VAR_TYPE.E_SIGNAL == var.GetVarSrc())
            {
                return GetSignalState(var.GetName());
            }
            else
            {
                if (true == IsContainVarDevice(var))
                {
                    if (true == IsContainVar(var))
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            return -1;
        }

        private void AddCombinSectionNode(ref XmlVisitor node, string attri, string value)
        {
            m_combinedsectionsBuffer += "[" + value + "]";
            node.AppendChild(attri, value);
        }

        public XmlVisitor GetCombinedSectionsNode()
        {
            XmlVisitor Node = XmlVisitor.Create("Combined_sections", null);

//BMGR-0048 red signal has <Combined_sections />

            if (null != upPath)
            {//BMGR-0070
                AddCombinSectionNode(ref Node, "Upstream_section", upPath.GetUpstreamPathName());
                UpSection = upPath.GetUpstreamPathName();
            }

            if (null != rpRs)
            {//BMGR-0077
                AddCombinSectionNode(ref Node, "Reopening_section", rpRs.GetName());
                RpSection = rpRs.GetName();
            }

            if (null != apRs)
            {//BMGR-0078
                AddCombinSectionNode(ref Node, "Approach_section", apRs.GetName());
                ApSection = apRs.GetName();
            }

            //BMGR-0079
            if (null != olPath)
            {
                AddCombinSectionNode(ref Node, "Overlap_section", olPath.GetOverlapPathName());
                OlSection = olPath.GetOverlapPathName();
            }
            else if (null != overlap)
            {
                AddCombinSectionNode(ref Node, "Overlap_section", overlap.GetSigname());
                OlSection = overlap.GetSigname();
            }

            return Node;
        }

        public XmlVisitor GetXmlNode()
        {
            XmlVisitor Node = XmlVisitor.Create("Message", null);

            Node.UpdateAttribute("RANK", Rank);
            //BMGR-0048 rank=0 has no Combined_sections
            if (0 != Rank)
            {
                Node.AppendChild(GetCombinedSectionsNode());
            }

            //Variant_state

            //Urbalise_iTC

            //Interoperable
            //they are generate after call this function

            return Node;
        }
    }
}
