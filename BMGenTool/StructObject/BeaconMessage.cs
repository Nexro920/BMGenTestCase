using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MetaFly.Summer.Generic;

using BMGenTool.Info;
using BMGenTool.Common;
using System.Runtime.InteropServices;

namespace BMGenTool.Info
{
    public class MSG_CONSTANT
    {
        //const data in head
        public const string Q_UPDOWN = "1";
        public const string M_VERSION = "0010000";
        public const string Q_MEDIA = "0";
        public const string N_PIG = "000";
        public const string N_TOTAL = "000";
        public const string M_DUP = "00";
        public const string M_MCOUNT_NOBM = "11111111";
        public const string M_MCOUNT_BMDEFAULT = "11111100";
        public const string M_MCOUNT_LEUDEFAULT = "00000000";
        public const string M_MCOUNT_LEU = "11111111";
        public const string Q_LINK = "0";
        //const data in user info
        public const string NID_PACKET = "00101100";
        public const string Q_DIR = "01";
        //ETCS-44 package map version message
        public const string L_PACKET_VERMSG = "0000000110000";//48;
        public const string NID_XUSER_ETCS44 = "011001010";//202;

        //ETCS-44 package of BM
        public const string NID_XUSER_BMETCS44 = "011001011";//203;

        public const int L_PACKET_COMMONMSG = 124;
        public const int L_PACKET_USER = 772;
        public const int L_PACKET_ITC = 29;
        public const int L_PACKET_INTEROPERABLE = 832;
    }
    public class BeaconMessage
    {
        private GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON bibbm = null;
        private int BMB_Distance_Unitcm = 0;
        private Message msg = null;

        private int lineID = 0;
        private int beaconID = 0;
        private bool isVB;
        private int version = 0;
        public BeaconMessage()
        {
        }

        private void setValue(int lID, IBeaconInfo layout)
        {
            lineID = lID;
            beaconID = layout.ID;
            isVB = layout.IsVariantBeacon();
            version = layout.getVersion();
        }
        
        private string dataTobitStr(long data, int bitNum)
        {
            string buff = Convert.ToString(data, 2);
            if (buff.Length <= bitNum)
            {
                return buff.PadLeft(bitNum, '0');
            }
            else
            {
                return buff.Remove(0, buff.Length - bitNum);
            }
        }
        [DllImport("./Config/Scram_Tel.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool ScrambleTel(byte[] UserTelegram, byte[] Telegram);
        [MarshalAs(UnmanagedType.LPArray)]
        public static byte[] AfterCoding;
        [MarshalAs(UnmanagedType.LPArray)]
        private byte[] byteList;
        private string bitStrTohexStr(string bitStr)
        {
            if (0 != bitStr.Length % 8)
            {
                TraceMethod.RecordInfo($"Error: generate BM Message len={bitStr.Length} % 8 != 0, which is invalid!");
            }
            System.Text.RegularExpressions.CaptureCollection cs =
                System.Text.RegularExpressions.Regex.Match(bitStr, @"([01]{8})+").Groups[1].Captures;
            //byte[] byteList = new byte[cs.Count];

            byteList = new byte[cs.Count];

            string hexBuff = "";
            for (int i = 0; i < cs.Count; ++i)
            {
                byteList[i] = Convert.ToByte(cs[i].Value, 2);
                hexBuff += Convert.ToString(byteList[i], 16).ToUpper().PadLeft(2, '0') + " ";
            }
            
            //AfterCoding = new byte[128];           
            //ScrambleTel(byteList, AfterCoding);
            
            hexBuff = hexBuff.Remove(hexBuff.Length-1, 1);
            return hexBuff;
        }

        public string GenerateMessage(BEACON Beacon, int LineID, Message Msg)
        {
            this.msg = Msg;
            this.bibbm = Beacon.m_ibbmInfo;
            this.BMB_Distance_Unitcm = Beacon.BMB_Distance_cm;
            setValue(LineID, Beacon.m_layoutInfo);
            return GenerateMessage();
        }
        public string GenerateMessage(IBeaconInfo layout, int lineID)
        {
            if (null == layout)
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, "GenerateMessage Error:input null beacon"); ;
                return "";
            }
            setValue(lineID, layout);
            return GenerateMessage();
        }

