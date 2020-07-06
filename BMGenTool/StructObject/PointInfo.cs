using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Summer.System.IO;
using MetaFly.Summer.IO;
using MetaFly.Summer.Generic;

namespace BMGenTool.Info
{
    //enum is value type
    public enum PointLocation 
    {
         UpSteam = 1,
         Route = 2,
         Overlap = 3,
         Invalid
    }

    public class PointInfo
    {
        IInfo srcStr;
        public string Info
        {
            get
            {
                return $"{Point.Name} located in {ptSrc}[{srcStr.Info}] {position} {Orientation}";
            }
        }
        public GENERIC_SYSTEM_PARAMETERS.POINTS.POINT Point;
        //BMGR-0031
        /// <summary>
        /// Calculate the value of VariantPoint
        /// </summary>
        /// <param name="varinatPos"></param>
        /// <param name="switchPos"></param>
        /// <returns></returns>
        public int GetVariantValue(string varinatPos, string switchPos = "")
        {
            string variantPos = varinatPos;
            string valuePos = "";
            if (orient == Sys.Convergent)
            {
                valuePos = position;
            }
            else if (PointLocation.Overlap == ptSrc)
            {
                valuePos = switchPos;
            }

            if ("" == valuePos)
            {
                return -1;
            }

            if (variantPos[0] == valuePos[0])
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public string Position
        {
            get { return position; }
        }
        public string Orientation
        {
            get { return orient; }
        }
        private string orient;
        private string position;//point in route or overlap or upstream should have a position
        private PointLocation ptSrc;
        public PointInfo(GENERIC_SYSTEM_PARAMETERS.POINTS.POINT point, string pos, string orient, PointLocation src, IInfo srcI)
        {
            this.ptSrc = src;
            this.Point = point;
            this.position = pos;
            this.orient = orient;
            this.srcStr = srcI;
            check();
        }

        //根据配置得到point信息
        public PointInfo(GENERIC_SYSTEM_PARAMETERS.POINTS.POINT point, string pos, string orient, bool isConfig, PointLocation src)
        {
            //upstream not use
            throw new Exception("Upstream call PointInfo, Error, upstream is not support!");
        }

       //GMBR-0059
        public int GetPostionInt()
        {
            check();
            if ("Normal" == Position)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }
        public bool CalVariants(List<Variant> vList, string switchPos = "")
        {
            //BMGR-0040 a point => 2 variant of N and R
            foreach (string variantPos in Sys.PointPositions)
            {
                //BMGR-0041
                Variant vpt = new VariantPoint(this, variantPos, switchPos);
                vList.Add(vpt);
            }
            return true;
        }

        //BMGR-0023 //BMGR-0068 ptname-N or ptname-R
        public string getNamePosStr(string pos = "")
        {
            string buff;
            if (Sys.Normal == pos)
            {
                buff = string.Format("{0}-N", Point.Name);
            }
            else if (Sys.Reverse == pos)
            {
                buff = string.Format("{0}-R", Point.Name);
            }
            else
            {
                check();
                buff = string.Format("{0}-{1}", Point.Name, Position[0]);
            }

            return buff;
        }

        private void check()
        {
            if(Sys.Normal != Position
                && Sys.Reverse != Position)
            {
                throw new Exception(string.Format("Point[{0}] Position[{1}] is invalid!", Point.Name, Position));
            }

            if (Sys.Convergent != orient
                && Sys.Divergent != orient)
            {
                throw new Exception(string.Format("Point[{0}] orient[{1}] is invalid!", Point.Name, orient));
            }
        }

        //BMGR-0025  BMGR-0069
        public XmlVisitor GetXmlNode()
        {//if point is in upstream, not support now, need modify
            XmlVisitor pointNode = XmlVisitor.Create("Point", null);
            pointNode.UpdateAttribute("NAME", this.Point.Name);
            pointNode.AppendChild("Id", this.Point.ID);
            
            pointNode.AppendChild("Position", this.Position);//BMGR-0024 //BMGR-0069
            pointNode.AppendChild("Orientation_in_route", orient);
            return pointNode;
        }

       //BMGR-0041 //BMGR-0031 
        //if input pos is same with origin Position, return 1
        //not same, return 0
        //input pos invalid, will raise exception
       public int GetPosValue(string pos)
       {
           if (Sys.Normal != pos && Sys.Reverse != pos)
           {
                TraceMethod.RecordInfo(string.Format("input pos[{0}] in GetPosValue is invalid pointName[{1}]", pos, Point.Name));
                return 9999;
                
               //throw new Exception(string.Format("input pos[{0}] in GetPosValue is invalid", pos));
           }

           if (Position[0] == pos[0])
           {
               return 1;
           }
           return 0;
       }
    }
}
