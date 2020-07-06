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
using BMGenTest;

namespace BMGenTool.Info
{
    /// <summary>
    /// case0100
    /// test for the whole process
    /// </summary>
    public class TC0100 : AbstractTestCase
    {
        private bool haschecked = false;
        public override int CurTestCaseID => 100;
        public override void Run()
        {
            test_Generate("ZJ");
            test_Generate("HHHT2");
            test_Generate("TestLine");
            Console.WriteLine($"Run case {CurTestCaseID} for the whole process, PASS!");
        }

        public void test_Generate(string lineInfo)
        {
            File.Copy($".\\input\\0100input_{lineInfo}\\config.xml", ".\\Config\\config.xml", true);
            BMGenTest.BMGenTool program = new BMGenTest.BMGenTool();
            program.Generate();

            string leuoutputpath = $".\\0100output_{lineInfo}\\LEUBinary";
            Debug.Assert(Directory.Exists(leuoutputpath));
            DirectoryInfo dir = new DirectoryInfo(leuoutputpath);

            string rightdir = $".\\input\\0100input_{lineInfo}\\standardoutput\\LEUBinary\\";
            if (Directory.Exists(rightdir))
            {
                foreach (var leuout in dir.GetDirectories())
                {
                    string filename = "\\" + leuout.Name + ".xml";
                    Debug.Assert(TestCase.LEU.checkLEUXmlInfo(leuout.FullName + filename, rightdir + leuout.Name + filename));
                }
            }
            else
            {
                Debug.Assert(false, $"no right output in {rightdir}");
            }
           
            string beaconoutputpath = $".\\0100output_{lineInfo}\\Beacon";
            Debug.Assert(Directory.Exists(beaconoutputpath));
            dir = new DirectoryInfo(beaconoutputpath);
            rightdir = $".\\input\\0100input_{lineInfo}\\standardoutput\\Beacon\\";
            if (Directory.Exists(rightdir))
            {
                int count = 0;
                foreach (var bf in dir.GetFiles("*.xml"))
                {
                    Debug.Assert(TestCase.Balise.checkBaliseXmlInfo(bf.FullName, rightdir + bf.Name));
                    ++count;
                }
                DirectoryInfo dirright = new DirectoryInfo(rightdir);
                Debug.Assert(count == dirright.GetFiles("*.xml").Count());
            }
            else
            {
                Debug.Assert(false, $"no right output in {rightdir}");
            }
        }
    }

    public static class test100Extend
    {
        public static void Generate(this BMGenTest.BMGenTool instance)
        {
            MethodHelper.InvokePrivateMethod<BMGenTest.BMGenTool>(instance, "Generate");
        }
    }
}