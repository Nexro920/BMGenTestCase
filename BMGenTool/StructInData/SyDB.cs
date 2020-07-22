using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Summer.System.IO;
using MetaFly.Summer.IO;
using MetaFly.Summer.IO.CSV;
using MetaFly.Summer.Generic;
using BMGenTool.Common;
using MetaFly.Datum.Figure;

namespace BMGenTool.Info
{

    public interface IInfo
    {
        string Info { get; }
    }
    public interface IBeaconInfo: IInfo
    {
        int ID { get; }
        string Name { get; }
        int TrackID { get; set; }
        KP_V kp { get; set; }
        int BMB_Distance_cm { get; set; }
        bool IsVariantBeacon();
        int getVersion();
    }
    
    public class boundaryBeacon : IBeaconInfo
    {
        int _BMB_Distance_cm;
        public boundaryBeacon(Line_boundary_BM_beacons.BEACON indata)
        {
            data = indata;
            //this func may raise exception if error
            _BMB_Distance_cm = int.Parse(DataOpr.Multi100(data.BMB_SDDB_distance));
        }
        Line_boundary_BM_beacons.BEACON data;
        public string Info
        {
            get
            {
                return $"Boundary beacon {data.Name}-{data.Id}";
            }
        }
        public int ID
        {
            get
            {
                return (int)data.Id;
            }
        }


        public int TrackID
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public KP_V kp
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int BMB_Distance_cm
        {
            get
            {
                return _BMB_Distance_cm;//the unit should be cm in input xml file
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                return data.Name;
            }
        }


        public bool IsVariantBeacon()
        {
            return true;
        }

        public int getVersion()
        {
            if (null == data.Version 
                || null == data.Version.Value
                || "" == data.Version)
            {
                return 0;
            }
            return data.Version;
        }
    }

    public class BeaconLayout :  IBeaconInfo
    {
        public string Info
        {
            get
            {
                return $"SyDB beacon {Name}-{ID}";
            }
        }
        public int TrackID { get; set; }
        public KP_V kp { get; set; }
        //public string BeaconType { get; set; }
        //public string Direction { get; set; }
        //public bool BeaconCBTCAsso { get; set; }
        public int BeaconBM { get; set; }
        //public int BeaconTolID { get; set; }
        public int BeaconVersion { get; set; }

        public int BMB_Distance_cm
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int ID  { get; set; }

        public string Name { get; set; }

        public bool IsVariantBeacon()
        {
            if (0 == BeaconBM)
            {
                return false;
            }
            return true;
        }

        public int getVersion()
        {
            if (-1 == BeaconVersion)
            {
                return 0;
            }
            return BeaconVersion;
        }

    }    

    public class SyDB
    {
        private static SyDB _SyDB = null;
        public static SyDB GetInstance()
        {
            if (null == _SyDB)
            {
                _SyDB = new SyDB();
            }
            return _SyDB;
        }

        private GENERIC_SYSTEM_PARAMETERS data;

        public void LoadData(GENERIC_SYSTEM_PARAMETERS t)
        {
            data = t;

            ibbmInfoList = t.Implementation_Beacon_Block_Mode.BM_Beacon;
            signalInfoList = t.Signals.Signal;
            pointInfoList = t.Points.Point;
            overlapInfoList = t.Overlaps.Overlap;
            switchInfoList = t.Switchs.Switch;
            sddbInfoList = t.Secondary_Detection_Device_Boundarys.Secondary_Detection_Device_Boundary;
            sddInfoList = t.Secondary_Detection_Devices.Secondary_Detection_Device;
            routeInfoList = t.Routes.Route;
            blockInfoList = t.Blocks.Block;
        }
        
