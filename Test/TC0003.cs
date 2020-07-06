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
    /// case0003
    /// test for BeaconMessage.cs
    /// </summary>
    public class TC0003 : AbstractTestCase
    {
        private bool haschecked = false;
        public override int CurTestCaseID => 003;
        public override void Run()
        {
            test_DefaultGenerateMessage();
            Console.WriteLine($"Run case {CurTestCaseID} for BeaconMessage.cs only default msg lack common msg, PASS!");
        }
        /// <summary>
        /// test generate default message of VB and FB from csv and xml
        /// </summary>
        public void test_DefaultGenerateMessage()
        {
            BeaconMessage bm = new BeaconMessage();
            Debug.Assert("" == bm.GenerateMessage(null, 1));

            //Arrange
            string csv = ".//input//validbeacons.csv";
            string xml = ".//input//validbeacons.xml";
            BFGen bfgen = new BFGen(csv, xml, "", false, false);
            Debug.Assert(bfgen.Init() == true);
            SyDB sydb = SyDB.GetInstance();
            int[] lineids = { 0, 1, 0x3ff };
            int i = 0;
            foreach (IBeaconInfo b in sydb.GetBeacons())
            {
                //Act
                BeaconMessage bm1 = new BeaconMessage();
                int lineid = lineids[i%(lineids.Count())];
                checkbeaconmessage(b, bm1.GenerateMessage(b, lineid), lineid);
                ++i;
            }
            Debug.Assert(i == 12);
        }
        /// <summary>
        /// check the lineid bid bversionid in default message of FB and VB
        /// </summary>
        /// <param name="b"></param>
        /// <param name="msg"></param>
        /// <param name="lineid"></param>
        public static void checkbeaconmessage(IBeaconInfo b, string msg, int lineid)
        {
            string pattern = "";
            if (false == b.IsVariantBeacon())
            {
                pattern = @"90 00 7F([0-9a-fA-F\s]*)B 10 18 32 ([0-9a-fA-F\s]*)F FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FC";
            }
            else
            {
                pattern = @"90 00 7E([0-9a-fA-F\s]*)B 10 18 32 ([0-9a-fA-F\s]*)B 10 3E 32 C0 00 08 00 00 40 00 00 00 00 00 03 FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FC";
            }
            //Assert
            Match match = Regex.Match(msg, pattern);
            Debug.Assert(match.Groups.Count == 3);

            Debug.Assert(lineid == (int)getInt64inhex(match.Groups[1].ToString(), 0x007FE0000, 17));
            Debug.Assert(b.ID == (int)getInt64inhex(match.Groups[1].ToString(), 0x00001FFF8, 3));
            Debug.Assert(b.getVersion() == (int)getInt64inhex(match.Groups[2].ToString(), 0x003FFFC, 2));
        }

        public static Int64 getInt64inhex(string hex, Int64 mask, int moveright)
        {
            hex = hex.Replace(" ", "");
            Int64 i = Int64.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return ((mask) & i)>>moveright;
        }
    }
}