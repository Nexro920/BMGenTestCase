using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

//using Summer.System.IO;
using MetaFly.Summer.IO;
using BMGenTool.Info;

namespace BMGenTool.Common
{
    public class DataOpr
    {
        [DllImport("Config\\Scram_Tel.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        static extern private bool ScrambleTel(byte[] usrTel, byte[] tel);

        [DllImport("Config\\Sacem.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        extern private static void CallSacem(int[] data, int[] checksum);
        
        public static void PackCallSacem(int[] data, ref int[] checksum)
        {
            int[] checkSum2 = new int[2];
            CallSacem(data, checkSum2);
            //this is in REF5
            checksum = Sys.TransferChk(checkSum2);
        }

        public static bool PackCallScram_Tel(byte[] usrTel, byte[] tel)
        {
            return ScrambleTel(usrTel, tel);
        }
        

        public static string Byte2string(List<byte> dataList)
        {
            string str = "";
            foreach (byte data in dataList)
            {
                string strData = Convert.ToString(data, 16);
                if (1 == strData.Length)
                {
                    str += " 0" + strData;
                }
                else
                {
                    str += " " + strData;
                }
            }
            if(str.StartsWith(" "))
            {
                str = str.Remove(str.IndexOf(" "), 1);
            }
            if (str.EndsWith(" "))
            {
                str = str.Remove(str.LastIndexOf(" "));
            }            
            return str.ToUpper();
        }

        public static string Byte2string(byte[] dataList)
        {
            List<byte> list = dataList.ToList();
            return DataOpr.Byte2string(list);
        }

        public static byte[] String2byte(string data)
        {
            string[] splits = data.Split(' ');
            List<byte> list = new List<byte>();
            foreach(string node in splits)
            {
                byte bData = Convert.ToByte(node, 16);
                list.Add(bData);
            }
             return list.ToArray();
        }
        /// <summary>
        /// output number str of no fraction
        /// eg input    89  89,0    89,0   89,00  89.  89.0    89.00   89.000
        ///    output   8900
        /// eg input 89,001 raise exception
        /// </summary>
        /// <param name="floatstr">input numstr of fraction max[0.01]</param>
        /// <returns></returns>
        public static string Multi100(string floatstr)
        {
            string[] parts = floatstr.Trim().Split(new char[] { ',', '.' });

            string kpval = "";
            if (1 == parts.Length)
            {
                kpval = "00";//transfer to cm, so only need 2
            }
            else if (2 == parts.Length)
            {
                string frac = parts[1];
                kpval = parts[1].PadRight(2, '0').Substring(0,2);//transfer to cm, so only need 2
                if (frac.TrimEnd('0') != kpval.TrimEnd('0'))
                {
                    throw new Exception($"csv ERROR: kp=[{floatstr}] is invalid. format as [0.01]");
                }
            }
            else
            {
                throw new Exception($"csv ERROR: kp=[{floatstr}] is invalid. format as [0.01]");
            }

            kpval = parts[0] + kpval;

            kpval = kpval.TrimStart('0');
            if ("" == kpval)
            {
                return "0";
            }

            return kpval;//return string value of kp union cm
        }

    }
}
