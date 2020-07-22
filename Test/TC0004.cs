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
using MetaFly.Summer.Generic;

namespace BMGenTool.Info
{
    /// <summary>
    /// case0004
    /// test for BMVFGen.cs
    /// </summary>
    public class TC0004 : AbstractTestCase
    {
        private bool haschecked = false;
        public override int CurTestCaseID => 004;
        SyDB sydb = SyDB.GetInstance();
        public override void Run()
        {
            test_GenrateDeviceByIBBM();
            test_generate();
            Console.WriteLine($"Run case {CurTestCaseID} for BMVFGen.cs test generate & GenrateDeviceByIBBM, PASS!");
        }

        void test_generate()
        {
            List<LEU> leulist = new List<LEU>();
            List<BEACON> blist = new List<BEACON>();
            BMVFGen bmvf = new BMVFGen(false, ref blist, ref leulist);
            sydb.clear();
            Debug.Assert(false == bmvf.Generate(".//output"));//lack load data, so generate return false

            Prepare.ReloadGlobalSydb(".//input//BMVF_FromZJ.xml");
            BFGen bf = new BFGen(".//input//BMVF_FromZJ.csv", "", "", false, false);
            MethodHelper.InvokePrivateMethod<BFGen>(bf, "Init");

            Debug.Assert(true == bmvf.Generate(".//output"));  // generate BMVF file.
            Debug.Assert(true == File.Exists(".//output//BMV//block_mode_variants_file.xml"));
        }

        public void test_GenrateDeviceByIBBM()
        {
            //Arrange
            sydb.clear();
            List<LEU> leulist = new List<LEU>();
            List<BEACON> blist = new List<BEACON>();

            BMVFGen bvf = new BMVFGen(false, ref blist, ref leulist);  // initial
            //Act Assert
            Debug.Assert(false == bvf.GenrateDeviceByIBBM());
            
            //Arrange
            BFGen bf = new BFGen(".//input//validbeacons.csv", "", "", false, false);
            bf.Init();

            List<string> validleunames = new List<string>() { "E1D", "E2D", "E3D", "E1C", "E3D2", "E2D2", "E2D3", "E2C1", "E2C7"};
            GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE ibbms = FileLoader.Load<GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE>(".//input//IBBM.xml");
            sydb.ibbmInfoList.Clear();
            sydb.ibbmInfoList = ibbms.BM_Beacon;  // read all IBBM beacons

            //Act
            Debug.Assert(true == bvf.GenrateDeviceByIBBM());
            //Assert
            
            foreach (IBeaconInfo b in sydb.GetBeacons())
            {
                if (b.IsVariantBeacon() == true)
                {
                    Debug.Assert(blist.Exists(x => x.Name == b.Name));
                }
                else
                {
                    Debug.Assert(false == blist.Exists(x => x.Name == b.Name));
                }
            }

            leulist.ForEach(Print);
             
            Debug.Assert(validleunames.Count == leulist.Count());
            int leuid = 1;
            foreach (LEU l in leulist)
            {
                Debug.Assert(leuid == l.ID);
                Debug.Assert(l.Name == validleunames[leuid - 1]);
                ++leuid;
            }
        }
        public void Print(LEU s)
        {
            Console.WriteLine(s.Name);
        }
    }
    public static class test0004Extend
    {
        public static bool GenrateDeviceByIBBM(this BMVFGen instance)
        {
            return (bool)MethodHelper.InvokePrivateMethod<BMVFGen>(instance, "GenrateDeviceByIBBM");
        }
    }
}