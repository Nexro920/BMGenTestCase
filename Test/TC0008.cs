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
    /// test for LEUXmlGen.cs
    /// lack test for iTC files
    /// </summary>
    public class TC0008 : AbstractTestCase
    {
        private bool haschecked = false;
        public override int CurTestCaseID => 008;
        SyDB sydb = SyDB.GetInstance();
        public override void Run()
        {
            test_InitErr();
            test_GenLEUXmlFile();
            test_GenerateBin();
            Console.WriteLine($"Run case {CurTestCaseID} for LEUXmlGen.cs, PASS!");
        }

        void test_InitErr()
        {
            //no input LEURF file
            LEUXmlGen gen = new LEUXmlGen(null, "", "./", false, true);
            Debug.Assert(false == gen.Init());
            //Arrange compiler
            MethodHelper.ModifyFieldValue(gen, "leuComPath", "input//run.exe");
            Debug.Assert(false == gen.Init());
            //Arrange LEUTemplate file
            gen = new LEUXmlGen(null, "input//0008//LEU_Result_Filtered_Values.xml", "./", false, true);
            MethodHelper.ModifyFieldValue(gen, "leuTF", "input//0008//LEU_Result_Filtered_Values.xml");
            Debug.Assert(false == gen.Init());
            //Arrange GID
            gen = new LEUXmlGen(null, "input//0008//LEU_Result_Filtered_Values.xml", "./", false, true);
            MethodHelper.ModifyFieldValue(gen, "gidTable", "input//0008//LEU_Result_Filtered_Values.xml");
            Debug.Assert(false == gen.Init());
            //Arrange GID
            MethodHelper.ModifyFieldValue(gen, "gidTable", "input//0008//GID-Table-notexist.txt");
            Debug.Assert(false == gen.Init());
            //Arrange GID
            MethodHelper.ModifyFieldValue(gen, "gidTable", "input//0008//GID-Table-err.txt");
            Debug.Assert(false == gen.Init());
            //Arrange GID
            MethodHelper.ModifyFieldValue(gen, "gidTable", "input//0008//GID-Table-less.txt");
            Debug.Assert(false == gen.Init());
        }

        void test_GenLEUXmlFile()
        {
            //Arrange
            List<LEU> leulist = new List<LEU>();
            LEUXmlGen gen = new LEUXmlGen(leulist, "input//0008//LEU_Result_Filtered_Values.xml", "./", false, false);
            Debug.Assert(true == gen.Init());

            string leufilefullname = "leufile.xml";

            foreach (var leu in gen.LeuInfoList())
            {
                //Act
                gen.GenLEUXmlFile(leu, new GID("1","2","3"), leufilefullname);

                //Assert
                Debug.Assert(File.Exists(leufilefullname));
                string xmlrightfullname = string.Format($"input//0008//{leu.NAME}.xml");
                Check.CompareXmlFile(leufilefullname, xmlrightfullname);
                File.Delete(leufilefullname);
            }
        }

        void test_GenerateBin()
        {
            //Arrange
            List<LEU> leulist = new List<LEU>();
            
            string xmlfullname = string.Format($"input//0008//E1D.xml");
            List<LEU_filtered_values.leu.BEACON> beaconlist = new List<LEU_filtered_values.leu.BEACON>();
            for (int i = 1; i < 5; i++)
            {
                LEU_filtered_values.leu.BEACON beacon = new LEU_filtered_values.leu.BEACON();
                beacon.NUM = new StringData(i.ToString());
                beaconlist.Add(beacon);
            }
            foreach (bool isItc in new bool[] { false, true })
            {
                LEUXmlGen gen = new LEUXmlGen(leulist, "", "./", isItc, false);
                string outputdir = "output8";
                Directory.CreateDirectory(outputdir);
                Debug.Assert(isItc != gen.GenerateBin(xmlfullname, outputdir, beaconlist));
                Directory.Delete(outputdir, true);
            }

        }
    }

    public static class test8Extend
    {
        public static bool Init(this LEUXmlGen instance)
        {
            return (bool)MethodHelper.InvokePrivateMethod<LEUXmlGen>(instance, "Init");
        }

        public static bool GenLEUXmlFile(this LEUXmlGen instance, LEU_filtered_values.leu leurf, GID gid, string filename)
        {
            return (bool)MethodHelper.InvokePrivateMethod<LEUXmlGen>(instance, "GenLEUXmlFile", new object[] { leurf, gid, filename });
        }
        public static bool GenerateBin(this LEUXmlGen instance, string filename, string outDir, List<LEU_filtered_values.leu.BEACON> list)
        {
            return (bool)MethodHelper.InvokePrivateMethod<LEUXmlGen>(instance, "GenerateBin", new object[] { filename, outDir, list });
        }

        public static List<LEU_filtered_values.leu> LeuInfoList(this LEUXmlGen instance)
        {
            return (List<LEU_filtered_values.leu>)MethodHelper.InvokePrivateMember(instance, "LeuInfoList");
        }
    }
}