using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MetaFly.Datum.Figure;
using MetaFly.Serialization;
using MetaFly.Summer.Generic;

namespace BMGenTool.Info
{
    public class GENERIC_SYSTEM_PARAMETERS
    {
        public LINES Lines { get; set; }
        public class LINES
        {
            public List<LINE> Line { get; set; }
            public class LINE
            {
                [XmlElement]
                public StringData Interoperable_Line_Number { get; set; }
            }
        }

        public IMPLEMENTATION_BEACON_BLOCK_MODE Implementation_Beacon_Block_Mode { get; set; }
        public class IMPLEMENTATION_BEACON_BLOCK_MODE
        {
            public List<BM_BEACON> BM_Beacon { get; set; }
            public class BM_BEACON: Node
            {
                [XmlAttribute]
                public StringData ID { get; set; }
                [XmlAttribute]
                public StringData Name { get; set; }

                public leu LEU { get; set; }
                public class leu:IInfo
                {
                    [XmlAttribute]
                    public StringData LEU_Name { get; set; }

                    [XmlAttribute]
                    public StringData Beacon_Output_number { get; set; }

                    public string Info
                    {
                        get
                        {
                            return $"IBBM LEU {LEU_Name}";
                        }
                    }
                }
                
                [XmlElement]
                public StringData CI_Name { get; set; }

                [XmlElement]
                public StringData Direction { get; set; }

                [XmlElement]
                public StringData Is_Guaranted_Beacon { get; set; }

                public List<INPUT_SIGNAL> Input_Signal { get; set; }
                public class INPUT_SIGNAL : INPUT_INFO
                {
                    public override string Info
                    {
                        get
                        {
                            return $"Input_Signal RANK={RANK} Name={Name} Type={Type}";
                        }
                    }

                }

                public List<INPUT_POINT> Input_Point { get; set; }

                public string Info
                {
                    get
                    {
                        return $"IBBM {Name}";
                    }
                }

                public class INPUT_POINT : INPUT_INFO
                {
                    public override string Info
                    {
                        get
                        {
                            return $"Input_Point RANK={RANK} Name={Name} Type={Type}";
                        }
                    }

                }

                private int reopenSigNum = 0;
                private int appSigNum = 0;
                private string type = BeaconType.Unknown;
                private string linkedSigName = "";

                public int getInputRank(string name, string type = "")
                {
                    int rank = -1;
                    if ("" == type && null != Input_Signal)
                    {
                        int idx = Input_Signal.FindIndex(x => (x.Name == name));
                        if (idx >= 0)
                        {
                            rank = Input_Signal[idx].RANK;
                        }
                    }
                    else if (null != Input_Point)
                    {
                        int idx = Input_Point.FindIndex(x => (x.Name.ToString().StartsWith(name) && x.Type == type));
                        if (idx >= 0)
                        {
                            rank = Input_Point[idx].RANK;
                        }
                    }

                    return rank;
                }

                public string getLinkedSigName()
                {
                    GetBeaconType();
                    return linkedSigName;
                }

                public string GetBeaconType()
                {
                    if (BeaconType.Unknown == type)
                    {
                        reopenSigNum = 0;
                        appSigNum = 0;
                        //BMGR-0082
                        foreach (var input in Input_Signal)
                        {
                            if (input.Type == Sys.Reopening)
                            {
                                ++reopenSigNum;
                                linkedSigName = input.Name;
                            }
                            else if (input.Type == Sys.Approach)
                            {
                                ++appSigNum;
                                if ("" == linkedSigName)
                                {
                                    linkedSigName = input.Name;
                                }
                            }
                            else
                            {
                                //error unknow input type
                                TraceMethod.Record(TraceMethod.TraceKind.ERROR,
                                $"input IBBM error: beacon {Name} has unknow input signal type[{input.Type}]!");
                            }
                        }

                        //chapter 4.1 check the IBBM<input_signal>
                        //1   approach 0 reopening: type = approach beacon
                        //0   approach 1 reopening: type = Reopening beacon
                        //1-3 approach 1 reopening: type = Reopening_Approach beacon
                        //others error

                        //BMGR-0015 get the type by ibbm input signal
                        //type = approach beacon:
                        //0 reopening 1 approach 
                        if ((0 == reopenSigNum && 1 == appSigNum)
                            )
                        {//@Type is Approach
                            type = BeaconType.Approach;
                        }
                        else if (1 == reopenSigNum)
                        {
                            BEACON.SignamReBeaconDic[linkedSigName] = Name;
                            if (appSigNum > 3)
                            {//error
                                type = BeaconType.Invalid;
                                TraceMethod.RecordInfo($"Beacon {Name} data error, the input signal number of approach is more than 3!");
                            }

                            else if (0 == appSigNum)
                            {//0 approach 1 reopening: type = Reopening beacon
                                type = BeaconType.Reopening;
                            }
                            else
                            {//1-3 approach 1 reopening: type = Reopening_Approach beacon
                                type = BeaconType.Reopening_Approach;
                            }
                        }
                        else
                        {//error
                            type = BeaconType.Invalid;
                            TraceMethod.RecordInfo($"Sydb file error: Beacon {Name}, the input signal number is unknown Reopening={reopenSigNum} Approach={appSigNum}!");
                        }
                    }

                    return type.ToString();
                }

            }

        }

