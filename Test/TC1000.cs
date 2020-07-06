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
    /// test for read invalid data
    /// boundary_beacon.xml
    /// </summary>
    public class TC1000 : AbstractTestCase
    {
        private bool haschecked = false;
        public override int CurTestCaseID => 1000;
        public override void Run()
        {
            test_readData();
            Console.WriteLine($"Run case {CurTestCaseID} for read boundary_beacon.xml data error, PASS!");
        }

        readonly string beaconxmlfullname = ".//input//errbeacon.xml";

        //check read csv file, only right range beacon will read in
        //check read xml file
        //check repeat beacon id or repeat beacon name will be ignore
        void test_readData()
        {
            string xml = ".//input//1beacon.xml";
            string buff = File.ReadAllText(xml);

            string[] tags = { "Id", "Version", "BMB_SDDB_distance" };
            List<string[]> beaconinfo = new List<string[]> {
                               //Id     //Version   //BMB_SDDB_distance     //BMB_SDDB_distance_cm
                new string[] { "1",      "2",         "0.00"                ,"0"},
                new string[] { "1",      "2",         "2048.00"             ,"204800"},
                new string[] { "1",      "2",         "2047."               ,"204700"},
                new string[] { "1",      "2",         "2047"                ,"204700"},
                new string[] { "1",      "2",         "2047.9"              ,"204790"},
                new string[] { "1",      "2",         "2047.99"             ,"204799"},
                new string[] { "1",      "2",         "2047.990"            ,"204799"},
                new string[] { "1",      "2",         "2047.991"},
                new string[] { "1",      "2",         "2047.9901"},
                new string[] { "1",      "2",         "-0.01"},
                new string[] { "1",      "2",         "2048.01"},
                new string[] { "1",      "2",         "108.00"             ,"10800"},
                new string[] { "65535",  "2",         "108.00"             ,"10800"},
                new string[] { "0",      "2",         "108.00"            },
                new string[] { "65536",  "2",         "108.00"            },
                new string[] { "1",      "0",         "108.00"             ,"10800"},
                new string[] { "1",      "255",       "108.00"             ,"10800"},
                new string[] { "1",      "-1",        "108.00"            },
                new string[] { "1",      "256",       "108.00"            },
            };

            foreach (var b in beaconinfo)
            {
                string newbuff = buff;
                for (int i = 0; i < tags.Length; i++)
                {
                    newbuff = newbuff.Replace($"{tags[i]}Change", b[i]);
                }

                savenewfile(beaconxmlfullname, newbuff);
                if (b.Length == tags.Length)
                {
                    Debug.Assert(true == TestCaseParser.RunEndlessTask(1000, RunBFGenInit));
                }
                else
                {
                    RunBFGenInit(int.Parse(b.Last()));
                }
            }

            File.Delete(beaconxmlfullname);
        }

        void RunBFGenInit(object obj)
        {
            BFGen bfgen = new BFGen("", beaconxmlfullname, "", false, false);
            bfgen.Init();
            if (obj != null)
            {
                int BMB_SDDB_distance_cm = (int)(obj);

                SyDB sydb = SyDB.GetInstance();
                List<IBeaconInfo> bs = sydb.GetBeacons();

                Debug.Assert(bs[0].BMB_Distance_cm == BMB_SDDB_distance_cm);
            }
        }

        void savenewfile(string fullname, string text)
        {
            File.WriteAllText(fullname, text);
        }

    }
}