        private string GenerateMessage()
        { 
            //50_bits for head
            string buff = CalHeadTel();

            //772-bits
            buff += CalUserTel();

            //8-bits 1 means end //BMGR-0013
            //add 2-bits 0 extra. 830-> 832 = 104 * 8 //BMGR-0032
            buff += "1111111100";

            if (buff.Length != 832)
            {
                throw new Exception(string.Format("GenMsgiTranavi generate message count[{0}] is not 832", buff.Length));
            }

            return bitStrTohexStr(buff);
        }

        /// <summary>
        /// 计算互联互通项目beacon报文头，共50个bit
        /// </summary>
        /// <param name="beacon"></param>
        /// <returns></returns>
        /// //BMGR-0007 BMGR-0053
        private string CalHeadTel()
        {
            string head50bits = "";

            head50bits =
                MSG_CONSTANT.Q_UPDOWN + //1bit
                MSG_CONSTANT.M_VERSION + //2~8bit = 7 
                MSG_CONSTANT.Q_MEDIA + //9bit
                MSG_CONSTANT.N_PIG + //10~12bit = 3
                MSG_CONSTANT.N_TOTAL + // 13~15bit = 3
                MSG_CONSTANT.M_DUP; // 16~17bit = 2

            //18~25bit,M_MCOUNT = 8
            if (false == isVB)
            {
                head50bits += MSG_CONSTANT.M_MCOUNT_NOBM;
            }
            else
            {
                if (null == msg)//beacon default msg
                {
                    head50bits += MSG_CONSTANT.M_MCOUNT_BMDEFAULT;
                }
                else if (0 == msg.GetRank())//leu default msg BMGR-0053 00000000
                {
                    head50bits += MSG_CONSTANT.M_MCOUNT_LEUDEFAULT;
                }
                else//normal msg BMGR-0053 11111111
                {
                    head50bits += MSG_CONSTANT.M_MCOUNT_LEU;
                }
            }

            head50bits += dataTobitStr(lineID, 10) + //26~35bit,NID_L = 10
                          dataTobitStr(beaconID, 14) + //36~49bit,NID_BG = 14
                          MSG_CONSTANT.Q_LINK; //50bit

            if (50 != head50bits.Length)
            {
                TraceMethod.RecordInfo("Error of CalHeadTel, the message is not 50 bits!");
            }
            return head50bits;
        }
        /// <summary>
        /// 计算互联互通项目beacon报文用户信息，共772个bit
        /// </summary>
        /// <param name="beacon"></param>
        /// <returns></returns>
        /// 
        private string CalUserTel()
        {
            string bitbuff = "";

            #region[ETCS-44 of map version]
            //BMGR-0053
            {
                bitbuff +=
                MSG_CONSTANT.NID_PACKET + //1~8bit = 8
                MSG_CONSTANT.Q_DIR + //9~10bit = 2
                MSG_CONSTANT.L_PACKET_VERMSG + //BMGR-0010 11~23bit,L_PACKET = 13
                MSG_CONSTANT.NID_XUSER_ETCS44 + // 24~32bit,NID_XUSER = 9
                dataTobitStr(version, 16);//33~48bit,M_EDITION
            }
            #endregion

            #region[ETCS-44 of common info]
            //BMGR-0008 only variant Beacon has common info
            if (true == isVB)
            {
                bitbuff +=
                MSG_CONSTANT.NID_PACKET +//49~56bit = 8
                MSG_CONSTANT.Q_DIR;//57~58bit = 2
                if (null == msg)
                {
                    bitbuff +=
                        dataTobitStr(MSG_CONSTANT.L_PACKET_COMMONMSG, 13) + ////BMGR-0009//L_PACKET BMGR-0054 59~71bit = 13
                        MSG_CONSTANT.NID_XUSER_BMETCS44 + ////NID_XUSER//BMGR-0011 72~80bit = 9
                        dataTobitStr(1, 19) + //Q_SIGNAL_ASPECT 81~99bit = 19
                        dataTobitStr(0, 19) + //Q_SIGNAL_ASPECT_PRE 100~118bit = 19
                        "01" +                     //C_CI_LEU C_LEU_BALISE beacon default msg  01 119~120bit= 2
                        dataTobitStr(0, 24 * 2) + //D_DIS + D_DIS_OVERLAP 121~168bit= 24 * 2
                        "0000";//N_SWITCH 169~172bit = 4
                }
                else
                {
                    bitbuff += CalUserTelPart4Msg(msg);
                }
            }
            #endregion

            //BMGR-0006  //BMGR-0012 add 111 for bitnum less than 772
            bitbuff = bitbuff.PadRight(MSG_CONSTANT.L_PACKET_USER, '1');

            return bitbuff;
        }