        public static GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK GetLocatedBlock(KP_V kp, int TrackID)
        {
            foreach (GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK node in SyDB.GetInstance().blockInfoList)
            {
                if (TrackID == node.Track_ID
                    && Math.Abs(node.Kp_Begin - node.Kp_End) == Math.Abs(node.Kp_Begin - kp.KpRealValue) + Math.Abs(node.Kp_End - kp.KpRealValue)
                    )
                {
                    return node;
                }
            }
            return null;
        }
        public static GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK GetLocatedBlock(KP_V kp, int TrackID, List<GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK> blockList)
        {
            foreach (GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK node in blockList)
            {
                if (true == IsLocatedOnBlock(kp, TrackID, node))
                {
                    return node;
                }
            }
            return null;
        }

        public static bool IsLocatedOnBlock(KP_V kp, int TrackID, GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK node)
        {
            if (TrackID == node.Track_ID
                && Math.Abs(node.Kp_Begin - node.Kp_End) == Math.Abs(node.Kp_Begin - kp.KpRealValue) + Math.Abs(node.Kp_End - kp.KpRealValue)
                )
            {
                return true;
            }
            return false;
        }

        public static bool IsLocatedOnBlockBeginOrEnd(KP_V kp, int TrackID, GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK node)
        {
            if (TrackID == node.Track_ID
                && (kp.KpRealValue == node.Kp_Begin || kp.KpRealValue == node.Kp_End)
                )
            {
                return true;
            }
            return false;
        }

        public static string GetReverseDir(string dir)
        {
            if (dir == Sys.Up)
            {
                return Sys.Down;
            }
            else if (dir == Sys.Down)
            {
                return Sys.Up;
            }

            throw new Exception(String.Format("GetReverseDir error! input dir[{0}]", dir));
        }
        /// <summary>
        /// get next block by current block and dir, which means only has one next block in the dir
        /// if occer unexpected convergent point then return -1.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="dir"></param>
        /// <returns>if get return id, else return -1</returns>
        public static int GetNextBlkID(GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK start, string dir)
        {
            int nextID = -1;
            if (Sys.Up == dir)
            {
                if (null != start.Next_Up_Reverse_Block_ID && null == start.Next_Up_Normal_Block_ID)
                {//only has reverse
                    nextID = start.Next_Up_Reverse_Block_ID;
                }
                else if (null == start.Next_Up_Reverse_Block_ID && null != start.Next_Up_Normal_Block_ID)
                {//has no reverse
                    nextID = start.Next_Up_Normal_Block_ID;
                }
            }
            else if (Sys.Down == dir)
            {
                if (null != start.Next_Down_Reverse_Block_ID && null == start.Next_Down_Normal_Block_ID)
                {
                    nextID = start.Next_Down_Reverse_Block_ID;
                }
                else if (null == start.Next_Down_Reverse_Block_ID && null != start.Next_Down_Normal_Block_ID)
                {
                    nextID = start.Next_Down_Normal_Block_ID;
                }
            }
            return nextID;
        }
        /// <summary>
        /// get next block by current block , dir and pos, 
        ///     first cal next block by curblock and dir,if can't get, then add pos 
        ///         which means if has 2 next block, will be chose by the pos
        /// </summary>
        /// <param name="start"></param>
        /// <param name="dir"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static int GetNextBlkID(GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK start, string dir, string pos)
        {
            int nextID = GetNextBlkID(start, dir);
            if (0 < nextID)
            {
                return nextID;
            }

            if (Sys.Up == dir)
            {
                if (Sys.Reverse == pos && null != start.Next_Up_Reverse_Block_ID)
                {//input reverse and has reverse
                    nextID = start.Next_Up_Reverse_Block_ID;
                }
                else if(null != start.Next_Up_Normal_Block_ID)
                {//input normal or has no reverse
                    nextID = start.Next_Up_Normal_Block_ID;
                }
            }
            else if (Sys.Down == dir)
            {
                if (Sys.Reverse == pos && null != start.Next_Down_Reverse_Block_ID)
                {
                    nextID = start.Next_Down_Reverse_Block_ID;
                }
                else if(null != start.Next_Down_Normal_Block_ID)
                {
                    nextID = start.Next_Down_Normal_Block_ID;
                }
            }
            else
            {
                throw new Exception(String.Format("GetNextBlkID error! input blockid[{0}] dir[{1}] pos[{2}]", start.ID, dir, pos));
            }
            return nextID;
        }

