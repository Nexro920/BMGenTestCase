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
using MetaFly.Summer.Generic;

namespace BMGenTool.Info
{
    /// <summary>
    /// case0001
    /// test for CIReportExcel
    /// </summary>
    public class TC0001 : AbstractTestCase
    {
        private bool haschecked = false;
        public override int CurTestCaseID => 001;
        public override void Run()
        {
            test_generateExcel();//this case need check excel in debug
            test_createworkbook();
            test_readcompiledmsg();
            test_getrecordfrombeacon();
            test_getdataprocess();
            Console.WriteLine($"Run case {CurTestCaseID} test for CIReportExcel.cs lack test to check the record count, PASS!");
        }
        void test_createworkbook()
        {
            FileStream sw = new FileStream("input//1.xls", FileMode.Open, FileAccess.ReadWrite);
            IWorkbook iw = CIReportExcel.createworkbook(sw);
            Debug.Assert(iw.GetType() == typeof(HSSFWorkbook));
            iw.Close();
            sw.Close();

            sw = new FileStream("input//1.xlsx", FileMode.Open, FileAccess.ReadWrite);
            iw = CIReportExcel.createworkbook(sw);
            Debug.Assert(iw.GetType() == typeof(XSSFWorkbook));
            iw.Close();
            sw.Close();

            sw = new FileStream("input//LEURFV.xml", FileMode.Open);
            iw = CIReportExcel.createworkbook(sw);
            Debug.Assert(iw == null);
            sw.Close();
        }
        /// <summary>
        /// check this test result need check excel file in Debug.
        /// </summary>
        void test_generateExcel()
        {
            List<LEU> leus = new List<LEU>();
            List<LEU_filtered_values.leu> leuinfos = new List<LEU_filtered_values.leu>();
            for (int i = 0; i < 5; ++i)
            {
                LEU leu = new LEU($"leu{i}", i, $"CI{i % 2}");
                leus.Add(leu);
                LEU_filtered_values.leu leuinfo = FileLoader.Load<LEU_filtered_values.leu>("input//LEURFV.xml");
                leuinfo.NAME = new StringData(leu.Name);
                leuinfos.Add(leuinfo);
            }
            CIReportExcel excel = new CIReportExcel("..//..//..//BMGenTool//bin//Debug//Config//CI-LEU一致性测试报告 CI-LEU correspondence test report.xlsx",
                ".//", leus, leuinfos);

            using (FileStream sw = new FileStream("CI-LEU一致性测试报告 CI-LEU correspondence test report.xlsx", FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workBook = CIReportExcel.createworkbook(sw);

                for (int pagei = 0; pagei < 2; ++pagei)
                {
                    ISheet sheet = workBook.GetSheetAt(workBook.NumberOfSheets - 1 - pagei);
                    for (int rowi = 16; rowi < 30; ++rowi)
                    {
                        IRow row = sheet.GetRow(rowi);
                        if (null != row && row.Cells.Count() == 8)
                        {
                            haschecked = true;
                            ICell cell = row.GetCell(0);
                            Debug.Assert(0 == cell.StringCellValue.IndexOf("leu"));
                        }
                    }
                    Debug.Assert(haschecked == true);
                    haschecked = false;
                }

                workBook.Close();
                sw.Close();
            }
        }

        //check get sheets num of same CIName
        void test_getdataprocess()
        {
            List<LEU> leus = new List<LEU>();
            CIReportExcel excel = new CIReportExcel("", "", leus, null);

            Debug.Assert(excel.getsheetnum() == 0);
            for (int cin = 1; cin < 50; ++cin)
            {
                for (int i = 0; i < 500; ++i)
                {
                    LEU leu = new LEU("leu1", i, $"CI{i % cin}");
                    leus.Add(leu);
                }
                MethodHelper.InvokePrivateMethod<CIReportExcel>(excel,"getdataprocess", new object[] { leus, null, "" });

                Debug.Assert(excel.getsheetnum() == cin);
            }

            //lack test to check the record count
        }

        /// <summary>
        /// if read tmgfile right input valid
        /// if read tgmfile error input invalid
        /// </summary>
        void test_readcompiledmsg()
        {
            string[] msginfile = {
                "E9 C1 F6 4D D1 B5 E0 54 55 81 30 A2 8B 14 25 F9 ED EB 96 60 58 4F BE 36 50 C9 B1 03 D4 FC D8 78 AB 18 35 9E C4 7A 89 60 9A 75 E5 66 67 A7 DE 3B 04 1F 0B EF B5 CF 9D F2 18 FB 24 F3 E0 F4 FB 8D 89 A6 C2 69 C2 A7 2D 1A A3 94 D8 7A 11 32 85 AD FE 23 18 AC 9A B5 86 0E 2B A1 8E 47 CC 25 84 E1 72 4B E7 73 A5 6B C3 7B C0 B3 05 9A 2D 6D 8F C2 F2 80 90 12 8C EA 66 12 93 BF D0 66 35 41 B1 B4",
                "F7 CA 66 9F 25 25 E9 E7 97 D1 B1 32 DB A4 E7 69 2E 4B 3A 9C 71 64 63 A8 98 C4 BA 50 9B 02 52 6A D0 0E 4C 70 D4 8E A4 B1 53 CA CB 43 8E 72 ED E0 C0 8F 7C 14 51 91 BD 0D A4 95 8A C0 FF 20 77 5C 2E C7 8E AB 1D 49 61 13 CB EA F8 C9 39 B1 1A C3 DA E1 DB 11 78 84 36 14 AD E6 15 72 C0 B6 E6 22 57 2E 59 81 16 9C 47 BA 99 9B 70 82 D9 34 0A D8 14 30 90 31 FC 42 F6 3A 77 D2 CD DE A7 63 E4 22",
                "5F DE 75 49 FB D8 36 A6 E7 CE E9 CD 5C C8 5E 99 7A 39 1A 62 D7 79 D5 50 14 75 10 3D 87 03 09 86 1D D2 61 4B 33 DB 0F 47 30 D6 ED 16 C4 86 97 3F 3A FA DE 12 CD EF BB 52 1A C1 FB 97 03 75 2E F2 AD B7 E7 89 40 85 1B 18 35 88 B2 C9 0A F1 62 24 43 5E 72 9F 43 8A 89 3E 93 24 F7 DB F9 36 46 2C 17 83 ED 43 14 39 AD DA 6F 0B A9 CE CF 6B 0A 36 AB D8 90 1C 5D AB D0 76 F8 75 1E 45 F8 6D 07 D6",
                "8F C3 F1 68 4C 33 FC 41 68 8A 4C EA 86 41 75 AF 0C 5D BA 2B 79 5A 4A 07 63 5D 38 DF 77 E1 F0 C1 5D 6D E1 8F 3A 6C 1F 4B C6 6B 27 5A 98 4C 8D C6 EE B9 E7 E3 43 84 FE 82 70 38 D2 4B E7 48 7F 8B 6C 12 6E 27 95 F6 07 4B 5F 62 0E 09 88 0E EB 8B 20 D9 3F 2D 23 C2 2A C1 9B 47 6B BF 26 6D 26 5A A5 DA 14 B9 AE 51 92 A8 12 D9 10 53 12 D1 82 A9 61 D5 10 10 D0 34 02 36 49 9C 53 C0 D6 E0 F8 E2"
            };
            ushort[] nums = { 0, 30, 5, 127};
            foreach (ushort num in nums)
            {
                List<string> msgs = CIReportExcel.readcompiledmsg("input//telgen_4.TGM", num);
                Debug.Assert(num == msgs.Count);

                int i = 0;
                foreach (string msg in msgs)
                {
                    Debug.Assert(msg == msginfile[i]);

                    if (i < msginfile.Length - 1)
                    {
                        ++i;
                    }
                }
            }

            int[] illegalnums = { -1, 128 };
            foreach (int i in illegalnums)
            {
                List<string> msgs = CIReportExcel.readcompiledmsg("input//telgen_4.TGM", (ushort)i);
                Debug.Assert(0 == msgs.Count);
            }
        }

        //get msg from 1 beacon
        //last-1 default record, has default nums cols
        //last one is null record
        //other are msg record, has msg nums cols
        void test_getrecordfrombeacon()
        {
            CIReportExcel excel = new CIReportExcel("","",null,null);

            LEU_filtered_values.leu leu = FileLoader.Load<LEU_filtered_values.leu>("input//LEURFV.xml");
            foreach (LEU_filtered_values.leu.BEACON b in leu.beaconList)
            {
                List<List<string>> rows = CIReportExcel.getrecordfrombeacon(b, "leu1", "input", false);
                int nullidx = rows.Count - 1;
                int defaultidx = nullidx - 1;

                Debug.Assert(3 <= rows.Count);
                for (int i=0; i<rows.Count; ++i)
                {
                    if (i == nullidx)
                    {
                        Debug.Assert(rows[i] == null);
                        continue;
                    }
                    else
                    {
                        Debug.Assert(rows[i][0] == "leu1");
                        Debug.Assert(rows[i][1] == b.NAME);
                        Debug.Assert(rows[i][2] == b.ID);
                        Debug.Assert(rows[i][3] == b.Variants_inputs);

                        if (i == defaultidx)
                        {
                            Debug.Assert(rows[i].Count == CIReportExcel.defaultmsgcols.Count());
                            Debug.Assert(rows[i][4] == b.msgList[0].getReportMsgString());
                        }
                        else
                        {
                            Debug.Assert(rows[i].Count == CIReportExcel.cols.Count());
                            Debug.Assert(rows[i][4] == b.msgList[i + 1].Combined_sections.getReportString());
                            Debug.Assert(rows[i][5] == b.msgList[i + 1].VarState);
                            Debug.Assert(rows[i][6] == b.msgList[i+1].getReportMsgString());
                        }
                    }
                }
            }
        }
    }
}