using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MetaFly.Datum.Figure;
using MetaFly.Serialization;
using MetaFly.Summer.Generic;

namespace BMGenTool.Info
{
    public class NodeApi
    {
        public static string getNameNullSafe(Node n)
        {
            if (n == null || null == n.Name)
            {
                return "Null";
            }
            return n.Name;
        }

        public static string getIDNullSafe(Node n)
        {
            if (n == null || null == n.ID)
            {
                return "Null";
            }
            return n.ID;
        }
    }
    public interface Node:IInfo
    {
        StringData Name { get; set; }

        StringData ID { get; set; } 
    }
    public class INPUT_INFO:IInfo
    {
        public virtual string Info
        {
            get;
        }
        [XmlAttribute]
        public StringData RANK { get; set; }

        [XmlAttribute]
        public StringData Name { get; set; }

        [XmlAttribute]
        public StringData Type { get; set; }
    }
    
    //value of kp. union is cm
    public class KP_V
    {
        //in union of cm, correspond to sydb KP
        public int KpRealValue
        {
            get
            {
                int v = Value;
                if (null != Corrected_Gap_Value)
                {
                    v += Corrected_Gap_Value;
                }
                if (null != Corrected_Trolley_Value)
                {
                    v += Corrected_Trolley_Value;
                }
                return v;
            }
        }
        [XmlAttribute]
        public StringData Value { get; set; }

        [XmlAttribute]
        public StringData Corrected_Gap_Value { get; set; }

        [XmlAttribute]
        public StringData Corrected_Trolley_Value { get; set; }

        [XmlAttribute]
        public StringData Is_In_Long_Gap { get; set; }

    }

    public static class SydbClassExtend
    {
        public static bool checkDirection(this GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL instance)
        {
            if (Sys.Up != instance.Direction && Sys.Down != instance.Direction)
            {
                return false;
            }
            return true;
        }
        public static bool IsValidBMRoute(this GENERIC_SYSTEM_PARAMETERS.ROUTES.ROUTE instance)
        {
            if (!instance.Block_Mode.Equals(true) || instance.Name.ToString().EndsWith("_TAR"))
            {//BMGR-0022 route.BlockMode should be true, then add
                return false;
            }

            if (null == instance.Block_ID_List.Block_ID || 0 == instance.Block_ID_List.Block_ID.Count)
            {
                TraceMethod.Record(TraceMethod.TraceKind.WARNING, $"sydb route[{instance.Info}] Block_ID_List is none, this route will be ignore!\n");
                return false;
            }
            return true;
        }
        public static bool IncludeSDDB(this GENERIC_SYSTEM_PARAMETERS.SECONDARY_DETECTION_DEVICES.SECONDARY_DETECTION_DEVICE instance, int sddbid)
        {
            if (sddbid > 0)
            {
                return instance.Secondary_Detection_Device_Boundary_ID_List.Secondary_Detection_Device_Boundary_ID.Cast<int>().ToList().Exists(s => s == sddbid);
            }
            return false;
        }

        public static int GetBlockLen(this GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK instance)
        {
            return Math.Abs(instance.Kp_End - instance.Kp_Begin);
        }
        public static int GetSDDBIdByDirection(this GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK instance ,string dir)
        {
            if (Sys.Up == dir && null != instance.Up_Secondary_Detection_Device_Boundary_ID)
            {
                return instance.Up_Secondary_Detection_Device_Boundary_ID;
            }
            else if (Sys.Down == dir && null != instance.Down_Secondary_Detection_Device_Boundary_ID)
            {
                return instance.Down_Secondary_Detection_Device_Boundary_ID;
            }
            return -1;
        }
    }
    
}