        public static string GetOrientByBlocks(GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK pre, GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK next)
        {
            if (pre.Next_Down_Reverse_Block_ID == next.ID || pre.Next_Up_Reverse_Block_ID == next.ID)
            {
                return Sys.Divergent;
            }

            if ((pre.Next_Up_Normal_Block_ID == next.ID && null != pre.Next_Up_Reverse_Block_ID)
                || (pre.Next_Down_Normal_Block_ID == next.ID && null != pre.Next_Down_Reverse_Block_ID))
            {
                return Sys.Divergent;
            }

            if (next.Next_Down_Reverse_Block_ID == pre.ID || next.Next_Up_Reverse_Block_ID == pre.ID)
            {
                return Sys.Convergent;
            }

            if ((next.Next_Up_Normal_Block_ID == pre.ID && null != next.Next_Up_Reverse_Block_ID)
                || (next.Next_Down_Normal_Block_ID == pre.ID && null != next.Next_Down_Reverse_Block_ID))
            {
                return Sys.Convergent;
            }

            throw new Exception(string.Format("Error call GetOrientByBlocks, there exist no point between block[{0}] and block[{1}]", pre.ID, next.ID));
        }
        public static string GetPosByBlocks(GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK pre, GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK next)
        {
            if (pre.Next_Down_Reverse_Block_ID == next.ID
                || pre.Next_Up_Reverse_Block_ID == next.ID
                || next.Next_Down_Reverse_Block_ID == pre.ID
                || next.Next_Up_Reverse_Block_ID == pre.ID)
            {
                return Sys.Reverse;
            }
            if ((pre.Next_Down_Normal_Block_ID == next.ID && next.Next_Up_Normal_Block_ID == pre.ID)
                || (pre.Next_Up_Normal_Block_ID == next.ID && next.Next_Down_Normal_Block_ID == pre.ID)
                )
            {
                return Sys.Normal;
            }
            throw new Exception(string.Format("Error call GetPosByBlocks, there exist no point between block[{0}] and block[{1}]", pre.ID, next.ID));
        }
        public static int GetSigIDInBlock(int blkID, List<int> sigList)
        {
            SyDB sydb = SyDB.GetInstance();
            var block = (GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK)Sys.GetNode(blkID, sydb.blockInfoList.Cast<Node>().ToList());
            foreach (int sID in sigList)
            {
                GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL sig = sydb.signalInfoList.Find(x => x.ID == sID);
                if (null != sig && true == SyDB.IsLocatedOnBlock(sig.Kp, sig.Track_ID, block))
                {
                    return sID;
                }
            }
            return -1;
        }
        public void clear(bool onlyclearbeacon = false)
        {
            if (true == onlyclearbeacon)
            {
                beaconInfoList.Clear();
                boundBeaconInfoList.Clear();
                return;
            }

            //clear all info list
            beaconInfoList.Clear();
            boundBeaconInfoList.Clear();
            ibbmInfoList.Clear();
            signalInfoList.Clear();
            pointInfoList.Clear();
            overlapInfoList.Clear();
            switchInfoList.Clear();
            sddbInfoList.Clear();
            sddInfoList.Clear();
            routeInfoList.Clear();
            blockInfoList.Clear();
        }

        public static bool checkRepeat(List<string> list, ref string info)
        {
            var q = list.GroupBy(x=>x).Where(x => x.Count() > 1).ToList();
            string repeats = "";
            foreach (var item in q)
            {
                repeats += item.Key + "  ";
            }
            if ("" == repeats)
            {
                return true;
            }
            info = info + ":" + repeats;
            return false;
        }

