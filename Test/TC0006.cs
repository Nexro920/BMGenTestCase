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
    /// case0006
    /// test for ovelapconfig.cs
    /// </summary>
    public class TC0006 : AbstractTestCase
    {
        private bool haschecked = false;
        public override int CurTestCaseID => 006;

        private SyDB sydb = SyDB.GetInstance();

        public override void Run()
        {
            test_overlapAPI();
            Console.WriteLine($"Run case {CurTestCaseID} for ovelapconfig.cs test overlap nothing, PASS!");
        }

        public void test_overlapAPI()
        {
            Prepare.ReloadGlobalSydb(".//input//overlapsSydb_FromHHHT2.xml");

            ObjOverlap ovelap = new ObjOverlap(null, null);
            Debug.Assert(true == ovelap.GeneratePath(null));
            Debug.Assert(ovelap.GetSigname()== "Null");

            Dictionary<string, string[]> validoverlaps = new Dictionary<string, string[]>() {
                //overlap                   //dstSig  //ibbm   //variant num //olpathInfo
                { "O_S0101",    new string[]{"S0101", "VB0101" ,"0",            ""} },
                { "O_S0102",    new string[]{"S0102", "VB0102" ,"0",            ""} },
                { "O_S0103",    new string[]{"S0103", "VB0103" ,"0",            "S0103" } },
                { "O_S0107",    new string[]{"S0107", "VB0107" ,"2",            "S0107|P0101-N S0107|P0101-R" } },
                { "O_S0107_N",  new string[]{"S0107", "VB0107" ,"2",            "S0107|P0101-N" } },
                { "O_S0107_R",  new string[]{"S0107", "VB0107" ,"2",            "S0107|P0101-R" } },
                {"O_S0107_NOIN",new string[]{"S0107", "VB0107E","2",            "S0107|P0101-R" } },
                {"O_X0109",     new string[]{"X0109", "VB0109" ,"6",            "X0109|P0105-R|P0103-R|P0102-R" } },
                { "O_S0304",    new string[]{"S0304", "VB0304" ,"4",            "S0304|P0302-N|P0602-N S0304|P0302-N|P0602-R" } },
                {"O_S0304_NOIN",new string[]{"S0304", "VB0304E" ,"4",           "S0304|P0302-N|P0602-R" } },
                { "O_S1101_N",  new string[]{"S1101", "VB1101" , "4",           "S1101|P1101-N|P1103-N" } },
                { "O_S1101_R",  new string[]{"S1101", "VB1101" , "2",           "S1101|P1101-R" } },
                { "O_S0608",    new string[]{"S0608", "VB0608" , "6",           "S0608|P0606-N|P0608-N|P0610-R S0608|P0606-R" } }
            };

            #region check for valid ovelap
            {
                foreach (var ol in sydb.overlapInfoList)
                {
                    if (validoverlaps.ContainsKey(ol.Name))
                    {
                        string signame = validoverlaps[ol.Name][0];
                        GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL sig = (GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL)Sys.GetNode(signame, sydb.signalInfoList.Cast<Node>().ToList());
                        ObjOverlap overlap = new ObjOverlap(ol, sig);
                        Debug.Assert(overlap.GetSigname() == signame);

                        GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON inb = (GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON)Sys.GetNode(validoverlaps[ol.Name][1], sydb.ibbmInfoList.Cast<Node>().ToList());
                        Debug.Assert(true == overlap.GeneratePath(inb));
                        Debug.Assert(validoverlaps[ol.Name][3] == Prepare.getXmlNodeStr(overlap.GetXmlNode(), "/Path/@NAME"));

                        List<Variant> vlist = new List<Variant>();
                        Debug.Assert(true == overlap.CalVariants(vlist));
                        Debug.Assert(vlist.Count.ToString() == validoverlaps[ol.Name][2]);

                        haschecked = true;
                    }
                }
                Debug.Assert(haschecked == true);
                haschecked = false;
            }
            #endregion

        }
        
    }

}