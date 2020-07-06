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
    public class Prepare
    {
        /// <summary>
        /// clear the old data in sydb, then load new data from input file
        /// </summary>
        /// <param name="filefullname"></param>
        public static void ReloadGlobalSydb(string filefullname)
        {
            GENERIC_SYSTEM_PARAMETERS sydbdata = FileLoader.Load<GENERIC_SYSTEM_PARAMETERS>(filefullname);
            SyDB sydb = SyDB.GetInstance();
            sydb.clear();
            sydb.LoadData(sydbdata);
        }

        public static string getXmlNodeStr(XmlVisitor node, string xpath)
        {
            string buff = "";
            if (node == null || xpath == "")
            {
                return "";
            }
            foreach (var n in node.ChildrenByPath2(xpath))
            {
                buff += n.Value + " ";
            }
            return buff.TrimEnd();
        }
    }
}