        public List<IBeaconInfo> GetBeacons()
        {
            List<IBeaconInfo> list = new List<IBeaconInfo>();
            list.AddRange(beaconInfoList);
            list.AddRange(boundBeaconInfoList);

            List<string> beaconnames = (from beacon in list
                                               select beacon.Name).ToList<string>();

            string buff = "The beaconnames from layout.csv and boundarybeacons.xml is repeated, please modify";
            if (false == checkRepeat(beaconnames, ref buff))  // no repeat return true else false.
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, buff);
            }
            list = list.GroupBy(p => p.Name).Select(g => g.First()).ToList();

            List<string> ids = (from beacon in list select beacon.ID.ToString()).ToList<string>();
            buff = "The IDs from layout.csv and boundarybeacons.xml is repeated, please modify";
            if (false == checkRepeat(ids, ref buff))
            {
                TraceMethod.Record(TraceMethod.TraceKind.ERROR, buff);
            }
            return list.GroupBy(p => p.ID).Select(g => g.First()).ToList();
        }
        /// <summary>
        /// SYDB的BEACON表信息
        /// </summary>
        private List<BeaconLayout> beaconInfoList = new List<BeaconLayout>();

        /// <summary>
        /// Line_boundary_BM_beacons中的Beacon表信息
        /// </summary>
        private List<IBeaconInfo> boundBeaconInfoList = new List<IBeaconInfo>();

        public bool ReadcsvBeacon(string csvFileName)
        {
            beaconInfoList.Clear();
            CsvFileReader csvFileRd = new CsvFileReader(csvFileName, Encoding.UTF8, ';');

            int count = csvFileRd.RowCount;
            if (count <= 2)
            {
                TraceMethod.RecordInfo($"Warning: {csvFileName} line number <=2, can't get beacon info.");
                return false;
            }

            List<List<string>> data = csvFileRd.GetData(1, count);  //get csv data

            List<string> headList = data[0];

            for (int idx = 0; idx < headList.Count(); ++idx)
            {
                headList[idx] = headList[idx].ToLower();
            }

            Func<string, int, string> readcsv = (colName, row) =>
            {
                int col = headList.IndexOf(colName.ToLower());
                if (1 <= col)
                {
                    return data[row][col].Trim();
                }
                return "";
            };

            Func<string, int, Restriction, int> readintcsv = (colName, row, restri) =>
            {
                string buff = readcsv(colName, row);

                int val = int.Parse(buff);

                if (restri.Validate(buff, colName) == false)
                {
                    throw new Exception($"[{colName}] is invalid, read {buff}");
                }
                
                return val;
            };

            //get first beacon line
            int start = 2;
            for (int i = 1; i <= count; ++i)
            {
                bool isNote = false;
                if (data[i][0].ToUpper() == "V")
                {
                    for (int j = 0; j < csvFileRd.ColCount; ++j)
                    {
                        if (data[i][j].StartsWith("\""))
                        {
                            isNote = true;
                            break;
                        }
                    }
                }
                if (false == isNote)
                {
                    start = i;
                    break;
                }
            }

            //get restriction info
            Restriction res = new Restriction(@"./Config/Restriction.xml");
            res.SetParentPath("BEACON_LAYOUT");

            //for each line
            for (int i = start; i < count; ++i)
            {
                //check start
                if (data[i][0].ToUpper() == "V")
                {
                    BeaconLayout beacon = new BeaconLayout();
                    try
                    {
                        beacon.Name = readcsv("Beacon_Name", i);
                        beacon.BeaconVersion = readintcsv("Beacon_Version", i, res);
                        beacon.ID = readintcsv("Beacon_ID", i, res);
                        beacon.TrackID = readintcsv("Track_ID", i, res);
                        beacon.kp = new KP_V();
                        // kp in csv file = finally kp
                        beacon.kp.Value = new StringData(DataOpr.Multi100(readcsv("Kp", i)));
                        beacon.BeaconBM = readintcsv("Beacon_Block_Mode", i, res);
                    }
                    catch (Exception ex)
                    {
                        TraceMethod.Record(TraceMethod.TraceKind.ERROR, 
                            $"Warning:load layout beacon {beacon.Name} fail {ex.Message}, ignore this beacon.");
                        continue;
                    }
                    
                    beaconInfoList.Add(beacon);
                }
            }
            if (0 == beaconInfoList.Count())
            {
                return false;
            }
            return true;
        }

        public void ReadBoundaryBeacon(Line_boundary_BM_beacons bbeacons)
        {
            boundBeaconInfoList.Clear();
            foreach (Line_boundary_BM_beacons.BEACON b in bbeacons.Beacon)
            {
                try
                {
                    boundaryBeacon beacon = new boundaryBeacon(b);
                    boundBeaconInfoList.Add(beacon);
                }
                catch (Exception ex)
                {
                    TraceMethod.Record(TraceMethod.TraceKind.ERROR,
                        $"Warning:load boundary beacon {b.Name} fail {ex.Message}, ignore this beacon.");
                    continue;
                }
            }
        }
        /// <summary>
        /// SYDB的进路表信息
        /// </summary>
        public List<GENERIC_SYSTEM_PARAMETERS.ROUTES.ROUTE> routeInfoList = new List<GENERIC_SYSTEM_PARAMETERS.ROUTES.ROUTE>();

        /// <summary>
        /// SYDB的IBBM表信息
        /// </summary>
        public List<GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON> ibbmInfoList = new List<GENERIC_SYSTEM_PARAMETERS.IMPLEMENTATION_BEACON_BLOCK_MODE.BM_BEACON>();

        /// <summary>
        /// SYDB的SIGNAL表信息
        /// </summary>
        public List<GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL> signalInfoList = new List<GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL>();

        /// <summary>
        /// SYDB的BLOCK表信息
        /// </summary>
        public List<GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK> blockInfoList = new List<GENERIC_SYSTEM_PARAMETERS.BLOCKS.BLOCK>();

        /// <summary>
        /// SYDB的POINT表信息
        /// </summary>
        public List<GENERIC_SYSTEM_PARAMETERS.SWITCHS.SWITCH> switchInfoList = new List<GENERIC_SYSTEM_PARAMETERS.SWITCHS.SWITCH>();

        /// <summary>
        /// SYDB的POINT表信息
        /// </summary>
        public List<GENERIC_SYSTEM_PARAMETERS.POINTS.POINT> pointInfoList = new List<GENERIC_SYSTEM_PARAMETERS.POINTS.POINT>();

        /// <summary>
        /// SYDB的OVERLAP表信息
        /// </summary>
        public List<GENERIC_SYSTEM_PARAMETERS.OVERLAPS.OVERLAP> overlapInfoList = new List<GENERIC_SYSTEM_PARAMETERS.OVERLAPS.OVERLAP>();

        /// <summary>
        /// SYDB的SDD表信息
        /// </summary>
        public List<GENERIC_SYSTEM_PARAMETERS.SECONDARY_DETECTION_DEVICES.SECONDARY_DETECTION_DEVICE> sddInfoList = new List<GENERIC_SYSTEM_PARAMETERS.SECONDARY_DETECTION_DEVICES.SECONDARY_DETECTION_DEVICE>();

        /// <summary>
        /// SYDB的SDDB表信息
        /// </summary>
        public List<GENERIC_SYSTEM_PARAMETERS.SECONDARY_DETECTION_DEVICE_BOUNDARYS.SECONDARY_DETECTION_DEVICE_BOUNDARY> sddbInfoList = new List<GENERIC_SYSTEM_PARAMETERS.SECONDARY_DETECTION_DEVICE_BOUNDARYS.SECONDARY_DETECTION_DEVICE_BOUNDARY>();

        public int LineID
        {
            get
            {
                if (null == data || data.Lines.Line.Count <= 0)
                {
                    return -1;
                }
                return data.Lines.Line[0].Interoperable_Line_Number;
            }
        }

    }
}
