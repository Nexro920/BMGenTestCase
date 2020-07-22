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
using MetaFly.Summer.Generic;

namespace BMGenTool.Info
{
    /// <summary>
    /// case0002
    /// test for BFGen.cs
    /// also test sydb.GetBeacons()
    /// </summary>
    public class TC0002 : AbstractTestCase
    {
        private bool haschecked = false;
        public override int CurTestCaseID => 002;
        public override void Run()
        {
            test_BFGenIni();
            test_Generate();
            Console.WriteLine($"Run case {CurTestCaseID} for BFGen.cs, PASS!");
        }

        //check read csv file, only right range beacon will read in
        //check read xml file
        //check repeat beacon id or repeat beacon name will be ignore
        void test_BFGenIni()
        {
            //Arrange
            Func<int, bool> checkbeaconnumber = (num) =>
            {
                SyDB sydb = SyDB.GetInstance();
                List<IBeaconInfo> bs = sydb.GetBeacons();
                Debug.Assert(bs.Count == num);//only read the valid beacons
                return true;
            };

            Func<int, int[], bool> checkbeaconkps = (num, kps ) =>
            {
                SyDB sydb = SyDB.GetInstance();
                List<IBeaconInfo> bs = sydb.GetBeacons();
                Debug.Assert(bs.Count == num);//only read the valid beacons
                
                for (int i = 0; i < kps.Length; ++i)
                {
                    Debug.Assert(bs[i].kp.Value == kps[i]);
                }
                return true;
            };

            string csv = ".//input//beacons.csv";
            string xml = ".//input//beacons.xml";
            //Arrange of wrong file type
            BFGen bfgen = new BFGen(xml, csv, "", false, false);
            //Act   Assert
            Debug.Assert(bfgen.Init() == false);

            //no file
            bfgen = new BFGen("", "", "", false, false);
            Debug.Assert(bfgen.Init() == false);
            
            //csv: input data valid range
            bfgen = new BFGen(csv, "", "", false, false);
            Debug.Assert(bfgen.Init() == true);
            int[] kpscsv = { 0, 10000000, 9999900, 9999910, 9999992, 9999992, 0, 10000000, 9999900, 9999910, 9999992, 9999992 }; //valid beacon kps from csv
            checkbeaconkps(21, kpscsv);

            //csv and xml beacon repeated
            bfgen = new BFGen(csv, xml, "", false, false);
            Debug.Assert(bfgen.Init() == true);
            //delete the beacon of same name and same ID
            checkbeaconnumber(24);
        }
        /// <summary>
        /// test if generate the beacon default message files
        /// </summary>
        void test_Generate()
        {
            BFGen bfgen = new BFGen("", "", "", false, false);
            {
                string xml = ".//input//validbeacons.xml";
                Debug.Assert(false == bfgen.Generate(".\\output"));
                Debug.Assert(false == checkbeaconfiles(".\\output", false));

                bfgen = new BFGen("", xml, "", false, false);
                Debug.Assert(true == bfgen.Generate(".\\output"));
                Debug.Assert(true == checkbeaconfiles(".\\output", false));
            }

            {
                string csv = ".//input//validbeacons.csv";
                bfgen = new BFGen(csv, "", "", false, true);//lack bin generate tool so rt false
                Debug.Assert(false == bfgen.Generate(".\\output"));
                Debug.Assert(false == checkbeaconfiles(".\\output", true));
                //here error. beacon which is outof range can raise error info, but data still in the list

                bfgen = new BFGen(csv, "", ".\\compiler\\CompilerBaliseV4000\\main\\compile.exe", false, true);
                Debug.Assert(true == bfgen.Generate(".\\output"));
                Debug.Assert(true == checkbeaconfiles(".\\output", false));
            }
        }

        public static bool checkbeaconfiles(string path, bool hasbin)
        {
            bool check = false;
            if (Directory.Exists(path)
                && Directory.Exists(Path.Combine(path, "Beacon")))
            {
                SyDB sydb = SyDB.GetInstance();
                List<IBeaconInfo> bs = sydb.GetBeacons();
                List<string> postfixs = new List<string>();
                postfixs.Add(".xml");
                if (hasbin)
                {
                    postfixs.Add(".tgm");
                    postfixs.Add(".udf");
                }
                foreach (IBeaconInfo b in bs)
                {
                    string subpath = Path.Combine(path, "Beacon");
                    foreach (string fix in postfixs)
                    {
                        if (!File.Exists(Path.Combine(subpath, $"{b.Name}{fix}")))
                        {
                            return false;
                        }
                        else
                        {
                            check = true;
                        }
                    }
                }
            }
            else
            {
                return false;
            }
            return check;
        }
    }

    public static class test0002Extend
    {
        public static bool Init(this BFGen instance)
        {
            return (bool)MethodHelper.InvokePrivateMethod<BFGen>(instance, "Init");  // read csv correctly.
        }
    }
}