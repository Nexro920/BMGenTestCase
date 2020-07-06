using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetaFly.Summer.IO;
using MetaFly.Summer.Generic;

namespace BMGenTool.Info
{
    public enum PATH_TYPE
    {
        UPSTREAM,
        OVERLAP
    }
    public class PathInfo
    {
        //path with point then has pathName and ptList
        //path with no point then only has pathName
        //pathName is get by signal and pt(if has)

        public List<PointInfo> pointList;

        private GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL m_sig;

        private PATH_TYPE m_type;

        public PathInfo(GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL signal)
        {
            m_sig = signal;
            m_type = PATH_TYPE.OVERLAP;
            pointList = new List<PointInfo>();
        }
        public PathInfo(GENERIC_SYSTEM_PARAMETERS.SIGNALS.SIGNAL signal, List<PointInfo> points)
        {
            m_sig = signal;
            pointList = points;
            m_type = PATH_TYPE.OVERLAP;
        }

        public XmlVisitor GetXmlNode()
        {
            XmlVisitor node = XmlVisitor.Create("Path", null);

            if (PATH_TYPE.OVERLAP == m_type)
            {
                //BMGR-0068
                node.UpdateAttribute("NAME", GetOverlapPathName());
                //BMGR-0069
                foreach (PointInfo pt in pointList)
                {
                    node.AppendChild(pt.GetXmlNode());
                }
            }

            return node;
        }

        //BMGR-0043 
        public string GetUpstreamPathName()
        {
            string name = "";
            //如果有前缀情况

            foreach (PointInfo info in pointList)
            {
                if (info.Position == "Normal")
                {
                    name += string.Format("{0}_N|", info.Point.Name);
                }
                else if (info.Position == "Reverse")
                {
                    name += string.Format("{0}_R|", info.Point.Name);
                }

            }

            //when the path is from upstream file. has no signal name
            if (null != m_sig
                && "" != m_sig.Name)
            {
                name += "|" + m_sig.Name;
            }

            return name.Substring(0, name.Length - 1);
        }
        
        public string GetOverlapPathName()
        {
            string name = m_sig.Name + "|";
            
            foreach (PointInfo info in pointList)
            {
                if (info.Position == "Normal")
                {
                    name += string.Format("{0}-N|", info.Point.Name);
                }
                else if (info.Position == "Reverse")
                {
                    name += string.Format("{0}-R|", info.Point.Name);
                }
                
            }
            return name.Substring(0, name.Length - 1);
        }
        //public PathInfo(List<Block> bList,List<PointInfo> pList, Signal sig)
        //{//used by upstream not valid now
        //    this.pointList = pList;
        //    m_sig = sig;

        //    m_type = PATH_TYPE.UPSTREAM;
        //}

        //public PathInfo(string pathName, List<PointInfo> pList)
        //{//used by upstream not valid now
        //    this.pathName = pathName;
        //    this.pointList = pList;

        //    m_sig = null;
        //    m_type = PATH_TYPE.UPSTREAM;
        //}
    }
}
