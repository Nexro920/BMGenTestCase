using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MetaFly.Summer.IO;
using MetaFly.Summer.Generic;

using BMGenTool.Common;
using MetaFly.Datum.Figure;

namespace BMGenTool.Info
{ 
    public class BEACON
    {
        /// <summary>
        /// each signal has 1 reopen beacon
        /// this can help app beacon find reopen beacon
        /// </summary>
        public static Dictionary<string, string> SignamReBeaconDic = new Dictionary<string, string>();

        public string Info
        {
            get
            {
                return $"{m_layoutInfo.Info}";
            }
        }
        public const int MAXVARNUM = 32;
        //data for beacon input
        public IBeaconInfo m_layoutInfo;
        public GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON m_ibbmInfo;

        public IEnumerable<OriginSignal> GetOrgSignalList()
        {
            if (m_ReopenOrgSig != null)
            {
                yield return m_ReopenOrgSig;
            }
            foreach (OriginSignal b in m_AppOrgSigLst)
            {
                yield return b;
            }
        }
        public string Name
        {
            get { return m_layoutInfo.Name; }
        }

        //calculate
        private int m_BMB_Distance = 0;
        /// <summary>
        /// BMB distance is the distance between reopen beacon and the next SDDB 
        /// </summary>
        public int BMB_Distance_cm
        {
            get { return m_BMB_Distance; }
            set { m_BMB_Distance = value; }
        }

        public List<PathInfo> m_UpstreamLst;

        //this is the reopen signal for a beacon
        public OriginSignal m_ReopenOrgSig;

        //these approach signal is for the beacon which has 1-3 approach signal and a reopen signal
        public List<OriginSignal> m_AppOrgSigLst;

        public List<Variant> m_variantLst;

        public List<Message> m_MsgLst;

        public OriginSignal GetOriginSignalBySignalName(string sigName)
        {
            foreach (OriginSignal sig in GetOrgSignalList())
            {
                if (sig.GetName() == sigName)
                {
                    return sig;
                }
            }
            return null;
        }

        public string m_Type;  // add to save IBBM beacon type

        public BEACON(IBeaconInfo BInfo, GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON ibbm)
        {
            m_layoutInfo = BInfo;
            m_ibbmInfo = ibbm;

            m_UpstreamLst = new List<PathInfo>();
            m_AppOrgSigLst = new List<OriginSignal>();

            //todo now this must be called here
            //ibbm.GetBeaconType();
            m_Type = ibbm.GetBeaconType();
        }
        
        //BMGR-0046
        public bool SetBeaconInfoNode_LEURF(ref XmlVisitor node, int outnum)
        {
            node.UpdateAttribute("ID", m_layoutInfo.ID);
            node.UpdateAttribute("NAME", Name);
            node.UpdateAttribute("TYPE", m_ibbmInfo.GetBeaconType());

            node.UpdateAttribute("NUM", outnum);
            node.UpdateAttribute("VERSION", m_layoutInfo.getVersion());

            node.UpdateAttribute("LINKED_SIGNAL", GetLindedSignalName());

            if ("" == GetLindedSignalName())
            {
                return false;
            }
            return true;
        }

        //BMGR-0015 Block_mode_beacons.Beacon tag
        public bool SetBeaconInfoNode_BMVF(ref XmlVisitor beaconNode)
        {
            beaconNode.UpdateAttribute("NAME", Name);
            beaconNode.UpdateAttribute("TYPE", m_ibbmInfo.GetBeaconType());

            beaconNode.UpdateAttribute("VERSION", m_layoutInfo.getVersion());
            beaconNode.AppendChild("Id", m_layoutInfo.ID);

            //if m_ibbmInfo not valid return false
            if (null == m_ibbmInfo)
            {
                TraceMethod.RecordInfo($"Error:Beacon[{Info}] has on ibbm input struct");
                //log error
                return false;
            }

            beaconNode.AppendChild("Direction", m_ibbmInfo.Direction);
            if (true == m_ibbmInfo.Is_Guaranted_Beacon)
            {
                beaconNode.AppendChild("Is_guaranted_BM_beacon", null);
            }
            return true;
        }
        //BMGR-0047
        public string GetVariantsInputs()
        {
            //string[] VarInputs = new string[32]{"0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
            string[] VarInputs = new string[BEACON.MAXVARNUM];
            for (int i = 0; i < VarInputs.Count(); i++)
            {
                VarInputs[i] = "0";
            }
            foreach (Variant var in m_variantLst)
            {
                if (var.m_Idx > BEACON.MAXVARNUM)
                {
                    continue;
                }
                if(-1 != var.InputRank)
                {
                    VarInputs[var.m_Idx - 1] = var.InputRank.ToString("X");
                }
            }

            string buffer = VarInputs[0];
            for(int i=1; i<VarInputs.Count(); ++i)
            {
                buffer += " " + VarInputs[i]; 
            }

            return buffer;
        }

        //BMGR-0046
        public string GetLindedSignalName()
        {
            if (null != m_ReopenOrgSig)
            {
                return m_ReopenOrgSig.GetName();
            }
            else if (m_AppOrgSigLst.Count() > 0)
            {
                return m_AppOrgSigLst[0].GetName();
            }

            TraceMethod.RecordInfo(string.Format("Beacon[{0}] has no signal. error in GetLindedSignalName", Name));
            return "ErrorNoLinkedSignal";
        }
    }

    public class LEU
    {
        public string CI_Name { get; }
        public string Name { get; set; }
        public int ID { get; set; }

        public string Info
        {
            get
            {
                return $"LEU {Name}-{ID}";
            }
        }

        //the beacon name releated to the LEU
        private string[] beaconNames = new string[4] { "", "", "", "" };

        public LEU(string name, int id, string ciname)
        {
            this.Name = name;
            this.ID = id;
            this.CI_Name = ciname;
        }
        public string[] GetBeaconNames()
        {
            return beaconNames;
        }
        
        public int AddBeacon(int BeaconOutNum, string beaconName)
        {
            if (BeaconOutNum < 1 || BeaconOutNum > 4)
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, $"sydb file data error: Beacon[{beaconName}] IBBM.BM_Beacon.LEU.@Beacon_Output_Nb={BeaconOutNum} not in [1,4]");
                return -1;
            }
            //check if LEU.BeaconOutNum is repeated
            if ("" != beaconNames[BeaconOutNum-1])
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR,
                    $"sydb.IBBM data error: Beacon[{beaconNames[BeaconOutNum - 1]}] and Beacon[{beaconName}] has same LEU={Name} Beacon_Output_number={BeaconOutNum}");
                return -2;
            }

            beaconNames[BeaconOutNum - 1] = beaconName;
            return 0;
        }
    }

}
