using System;
using System.Collections.Generic;
using System.Linq;
using TestKit;
using BMGenTool.Common;
using System.Diagnostics;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using MetaFly.Serialization;
using MetaFly.Datum.Figure;
using BMGenTool.Generate;
using System.Text.RegularExpressions;
using System.Reflection;
using MetaFly.Summer.Generic;

namespace BMGenTool.Info
{
    /// <summary>
    /// case0005
    /// test for RouteSegConfig.cs and RouteSegment.cs
    /// </summary>
    public class TC0005 : AbstractTestCase
    {
        private bool haschecked = false;
        public override int CurTestCaseID => 005;
        public override void Run()
        {
            test_RouteSegmentLenVariantPts();
            test_RouteSegment();
            test_AddNewRoute();
            test_splitRoute();
            test_generateRouteSegments();
            Console.WriteLine($"Run case {CurTestCaseID} for RouteSegConfig.cs test generateRouteSegments & AddNewRoute & splitRoute & getlength & getpoints lack getvariants overlap, PASS!");
        }

        private SyDB sydb = SyDB.GetInstance();

        void test_RouteSegmentLenVariantPts()
        {
            Prepare.ReloadGlobalSydb(".//input//RouteLength_fromTestLine.xml");
            Dictionary<int, int[]> routes = new Dictionary<int, int[]>() {
                //id                //ptnum     //length
                { 234,    new int[]{13,         451833 - 393833 + 600100 - 592163 + 611317 - 606247 + 4000 + 626856 - 611317 + 632996 - 626856 + 961697 - 632996 } },
                { 233,    new int[]{8,          451833 - 393833 + 600100 - 592163 + 611317 - 606247 + 4000 + 626856 - 611317 + 632996 - 626856 + 641374 - 632996 } }, 
                { 50,     new int[]{5,          451833 - 446833 + 604100 - 592163 + 641374 - 605177 } },
                { 400,    new int[]{0,          1197376 - 1115639 + 240000 } },
                { 401,    new int[]{0,          1197376 - 1115639 + 240000 } },
                { 402,    new int[]{0,          1197376 - 1115639 + 240000 } },
                { 403,    new int[]{0,          1197376 - 1115639 + 240000 } },
                { 404,    new int[]{0,          1197376 - 1115639 + 240000 } },
                { 405,    new int[]{0,          1197376 - 1115639 + 240000 } },
                { 445,    new int[]{2,          1035200  - 1000000 } },
                { 61,     new int[]{2,          613473 - 842 - 485281 } },
                { 66,     new int[]{2,          613473 - 842 - 485281 } },
            };

            Dictionary<string, string[]> pts = new Dictionary<string, string[]>() {
                //NAME                       //ORIEN
                { "P02D",   new string[]{Sys.Divergent} },
                { "P20D",   new string[]{Sys.Divergent} },
                { "P08D",   new string[]{Sys.Convergent} },
                { "P21D",   new string[]{Sys.Convergent} },
                { "P10D",   new string[]{Sys.Convergent} },
                { "P14D",   new string[]{Sys.Convergent} },
                { "P09D",   new string[]{Sys.Divergent} },
                { "P07D",   new string[]{Sys.Convergent} },
                { "P01D",   new string[]{Sys.Convergent} },
                { "P16D",   new string[]{Sys.Divergent} },
                { "P18D",   new string[]{Sys.Convergent} },
                { "P02E",   new string[]{Sys.Divergent} },
                { "P20E",   new string[]{Sys.Convergent} },
                { "P01E",   new string[]{Sys.Convergent} },
                { "P08B",   new string[]{Sys.Divergent} },
                { "P02B",   new string[]{Sys.Convergent} },
                { "P12D",   new string[]{Sys.Convergent} },
                { "P06D",   new string[]{Sys.Divergent} },
            };

            haschecked = false;
            foreach (var r in sydb.routeInfoList)
            {
                RouteSegment route = new RouteSegment(r);

                if (route.GetPointLstFromBlkLst() == false)
                {
                    haschecked = true;
                }

                foreach (var pt in route.m_PtLst)
                {
                    Debug.Assert(pts[pt.Point.Name][0] == pt.Orientation);
                }

                List<Variant> vlist = new List<Variant>();
                Debug.Assert(route.CalVariants(vlist, false) == true);
                Debug.Assert(vlist.Count == routes[(int)(r.ID)][0] * 2);

                Debug.Assert(route.GetLength() == routes[(int)(r.ID)][1]);
            }

            Debug.Assert(haschecked == true);//if the route of pt more than 10, retur false
        }
        void test_generateRouteSegments()
        {
            Prepare.ReloadGlobalSydb(".//input//Routes_fromHHHT2.xml");
            RouteSegConfig SyDBRouteCfg = new RouteSegConfig(sydb);//this will call RouteSegConfig.generateRouteSegments()
            Debug.Assert(true == SyDBRouteCfg.generateRouteSegments());

            int count = 0;
            int splitcount = 0;
            foreach (var r in sydb.routeInfoList)
            {
                if (r.IsValidBMRoute())
                {
                    RouteSegment route = SyDBRouteCfg.m_RouteSpacing_routeLst.Find(x => x.m_OrgSig.ID == r.Origin_Signal_ID && x.RouteInfo().ID == r.ID);
                    Debug.Assert(null != route);
                    ++count;
                    splitcount = splitcount + r.Spacing_Signal_ID_List.Signal_ID.Count;
                    foreach (int id in r.Spacing_Signal_ID_List.Signal_ID)
                    {
                        route = SyDBRouteCfg.m_Spacing_routeLst.Find(x => x.m_OrgSig.ID == id && x.RouteInfo().ID == r.ID);
                        Debug.Assert(null != route);
                    }
                    

                    route.CalOverlap(null);

                    GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL dstsig = (GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL)MethodHelper.InvokePrivateMember(route, "DstSig");
                    if (null != dstsig.Overlap_ID)
                    {
                        Debug.Assert(null != route.m_overlap);
                    }
                    else
                    {
                        Debug.Assert(null == route.m_overlap);
                    }
                }
            }
            Debug.Assert(count == SyDBRouteCfg.m_RouteSpacing_routeLst.Count());
            Debug.Assert(splitcount == SyDBRouteCfg.m_Spacing_routeLst.Count());
        }

