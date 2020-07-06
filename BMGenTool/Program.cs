using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using BMGenTool.Common;

namespace BMGenTest
{
    static public class Program
    {
        //this used for tongji format generate
        public static bool GenerateTJFormat = false;
        //this used for auto test
        public static bool AUTOTEST = false;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        /// 
        [STAThread]
        static public void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if ("TJFormat" == args[0])
                {
                    GenerateTJFormat = true;
                }
                else if ("test" == args[0])
                {
                    AUTOTEST = true;
                }
            }

            if (AUTOTEST)
            { 
                BMGenTool testH = new BMGenTool();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new BMGenTool());
            }
        }
    }
}
