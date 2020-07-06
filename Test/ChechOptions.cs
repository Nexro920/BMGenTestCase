using System;
using System.Collections.Generic;
using System.Linq;
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
    public class Check
    {
        public static void CheckFileExistThenDelete(string filefullname)
        {
            Debug.Assert(File.Exists(filefullname));
            File.Delete(filefullname);
        }

        public static void CompareXmlFile(string xml, string xmlRight, bool ignoreNote = true)
        {
            XmlVisitor node1 = XmlFileHelper.CreateFromFile(xml).GetRoot();
            XmlVisitor rightnode = XmlFileHelper.CreateFromFile(xmlRight).GetRoot();
            Console.WriteLine($"now lack implement of CompareXmlFile {xml} {xmlRight}");
        }

    }
}