        public SIGNALS Signals { get; set; }
        public class SIGNALS
        {

            public List<SIGNAL> Signal { get; set; }
            public class SIGNAL: Node
            {
                public string Info
                {
                    get
                    {
                        return $"Signal {Name}-{ID}";
                    }
                }
                public int SDDId
                {
                    get { return Secondary_Detection_Device_ID; }
                }
                [XmlAttribute]
                public StringData ID { get; set; }

                [XmlAttribute]
                public StringData Name { get; set; }

                [XmlElement]
                public StringData Track_ID { get; set; }
                

                [XmlElement]
                public StringData Overlap_ID { get; set; }

                [XmlElement]
                public StringData Secondary_Detection_Device_ID { get; set; }


                [XmlElement]
                public StringData Direction { get; set; }

                [XmlElement]
                public StringData Signal_Type_Function { get; set; }


                public KP Kp { get; set; }
                public class KP : KP_V
                { }
                
                
            }

        }

        public ROUTES Routes { get; set; }
        public class ROUTES
        {
            public List<ROUTE> Route { get; set; }
            public class ROUTE:Node,IInfo
            {
                [XmlAttribute]
                public StringData ID { get; set; }

                [XmlAttribute]
                public StringData Name { get; set; }

                [XmlElement]
                public StringData Origin_Signal_ID { get; set; }
                

                [XmlElement]
                public StringData Destination_Signal_ID { get; set; }
                

                public BLOCK_ID_LIST Block_ID_List { get; set; }
                public class BLOCK_ID_LIST
                {
                    [XmlElement]
                    public List<StringData> Block_ID { get; set; }
                }

                [XmlElement]
                public StringData Block_Mode { get; set; }

                public SPACING_SIGNAL_ID_LIST Spacing_Signal_ID_List { get; set; }
                public class SPACING_SIGNAL_ID_LIST
                {
                    [XmlElement]
                    public List<StringData> Signal_ID { get; set; }

                }
                public string Info
                {
                    get
                    {
                        return $"Route {ID}-{Name} orgSig {Origin_Signal_ID} dstSig {Destination_Signal_ID}";
                    }
                }
            }

        }

        public BLOCKS Blocks { get; set; }
        public class BLOCKS
        {
            public List<BLOCK> Block { get; set; }
            public class BLOCK:Node
            {
                [XmlAttribute]
                public StringData ID { get; set; }

                [XmlAttribute]
                public StringData Name { get; set; }
                

                [XmlElement]
                public StringData Track_ID { get; set; }

                [XmlElement]
                public StringData Secondary_Detection_Device_ID { get; set; }
                

                [XmlElement]
                public StringData Kp_Begin { get; set; }

                [XmlElement]
                public StringData Kp_End { get; set; }

                [XmlElement]
                public StringData Next_Up_Normal_Block_ID { get; set; }
                
                [XmlElement]
                public StringData Down_Secondary_Detection_Device_Boundary_ID { get; set; }
                
                [XmlElement]
                public StringData Is_Direction_Opposite { get; set; }
                

                [XmlElement]
                public StringData Point_ID { get; set; }

                [XmlElement]
                public StringData Next_Down_Normal_Block_ID { get; set; }

                [XmlElement]
                public StringData Next_Down_Reverse_Block_ID { get; set; }

                [XmlElement]
                public StringData Up_Secondary_Detection_Device_Boundary_ID { get; set; }

                [XmlElement]
                public StringData Next_Up_Reverse_Block_ID { get; set; }
                

                public string Info
                {
                    get
                    {
                        return $"block {Name}-{ID}";
                    }
                }
            }

        }

        public POINTS Points { get; set; }
        public class POINTS
        {

            public List<POINT> Point { get; set; }
            public class POINT:Node
            {
                [XmlAttribute]
                public StringData ID { get; set; }

                [XmlAttribute]
                public StringData Name { get; set; }