        void test_AddNewRoute()
        {
            Prepare.ReloadGlobalSydb(".//input//Routes_fromHHHT2.xml");
            RouteSegConfig SyDBRouteCfg = new RouteSegConfig(sydb);

            var sydbroute = sydb.routeInfoList.Find(x => x.ID == 231);//this route has point
            Debug.Assert(false == SyDBRouteCfg.AddNewRoute(true, new RouteSegment(sydbroute)));
        }

        void clearlist(RouteSegConfig routeinfo)
        {
            routeinfo.m_RouteSpacing_routeLst.Clear();
            routeinfo.m_Spacing_routeLst.Clear();
        }

        void test_splitRoute()
        {
            Prepare.ReloadGlobalSydb(".//input//Routes_fromHHHT2.xml");
            RouteSegConfig SyDBRouteCfg = new RouteSegConfig(sydb);
            Debug.Assert(false == SyDBRouteCfg.splitRoute(null));

            int[] rids = {183, 161, 66, 76, 231};//these route has Spacing_Signal_ID_List, 231 has no Spacing_Signal_ID_List also can be test by this
            foreach (int id in rids)
            {//test blockinfo of route has Spacing_Signal_ID_List
                clearlist(SyDBRouteCfg);
                var sydbroute = sydb.routeInfoList.Find(x => x.ID == id);

                Debug.Assert(true == SyDBRouteCfg.splitRoute(sydbroute));

                Debug.Assert(SyDBRouteCfg.m_RouteSpacing_routeLst[0].m_OrgSig.ID == sydbroute.Origin_Signal_ID);//get RouteSpacing_route start with same sig as 183
                Debug.Assert(SyDBRouteCfg.m_Spacing_routeLst.Count == sydbroute.Spacing_Signal_ID_List.Signal_ID.Count);//get some Spacing_route

                List<StringData> blocks = new List<StringData>();
                SyDBRouteCfg.m_Spacing_routeLst.AddRange(SyDBRouteCfg.m_RouteSpacing_routeLst);
                foreach (var r in SyDBRouteCfg.m_Spacing_routeLst)
                {
                    foreach (var b in r.m_BlkLst)
                    {
                        blocks.Add(b.ID);
                    }
                }

                Debug.Assert(sydbroute.Block_ID_List.Block_ID.All(blocks.Contains));//add all split route blocks == orinal route blocks
            }

            {//test pointlist of route has no Spacing_Signal_ID_List
                clearlist(SyDBRouteCfg);
                var sydbroute = sydb.routeInfoList.Find(x => x.ID == 231);//this route has no Spacing_Signal_ID_List
                Debug.Assert(true == SyDBRouteCfg.splitRoute(sydbroute));
                Debug.Assert(SyDBRouteCfg.m_RouteSpacing_routeLst[0].m_OrgSig.ID == sydbroute.Origin_Signal_ID);//get RouteSpacing_route same as 231
                GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL dstsig = (GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL)MethodHelper.InvokePrivateMember(SyDBRouteCfg.m_RouteSpacing_routeLst[0], "DstSig");
                Debug.Assert(dstsig.ID == sydbroute.Destination_Signal_ID);
                Debug.Assert(SyDBRouteCfg.m_Spacing_routeLst.Count == 0);//get no Spacing_route
            }
        }

