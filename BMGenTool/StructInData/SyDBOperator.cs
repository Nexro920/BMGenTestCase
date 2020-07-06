using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using MetaFly.Summer.Generic;

namespace BMGenTool.Info
{
    public static class BeaconType
    {
        public const string Unknown = "Unknown";
        public const string Approach = "Approach";
        public const string Reopening = "Reopening";
        public const string Reopening_Approach = "Reopening_Approach";
        public const string Invalid = "Invalid";
    }

    public class Sys
    {
        enum DIR
        {
            UP,
            DOWN
        }
        public static string StrFormat = "f3";
        
        public static string Up = "Up";
        public static string Down = "Down";

        //beacon relation to signal
        public static string Reopening = "Reopening";
        public static string Approach = "Approach";

        //signal type
        public static string TYPEFUNC_ROUTESPACING = "Route Spacing Signal";
        public static string TYPEFUNC_SPACING = "Spacing Signal";

        public static string Normal = "Normal";
        public static string Reverse = "Reverse";
        public static string Both = "Either";

        public static string[] PointPositions = new string[] { Normal, Reverse };

        public static string SignalType = "SIGNAL";
        public static string PointNType = "POINT_N";
        public static string PointRType = "POINT_R";
        public static string UNomal = "_N";
        public static string UReverse = "_R";
        public static string Signal = "Signal";
        public static string UPPoint = "UP_Point";
        public static string RSPoint = "RS_Point";
        public static string OLPoint = "OL_Point";
        public static string Convergent = "Convergent";
        public static string Divergent = "Divergent";
        public static string[] PointOrients = new string[] { Convergent, Divergent };

        public static int GetSDDBLenOfLocatedBlock(KP_V p, GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK locatedBlk, string dir)
        {
            int length = 0;
            if (dir == Sys.Down)
            {
                length = Math.Abs(p.KpRealValue - locatedBlk.Kp_Begin);
            }
            else if (dir == Sys.Up)
            {
                length = Math.Abs(locatedBlk.Kp_End - p.KpRealValue);
            }
            else
            {
                throw new Exception($"Invalid dir {dir}");
            }
            return length;
        }
        public enum SddbInBlock
        {
            none,
            start,
            end
        }
        public static SddbInBlock SddbWalkThroughBlock(GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK blk, string dir)
        {
            //block_begin to end is up dir
            if (dir == Sys.Down)
            {// go down then meet up first, so judge up first
                if (null != blk.Up_Secondary_Detection_Device_Boundary_ID)
                {
                    return SddbInBlock.start;
                }
                else if (null != blk.Down_Secondary_Detection_Device_Boundary_ID)
                {
                    return SddbInBlock.end;
                }
            }
            else if (dir == Sys.Up)
            {
                if (null != blk.Down_Secondary_Detection_Device_Boundary_ID)
                {
                    return SddbInBlock.start;
                }
                else if (null != blk.Up_Secondary_Detection_Device_Boundary_ID)
                {
                    return SddbInBlock.end;
                }
            }
            else
            {
                throw new Exception($"Invalid dir {dir}");
            }
            return SddbInBlock.none;
        }

        public static SddbInBlock GetSDDBPosInLocatedBlock(GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK blk, string dir)
        {
            //block_begin to end is up dir
            if (dir == Sys.Down)
            {// go down then meet up first, so judge up first
                if (null != blk.Down_Secondary_Detection_Device_Boundary_ID)
                {
                    return SddbInBlock.end;
                }
            }
            else if (dir == Sys.Up)
            {
                if (null != blk.Up_Secondary_Detection_Device_Boundary_ID)
                {
                    return SddbInBlock.end;
                }
            }
            else
            {
                throw new Exception($"Invalid dir {dir}");
            }
            return SddbInBlock.none;
        }

        public static void NewEmptyPath(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                Directory.CreateDirectory(path);
            }
            catch (System.Exception ex)
            {
                string logMsg = string.Format("Exception while deal directory! {0}", ex.Message);
                TraceMethod.RecordInfo(logMsg);
            }
        }

        public static Node GetNode(string name, List<Node> list)
        {
            foreach (Node node in list)
            {
                if (node.Name == name)
                {
                    return node;
                }
            }
            TraceMethod.RecordInfo(string.Format("Error: can't find node whose name={0} in sydb", name));
            return null;
        }

        public static Node GetNode(int id, List<Node> list)
        {
            foreach (Node node in list)
            {
                if (node.ID == id)
                {
                    return node;
                }
            }
            TraceMethod.RecordInfo(string.Format("Error: can't find node whose id={0} in sydb", id));
            return null;
        }

        public static double Cm2Meter(int cm, int decimalNum)
        {
            double dCm = (double)cm;
            double meter = Convert.ToDouble(dCm / 100);
            return Math.Round(meter, decimalNum);
        }

        public static int Meter2Cm(double m)
        {
            double dCm = m * 1000;
            return (int)(dCm / 10);
        }

        public static int Mm2Cm(double mm)
        {
            return (int)(mm / 10);
        }

        //根据转换规则对SACEM校核字进行转换，参考VBA计算
        public static int[] TransferChk(int[] chk)
        {
            int[] result = new int[2];
            int ch1 = chk[0];
            int ch2 = chk[1];
            string str1 = Transfer(Convert.ToString(ch1, 16).ToUpper());
            string str2 = Transfer(Convert.ToString(ch2, 16).ToUpper());
            ch1 = Convert.ToInt32(str1, 16);
            ch2 = Convert.ToInt32(str2, 16);
            result[0] = ch1;
            result[1] = ch2;
            return result;
        }

        public static string Transfer(string str)
        {
            //补足成4个字节
            List<char> chList = str.ToList();
            chList.Reverse();
            for (int i = chList.Count; i < 8; i++)
            {
                chList.Add('0');
            }
            char[] chArr = new char[8];
            for (int i = 0; i < 8; i++)
            {
                char tsCh = '0';
                char ch = chList[i];
                switch (ch)
                {
                    case '0':
                        tsCh = '0';
                        break;
                    case '1':
                        tsCh = '8';
                        break;
                    case '2':
                        tsCh = '4';
                        break;
                    case '3':
                        tsCh = 'C';
                        break;
                    case '4':
                        tsCh = '2';
                        break;
                    case '5':
                        tsCh = 'A';
                        break;
                    case '6':
                        tsCh = '6';
                        break;
                    case '7':
                        tsCh = 'E';
                        break;
                    case '8':
                        tsCh = '1';
                        break;
                    case '9':
                        tsCh = '9';
                        break;
                    case 'A':
                        tsCh = '5';
                        break;
                    case 'B':
                        tsCh = 'D';
                        break;
                    case 'C':
                        tsCh = '3';
                        break;
                    case 'D':
                        tsCh = 'B';
                        break;
                    case 'E':
                        tsCh = '7';
                        break;
                    case 'F':
                        tsCh = 'F';
                        break;
                    default:
                        break;
                }
                chArr[i] = tsCh;
            }
            return new string(chArr);
        }
    }
}