                [XmlElement]
                public StringData Interoperable_ID { get; set; }
               

                public string Info
                {
                    get
                    {
                        return $"Point {Name}-{ID}";
                    }
                }
            }

        }

        public SECONDARY_DETECTION_DEVICE_BOUNDARYS Secondary_Detection_Device_Boundarys { get; set; }
        public class SECONDARY_DETECTION_DEVICE_BOUNDARYS
        {

            public List<SECONDARY_DETECTION_DEVICE_BOUNDARY> Secondary_Detection_Device_Boundary { get; set; }
            public class SECONDARY_DETECTION_DEVICE_BOUNDARY : Node
            {
                [XmlAttribute]
                public StringData ID { get; set; }

                [XmlAttribute]
                public StringData Name { get; set; }
                

                public string Info
                {
                    get
                    {
                        return $"SDDB {Name}-{ID}";
                    }
                }
            }

        }

        public SECONDARY_DETECTION_DEVICES Secondary_Detection_Devices { get; set; }
        public class SECONDARY_DETECTION_DEVICES
        {

            public List<SECONDARY_DETECTION_DEVICE> Secondary_Detection_Device { get; set; }
            public class SECONDARY_DETECTION_DEVICE : Node
            {
                [XmlAttribute]
                public StringData ID { get; set; }

                [XmlAttribute]
                public StringData Name { get; set; }
                

                public SECONDARY_DETECTION_DEVICE_BOUNDARY_ID_LIST Secondary_Detection_Device_Boundary_ID_List { get; set; }
                public class SECONDARY_DETECTION_DEVICE_BOUNDARY_ID_LIST
                {
                    [XmlElement]
                    public List<StringData> Secondary_Detection_Device_Boundary_ID { get; set; }
                }

                public string Info
                {
                    get
                    {
                        return $"SDD {Name}-{ID}";
                    }
                }
            }

        }

        public SWITCHS Switchs { get; set; }
        public class SWITCHS
        {

            public List<SWITCH> Switch { get; set; }
            public class SWITCH:Node
            {
                [XmlAttribute]
                public StringData ID { get; set; }

                [XmlAttribute]
                public StringData Name { get; set; }

                public List<int> PointIdLst {
                    get
                    {
                        List<int> list = new List<int>();
                        if (null != Convergent_Point_ID_List.Convergent_Point_ID)
                        {
                            list.AddRange(Convergent_Point_ID_List.Convergent_Point_ID.Cast<int>());
                        }
                        if (null != Divergent_Point_ID_List.Divergent_Point_ID)
                        {
                            list.AddRange(Divergent_Point_ID_List.Divergent_Point_ID.Cast<int>());
                        }
                        return list;
                    }
                }

                public CONVERGENT_POINT_ID_LIST Convergent_Point_ID_List { get; set; }
                public class CONVERGENT_POINT_ID_LIST
                {
                    [XmlElement]
                    public List<StringData> Convergent_Point_ID { get; set; }

                }

                public DIVERGENT_POINT_ID_LIST Divergent_Point_ID_List { get; set; }
                public class DIVERGENT_POINT_ID_LIST
                {
                    [XmlElement]
                    public List<StringData> Divergent_Point_ID { get; set; }

                }

                [XmlElement]
                public StringData Switch_Type { get; set; }
                

                public string Info
                {
                    get
                    {
                        return $"switch {Name}-{ID}-{Switch_Type}";
                    }
                }
            }

        }
        public OVERLAPS Overlaps { get; set; }
        public class OVERLAPS
        {

            public List<OVERLAP> Overlap { get; set; }
            public class OVERLAP:Node,IInfo
            {
                [XmlAttribute]
                public StringData ID { get; set; }

                [XmlAttribute]
                public StringData Name { get; set; }
                

                [XmlElement]
                public StringData Overlap_Type { get; set; }
                

                public OVERLAP_SWITCH_ID_LIST Overlap_Switch_ID_List { get; set; }
                public class OVERLAP_SWITCH_ID_LIST
                {
                    [XmlElement]
                    public List<StringData> Switch_ID { get; set; }

                }
                
                [XmlElement]
                public StringData D_Overlap { get; set; }
                

                public OVERLAP_BLOCK_ID_LIST Overlap_Block_ID_List { get; set; }
                public class OVERLAP_BLOCK_ID_LIST
                {
                    [XmlElement]
                    public List<StringData> Block_ID { get; set; }

                }

                public string Info
                {
                    get
                    {
                        return $"overlap {Name}-{ID}-{Overlap_Type}";
                    }
                }
            }

        }

    }

}
