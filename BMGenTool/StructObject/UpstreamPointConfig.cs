#if itc
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
//using Summer.System.IO;
using MetaFly.Summer.IO;

using BMGenTool.Info;
using BMGenTool.Common;

namespace BMGenTool.Generate
{
    public class UpstreamPointConfig
    {
        public BEACON Beacon;
        public string Dir;//此方向来自IBBM方向，与beacon layout文件中的方向定义不同
        public int Length;
        public SyDB Sydb;

        public UpstreamPointConfig(BEACON beacon,string dir, int len, SyDB sydb)
        {
            this.Beacon = beacon;
            this.Dir = dir;

            //BMGR-0037  MAX_TRAIN_LENGTH + beacon.BeaconTolID
            this.Length = len + this.GetBeaconPTDistance(beacon.m_layoutInfo.BeaconTolID);
            this.Sydb = sydb;
        }

        //BMGR-0042 ??
        private List<PathInfo> CalPathList(Signal sig)
        {
            //沿信标的反方向深度遍历生成upstream_path
            Block beaconBlock = this.GetBeaconBlockInfo(Beacon.m_layoutInfo);
            //沿着beacon的反方向开始搜索
            string pathDir = "";
            if (Dir == Sys.Up)
            {
                pathDir = Sys.Dn;
            }
            else
            {
                pathDir = Sys.Up;
            }
            //计算的是从beacon位置开始的路径，减去beacon到block起始的距离
            int abs = 0;
            if (pathDir == Sys.Up)
            {
                abs = Math.Abs(beaconBlock.KpEnd - this.Beacon.m_layoutInfo.kp.KpRealValue);
            }
            else
            {
                abs = Math.Abs(this.Beacon.m_layoutInfo.kp.KpRealValue - beaconBlock.KpBegin);
            }

            //构建二叉树
            BinaryTree bt = new BinaryTree(beaconBlock, this.Length - abs, pathDir, Sydb, Sys.Both, true);
            //按照beacon方向排列，即搜索的反方向
            return bt.GetPathList(sig);
        }

        //BMGR-0037
        public List<PathInfo> GetPathList(string otherfile = "")
        {
            List<PathInfo> list = new List<PathInfo>();
            Dictionary<string, List<PathInfo>> configBeacon = new Dictionary<string, List<PathInfo>>();
            if ("" != otherfile)
            {
                configBeacon = this.GetConfigBeacon(otherfile);
            }

            if (configBeacon.ContainsKey(this.Beacon.Name))
            {
                list = configBeacon[this.Beacon.Name];
            }
            else//如果配置数据中不存在，则根据规则计算
            {
                if (1 == Beacon.m_reopeningSigNum)
                {
                    list = CalPathList(Beacon.m_ReopenOrgSig.SignalInfo);
                }
                else
                {
                    list = CalPathList(Beacon.m_AppOrgSigLst[0].SignalInfo);
                }
            }

            return list;
        }

        //BMRG-0037
        private Dictionary<string, List<PathInfo>> GetConfigBeacon(string filename)
        {
            Dictionary<string, List<PathInfo>> allBeacons = new Dictionary<string, List<PathInfo>>();

            XmlFileHelper xmlfile = XmlFileHelper.CreateFromFile(filename);
            if (null != xmlfile)
            {
                XmlVisitor root = xmlfile.GetRoot();
                List<XmlVisitor> beacons = root.Children().ToList();
                foreach (XmlVisitor node in beacons)
                {
                    string beaconName = node.GetAttribute("name");
                    List<PathInfo> list = new List<PathInfo>();
                    List<XmlVisitor> pathList = node.Children().ToList();
                    foreach (XmlVisitor up in pathList)
                    {
                        string pathName = up.GetAttribute("name");
                        List<PointInfo> pointList = new List<PointInfo>();
                        List<XmlVisitor> pnList = up.Children().ToList();
                        foreach (XmlVisitor pn in pnList)
                        {
                            string ptName = pn.GetAttribute("name");
                            int id = DataOpr.Xmlvalue2Int(pn,"id");
                            Point point = (Point)Sys.GetNode(id, Sydb.pointInfoList.Cast<Basic>().ToList());
                            string pos = pn.FirstChildByPath("position").Value;
                            string orit = pn.FirstChildByPath("orientation_in_route").Value;
                            if (orit == Sys.Convergent || orit == Sys.Divergent)
                            {
                                PointInfo info = new PointInfo(point, pos, orit, true);
                                pointList.Add(info);
                            }
                            else
                            {
                                //log error
                            }

                        }
                        PathInfo path = new PathInfo(pathName, pointList);
                        list.Add(path);
                    }
                    allBeacons.Add(beaconName, list);
                }
            }
            return allBeacons;
        }

        //BMGR-0037 
        private int GetBeaconPTDistance(int btId)
        {
            if (btId == 0)
            {
                return 50;
            }
            else if (btId == 1)
            {
                return 10;
            }
            else if (btId == 2)
            {
                return 2;
            }

            throw new Exception("Sydb data error. Beacon_Tolerance_ID is out of range[0,1,2]");
        }

        //计算Beacon所在的block
        private Block GetBeaconBlockInfo(BeaconLayout beacon)
        {
            Block block = new Block();
            foreach (Block node in Sydb.blockInfoList)
            {
                //信号机坐标坐落在此block上
                if (node.TrackId == beacon.TrackID)
                {
                    if ((node.KpBegin <= beacon.kp.KpRealValue && node.KpEnd > beacon.kp.KpRealValue)
                        || (node.KpBegin >= beacon.kp.KpRealValue && node.KpEnd < beacon.kp.KpRealValue))
                    {
                        block = node;
                        break;
                    }
                }
            }

            return block;
        }
    }
}
#endif