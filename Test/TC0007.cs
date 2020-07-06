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
using MetaFly.Summer.IO;
using MetaFly.Summer.Generic;

namespace BMGenTool.Info
{
    /// <summary>
    /// case0007
    /// test for SDDB_BMB_Distance
    /// </summary>
    public class TC0007 : AbstractTestCase
    {
        private bool haschecked = false;
        public override int CurTestCaseID => 007;
        public override void Run()
        {
            test_GenerateBMBSDDBDisInfoNode();
            Console.WriteLine($"Run case {CurTestCaseID} for SDDB_BMB_Distance GENERATE T F & XMLREADDIS & BeaconMessage DIS get, PASS!");
        }

        public void test_GenerateBMBSDDBDisInfoNode()
        {

            List<LEU> leulist = new List<LEU>();
            List<BEACON> blist = new List<BEACON>();
            BMVFGen bmvf = new BMVFGen(false, ref blist, ref leulist);

            XmlVisitor beaconNodenull = XmlVisitor.Create("Beacon", null);
            Debug.Assert(false == bmvf.GenerateBMBSDDBDisInfoNode(null, ref beaconNodenull));

            Prepare.ReloadGlobalSydb(".//input//BMBDisSydb.xml");
            BFGen bf = new BFGen(".//input//BMBDisBeacons.csv", ".//input//BMBDisBeacons.xml", "", false, false);
            MethodHelper.InvokePrivateMethod<BFGen>(bf, "Init");
            bmvf.GenrateDeviceByIBBM();

            //the corresponding beacons are:
            //VB0102 VB0106 vb0101 vb0110 vb0111 vb0203 VB0609 vb1402 VB1303 ib1303 vb2002 fb1914 vb1705 VB0614 VB0601 VB0604
            string[] validdis = {"8.630", "5.910","6.410","5.340","6.020","6.060","5.710","5.550","5.650","5.650","130.580","44.740","73.580","3.230","3.100","6.000" };

            #region test the beacons of valid bmbdis
            {
                int beaconi = 0;
                foreach (var curdis in validdis)
                {
                    XmlVisitor beaconNode = XmlVisitor.Create("Beacon", null);
                    haschecked = false;
                    Debug.Assert(true == bmvf.GenerateBMBSDDBDisInfoNode(blist[beaconi], ref beaconNode));

                    //check BMBSDDB calculate and node generate
                    Debug.Assert(curdis == Prepare.getXmlNodeStr(beaconNode, "BMB_SDDB_distance"));

                    //check BeaconMessage use the right BMB_Dis
                    BeaconMessage bm = new BeaconMessage();
                    bm.GenerateMessage(blist[beaconi], 1, null);
                    Debug.Assert(bm.BMB_Distance_Unitcm() == blist[beaconi].BMB_Distance_cm);

                    ++beaconi;
                }
            }
            #endregion

            #region test the beacons of invalid bmbdis
            {//IB0302 FB1916
                int errBeaconi = validdis.Length;
                for (; errBeaconi < blist.Count; errBeaconi++)
                {
                    Debug.Assert(false == bmvf.GenerateBMBSDDBDisInfoNode(blist[errBeaconi], ref beaconNodenull));
                }
            }
            #endregion
        }

    }

    public static class test7Extend
    {
        public static int BMB_Distance_Unitcm(this BeaconMessage instance)
        {
            return (int)MethodHelper.InvokePrivateMember(instance, "BMB_Distance_Unitcm");
        }

        public static bool GenerateBMBSDDBDisInfoNode(this BMVFGen instance, BEACON b, ref XmlVisitor node)
        {
            return (bool)MethodHelper.InvokePrivateMethod<BMVFGen>(instance, "GenerateBMBSDDBDisInfoNode", new object[] { b, node});
        }
    }

 }