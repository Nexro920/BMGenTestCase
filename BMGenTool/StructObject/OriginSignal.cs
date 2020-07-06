using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Summer.System.IO;
using MetaFly.Summer.IO;
using MetaFly.Summer.Generic;
using BMGenTool.Generate;

namespace BMGenTool.Info
{
    public class OriginSignal
    {
        public GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL SignalInfo;//from signal
        public GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON.INPUT_SIGNAL m_ibbmIn;//from IBBM

        public List<RouteSegment> RsList = new List<RouteSegment>();
        //BMGR-0027
        public bool CalVariants(List<Variant> vList)
        {
            Variant vsig = new VariantSignal(this);
            vList.Add(vsig);
            bool isOverlap = false;
            if (Sys.Reopening == m_ibbmIn.Type)//BMGR-0026 only reopen signal of the beacon use overlap
            {
                isOverlap = true;
            }
            foreach (RouteSegment rs in RsList)
            {
                rs.CalVariants(vList, isOverlap);
            }
            return true;
        }
        public OriginSignal(GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL signal, GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON.INPUT_SIGNAL input)
        {
            //BMGR-0017 set ibbm and signal info
            this.SignalInfo = signal;
            this.m_ibbmIn = input;
        }
        //BMGR-0022
        public bool GetRouteSegments(RouteSegConfig SyDBRouteCfg, GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON beaconIbbm)
        {
            //use SigTypeFunc instead of Type_function, the values are similar
            if (Sys.TYPEFUNC_SPACING == SignalInfo.Signal_Type_Function)
            {
                UpdateRsList(SyDBRouteCfg.m_Spacing_routeLst, beaconIbbm);
                if (1 != RsList.Count())
                {
                    TraceMethod.RecordInfo($"Original Signal[{SignalInfo.Info}] find routesegment error, the route num={RsList.Count()} of Spacing Signal should be 1!");
                    return false;
                }
            }
            else if (Sys.TYPEFUNC_ROUTESPACING == SignalInfo.Signal_Type_Function)
            {
                UpdateRsList(SyDBRouteCfg.m_RouteSpacing_routeLst, beaconIbbm);
                if (RsList.Count() < 1 || RsList.Count() > 10)
                {
                    TraceMethod.RecordInfo($"Original Signal[{SignalInfo.Info}] find routesegment error, the route num={RsList.Count()} of Route Spacing Signal should be [1,10]!");
                    return false;
                }
            }
            else
            {
                TraceMethod.RecordInfo($"Original Signal[{SignalInfo.Info}] SigTypeFunc={SignalInfo.Signal_Type_Function} is unknow!");
                return false;
            }

            return true;
        }

        //BMGR-0022 get route by route.orgSig = becon.input_orignsig
        private void UpdateRsList(List<RouteSegment> routeLst, GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON beaconIbbm)
        {
            RsList.Clear();
            foreach (RouteSegment route in routeLst)//get route segment
            {
                if (route.m_OrgSig.ID == SignalInfo.ID)
                {
                    RouteSegment newRoute = route.DeepClone();
                    if (Sys.Reopening == m_ibbmIn.Type)//BMGR-0026 only reopen signal of the beacon use overlap
                    {
                        newRoute.CalOverlap(beaconIbbm);
                    }
                    RsList.Add(newRoute);
                }
            }
        }

        public string GetName()
        {
            return SignalInfo.Name;
        }

        public XmlVisitor GetXmlNode(string bName)
        {
            if (0 == RsList.Count)
            {
                TraceMethod.RecordInfo($"Warning: Beacon[{bName}] OriginSignal[{SignalInfo.Info}] has no RouteSegment!");
            }

            XmlVisitor orgSigNode = XmlVisitor.Create("Origin_signal", null);

            {//BMGR-0017
                orgSigNode.UpdateAttribute("NAME", m_ibbmIn.Name);
                orgSigNode.AppendChild("Id", SignalInfo.ID);

                if (Sys.TYPEFUNC_ROUTESPACING == SignalInfo.Signal_Type_Function)
                {
                    orgSigNode.AppendChild("Type_function", "Route_spacing_signal");
                }
                else if (Sys.TYPEFUNC_SPACING == SignalInfo.Signal_Type_Function)
                {
                    orgSigNode.AppendChild("Type_function", "Spacing_signal");
                }
                else
                {
                    TraceMethod.RecordInfo($"sydb Error:signal[{GetName()}] Signal_Type_Function={SignalInfo.Signal_Type_Function} is unknow!");
                }

                orgSigNode.AppendChild("BMB_type", m_ibbmIn.Type);
            }

            foreach (RouteSegment route in RsList)
            {
                XmlVisitor routeNode = XmlVisitor.Create("Route_segment", null);
                {
                    routeNode.UpdateAttribute("NAME", route.GetName());
                    int len = route.GetLength();
                    if (len <= 0)
                    {
                        TraceMethod.RecordInfo($"signal[{GetName()}] Route {route.Info} get length[{len}] is invalid!");
                    }
                    routeNode.AppendChild("Length", Sys.Cm2Meter(len, 3).ToString(Sys.StrFormat));

                    foreach (PointInfo pt in route.m_PtLst)
                    {
                        routeNode.AppendChild(pt.GetXmlNode());
                    }

                    //BMGR-0036
                    //use IBBM signal type instead of BMB_type
                    if (Sys.Reopening == m_ibbmIn.Type)//BMGR-0026 only reopen signal of the beacon use overlap
                    {
                        routeNode.AppendChild(route.GetDstSigXmlNode(true));
                    }
                    else
                    {
                        routeNode.AppendChild(route.GetDstSigXmlNode(false));
                    }
                }
                orgSigNode.AppendChild(routeNode);
            }
            
            return orgSigNode;
        }

    }

}