        void test_RouteSegment()
        {
            Prepare.ReloadGlobalSydb(".//input//Routes_fromHHHT2.xml");
            {
                var sydbroute = sydb.routeInfoList.Find(x => x.ID == 224);//this route has no points
                RouteSegment newr = new RouteSegment(sydbroute);
                Debug.Assert(newr.m_PtLst.Count == 0);
            }

            {//check length for oginal route
                var sydbroute = sydb.routeInfoList.Find(x => x.ID == 231);//this route has points
                RouteSegment newr = new RouteSegment(sydbroute);

                List<string> pnames = new List<string>() { "P01D-N", "P07D-R", "P09D-R" };//the pts in route 231
                int i = 0;
                foreach (var p in newr.m_PtLst)//check for route get points info
                {
                    Debug.Assert(pnames[i] == p.getNamePosStr());
                    ++i;
                }
                Debug.Assert((626856-615248 + 41374-26856) == newr.GetLength());
            }

            {//check length for the splitted routes
                var sydbroute = sydb.routeInfoList.Find(x => x.ID == 183);//this route has Spacing_Signal_ID_List
                RouteSegConfig SyDBRouteCfg = new RouteSegConfig(sydb);
                clearlist(SyDBRouteCfg);
                SyDBRouteCfg.splitRoute(sydbroute);
                SyDBRouteCfg.m_RouteSpacing_routeLst.AddRange(SyDBRouteCfg.m_Spacing_routeLst);
                int[] lengths = { 1170639- 906 - (1058617- 747), 1058617 - 747 - (1041880 - 747), (1041880 - 747)-(963697 + 176) };//length for the splitted route
                int i = 0;
                foreach (var r in SyDBRouteCfg.m_RouteSpacing_routeLst)
                {
                    Debug.Assert(lengths [i]== r.GetLength());
                    ++i;
                }
            }
        }
    }
    public static class test5Extend
    {
        public static GENERIC_SYSTEM_PARAMETERS.ROUTES.ROUTE RouteInfo(this RouteSegment instance)
        {
            return (GENERIC_SYSTEM_PARAMETERS.ROUTES.ROUTE)MethodHelper.InvokePrivateMember(instance, "RouteInfo");
        }

        public static bool splitRoute(this RouteSegConfig instance, GENERIC_SYSTEM_PARAMETERS.ROUTES.ROUTE route)
        {
            return (bool)MethodHelper.InvokePrivateMethod<RouteSegConfig>(instance, "splitRoute", new object[] { route});
        }

        public static bool AddNewRoute(this RouteSegConfig instance, bool isSplitRoute, RouteSegment rs)
        {
            return (bool)MethodHelper.InvokePrivateMethod<RouteSegConfig>(instance, "AddNewRoute", new object[] { isSplitRoute, rs });
        }
    }
}