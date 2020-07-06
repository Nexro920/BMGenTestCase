using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BMGenTool.Info;
//using TOOLCommon.Trace;
using MetaFly.Summer.Generic;
using MetaFly.Datum.Figure;

namespace BMGenTool.Generate
{
    //for each route should cal the org sig,dst sig, blk list,pt list,overlap
    //sydb.route has org sig id,dst sig id, blk id list
    //so 1, SetRoute() sigid->signal, dst sig id->overlap id->overlap info
    //   2, blk id list -> blk list
    //   3, GetPointLst() blk list -> pt list
    public class RouteSegConfig
    {
        private SyDB Sydb;

        public List<RouteSegment> m_RouteSpacing_routeLst;
        public List<RouteSegment> m_Spacing_routeLst;

        private bool AddNewRoute(bool isSplitRoute, RouteSegment rs)
        {
            if (isSplitRoute == true)
            {
                if (rs.m_PtLst.Count > 0)
                {
                    TraceMethod.Record(TraceMethod.TraceKind.ERROR, $"sydb split route[{rs.Info}] should has no point, this route will ignore\n");
                    return false;
                }
                if (null == m_Spacing_routeLst.Find(x => (x.m_OrgSig.ID == rs.m_OrgSig.ID 
                                                        && x.m_BlkLst.Count == rs.m_BlkLst.Count && x.m_BlkLst.All(rs.m_BlkLst.Contains))))
                {
                    m_Spacing_routeLst.Add(rs);
                }
                else
                {
                    TraceMethod.RecordInfo($"Warning: split route[{rs.Info}] is repeated, ignore the repeat ones!\r\n");
                }
            }
            else
            {
                if (null == m_RouteSpacing_routeLst.Find(x => (x.m_OrgSig.ID == rs.m_OrgSig.ID 
                                                        && x.m_BlkLst.Count == rs.m_BlkLst.Count && x.m_BlkLst.All(rs.m_BlkLst.Contains))))
                {
                    m_RouteSpacing_routeLst.Add(rs);
                }
                else
                {
                    TraceMethod.RecordInfo($"Warning: route[{rs.Info}] is repeated, ignore the repeat ones!\r\n");
                }
            }
            return true;
        }

        public RouteSegConfig(SyDB sydb)
        {
            this.Sydb = sydb;
            m_RouteSpacing_routeLst = new List<RouteSegment>();
            m_Spacing_routeLst = new List<RouteSegment>();
        }

        public bool generateRouteSegments()
        {
            //BMGR-0022 if more than one route, should be ordered by ID increase.
            List<GENERIC_SYSTEM_PARAMETERS.ROUTES.ROUTE> orderRouteList = Sydb.routeInfoList.OrderBy(o => (int)o.ID).ToList();
            foreach (GENERIC_SYSTEM_PARAMETERS.ROUTES.ROUTE route in orderRouteList)
            {
                if (false == route.IsValidBMRoute())
                {
                    continue;
                }

                //this is a RouteSpacing route
                if (null == route.Spacing_Signal_ID_List.Signal_ID || 0 == route.Spacing_Signal_ID_List.Signal_ID.Count)
                {
                    try
                    {
                        RouteSegment rs = new RouteSegment(route);
                        AddNewRoute(false, rs);
                    }
                    catch(Exception ex)
                    {
                        TraceMethod.RecordInfo(ex.Message);
                    }
                }
                else if (0 < route.Spacing_Signal_ID_List.Signal_ID.Count)//split the route to 1 RouteSpacing route and 1 or more Spacing route
                {//after BMGR-0022: split the route which has spacing signals
                    splitRoute(route);
                }
            }
            return true;
        }
        private bool splitRoute(GENERIC_SYSTEM_PARAMETERS.ROUTES.ROUTE route)
        {
            if (null == route)
            {
                return false;
            }
            int startSigID = route.Origin_Signal_ID;
            List<GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK> blkList = new List<GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK>();
            List<int> dstSigIDs = new List<int>();
            dstSigIDs.AddRange(route.Spacing_Signal_ID_List.Signal_ID.Cast<int>());
            dstSigIDs.Add(route.Destination_Signal_ID);
            foreach (int blkID in route.Block_ID_List.Block_ID)
            {
                var blk = (GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK)Sys.GetNode(blkID, Sydb.blockInfoList.Cast<Node>().ToList());

                int dstSigID = SyDB.GetSigIDInBlock(blkID, dstSigIDs);
                if (0 < dstSigID && dstSigIDs.Exists(x => x == dstSigID))
                {
                    List<GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK> newBlkList = new List<GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK>();
                    newBlkList.AddRange(blkList);
                    newBlkList.Add(blk);
                    try
                    {
                        RouteSegment split_rs = new RouteSegment(startSigID, dstSigID, newBlkList, route);

                        //update info for new split route
                        {
                            blkList.Clear();
                            dstSigIDs.Remove(dstSigID);
                            startSigID = dstSigID;
                        }

                        if (route.Origin_Signal_ID == split_rs.m_OrgSig.ID)//route_spacing signal route
                        {
                            AddNewRoute(false, split_rs);
                        }
                        else//spacing signal route
                        {
                            AddNewRoute(true, split_rs);
                        }
                    }
                    catch (Exception ex)
                    {
                        TraceMethod.RecordInfo(ex.Message);
                    }
                }
                else
                {
                    blkList.Add(blk);
                }
            }//end of foreach (int blkID in route.BlockIDList)
            if (0 != dstSigIDs.Count)
            {
                TraceMethod.RecordInfo($"route {route.Info} split error, some signal can't get splited route, such as signalID[{dstSigIDs[0]}]\n");
                return false;
            }
            return true;
        }

    }
}