        //D_DIS，D_DIS_OVERLAP，N_SWITCH BMGR-0058
        private string Generate_D_DIS_D_DIS_OVERLAP(Message msg)
        {
            string buff = "";
            long D_DIS = 0;
            long D_DIS_OVERLAP = 0;

            if (0 == msg.GetRank())
            {
                buff += dataTobitStr(0, 24 * 2);
                return buff;
            }

            if (null != msg.overlap)
            {
                D_DIS_OVERLAP = BMB_Distance_Unitcm + msg.rpRs.GetLength();
                D_DIS = D_DIS_OVERLAP + msg.overlap.GetDistance();
            }
            else
            {
                D_DIS_OVERLAP = 0;
                D_DIS = BMB_Distance_Unitcm;
                if (null != msg.rpRs)
                {
                    D_DIS += msg.rpRs.GetLength();
                }

                if (null != msg.apRs)
                {
                    D_DIS += msg.apRs.GetLength();
                }
            }

            buff += dataTobitStr(D_DIS, 24) +
                    dataTobitStr(D_DIS_OVERLAP, 24);
            return buff;
        }
        private string GenerateA2R(RouteSegment rs, Message msg)
        {
            string A2R = "";

            string R = "1";
            if (null == msg.olPath && null == msg.overlap)
            {
                R = "0";
            }

            string Q = "0";
            string[] P2A = new string[15] { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
            int P2Aidx = 0;
            if (null == rs.m_PtLst
                || 0 == rs.m_PtLst.Count())
            {
                Q = "1";
            }
            else
            {
                string tempQ = "1";
                foreach (PointInfo pt in rs.m_PtLst)
                {
                    if ("Divergent" == pt.Orientation)
                    {
                        if ("Normal" == pt.Position)
                        {
                            P2A[P2Aidx++] = "0";
                        }
                        else
                        {
                            P2A[P2Aidx++] = "1";
                            tempQ = "0";
                        }
                    }
                }
                Q = tempQ;
            }

            if ("1" == Q)
            {
                A2R = "000000000000000";
            }
            else
            {
                for (int i = 14; i >= 0; --i)
                {
                    A2R += P2A[i];
                }
            }

            A2R += Q + R;

            return A2R;
        }
        //BMGR-0056
        private string Generate_Q_SIGNAL_ASPECT(Message msg)
        {
            string Q_SIGNAL_ASPECT = "00";

            if (0 == msg.GetRank()
               || (null == msg.apRs
                    && null == msg.upPath
                    && null == msg.rpRs
                    && null == msg.olPath))
            {
                return "0000000000000000001";
            }

            RouteSegment rs = null;
            if (null != msg.rpRs)
            {
                rs = msg.rpRs;
            }
            else if (null != msg.apRs)
            {
                rs = msg.apRs;
            }

            return Q_SIGNAL_ASPECT + GenerateA2R(rs, msg);
        }

        //BMGR-0057
        private string Generate_Q_SIGNAL_ASPECT_PRE(Message msg)
        {
            string Q_SIGNAL_ASPECT_ORE = "00";

            if (0 == msg.GetRank()
                || 1 == msg.GetRank()
                || (BeaconType.Reopening == bibbm.GetBeaconType())//reopen beacon
                || (BeaconType.Approach == bibbm.GetBeaconType())//approach beacon
                || (null == msg.rpRs && null != msg.apRs))
            {
                return "0000000000000000000";
            }

            if ((BeaconType.Reopening_Approach == bibbm.GetBeaconType())
               && null == msg.apRs)
            {
                return "0000000000000000001";
            }

            if (null != msg.rpRs && null != msg.apRs)
            {
                return Q_SIGNAL_ASPECT_ORE + GenerateA2R(msg.apRs, msg);
            }

            return "";
        }
        //BMGR-0055
        private string CalUserTelPart4Msg(Message msg)
        {
            string buff = "";

            List<PointInfo> ptList = msg.GetAllPoints();

            //L_PACKET BMGR-0054
            int L_PACKET_C = ptList.Count() * 18;
            buff += dataTobitStr(MSG_CONSTANT.L_PACKET_COMMONMSG + L_PACKET_C, 13) +
                MSG_CONSTANT.NID_XUSER_BMETCS44 + //NID_XUSER BMGR-0055
                Generate_Q_SIGNAL_ASPECT(msg) + //BMGR-0057
                Generate_Q_SIGNAL_ASPECT_PRE(msg);

            //C_CI_LEU
            //C_LEU_BALISE 
            if (0 == msg.GetRank())//leu default msg  10
            {
                buff += "10";
            }
            else//normal msg  00
            {
                buff += "00";
            }

            //D_DIS，D_DIS_OVERLAP BMGR-0058
            buff += Generate_D_DIS_D_DIS_OVERLAP(msg);
            
            //N_SWITCH BMGR-0059
            if (0 == msg.GetRank())
            {
                buff += "0000";
                return buff;
            }

            //N_SWITCH BMGR-0059
            buff += dataTobitStr(ptList.Count(), 4);

            foreach (PointInfo pt in ptList)
            {
                buff += dataTobitStr(pt.Point.Interoperable_ID, 16);
                buff += dataTobitStr(pt.GetPostionInt(), 2);
            }

            return buff;
        }
        //BMGR-0004 BMGR-0052
        //public string GenMsgItc(string msgVstate = "")
        //{
        //    Byte[] msgByte = new Byte[29]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //    int[] data = new int[19]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };//used to cal checksum

        //    //beacon of bigendian
        //    msgByte[0] = (Byte)(m_layoutInfo.ID >> 8);
        //    msgByte[1] = (Byte)m_layoutInfo.ID;

        //    data[0] = m_layoutInfo.ID;

        //    if (false == IsNoVariant)
        //    {
        //        if ("" == msgVstate)//BMGR-0004 defalut msg is 01
        //        {
        //            msgByte[6] = 01;
        //            data[17] = 1;
        //        }
        //        else////BMGR-0052 normal msg
        //        {
        //            msgVstate = msgVstate.Replace('S', '0');
        //            msgVstate = msgVstate.Replace('P', '0');
        //            msgVstate = msgVstate.Replace(" ", "");

        //            //variant state of bigendian
        //            msgByte[2] = (byte)DataOpr.BitString2Int(msgVstate.Substring(0, 8));
        //            msgByte[3] = (byte)DataOpr.BitString2Int(msgVstate.Substring(8, 8));

        //            msgByte[7] = 01;

        //            int i = 1;
        //            foreach (char c in msgVstate)
        //            {
        //                data[i++] = int.Parse(c.ToString());
        //            }
        //            data[17] = 0;
        //            data[18] = 1;
        //        }
        //    }

        //    ////21~28 are 2 checksums
        //    int[] chk = new int[2];
        //    DataOpr.PackCallSacem(data, ref chk);

        //    //bit 21~24 is checkcum1 bigendian
        //    DataOpr.add2Buff_BigEndian(chk[0], ref msgByte, 21, 4);
        //    //bit 25~28 is checkcum2 bigendian
        //    DataOpr.add2Buff_BigEndian(chk[1], ref msgByte, 25, 4);

        //    return DataOpr.Byte2string(msgByte);
        //}
        
    }
}
