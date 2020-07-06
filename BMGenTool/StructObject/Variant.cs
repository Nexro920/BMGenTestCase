using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Summer.System.IO;
using MetaFly.Summer.IO;
using MetaFly.Summer.Generic;

namespace BMGenTool.Info
{
    public enum VAR_TYPE
    {
        E_POINT,
        E_SIGNAL
    }
    
    public class Variant:IEquatable<Variant>
    {
        public string Info
            {
            get
            {
                return $"variant {m_Idx} {ObjectName}";
            }
            }
        public bool Equals(Variant y)
        {
            return y.ObjectName == ObjectName;
        }

        public override int GetHashCode()
        {
            return ObjectName.GetHashCode();
        }

        public int m_Idx;
        public int InputRank { get; set; }
        public string ObjectName { get; }

        private VAR_TYPE m_varType;

        public VAR_TYPE GetVarSrc()
        {
            return m_varType;
        }
        public Variant(VAR_TYPE type, int rank, string name)
        {
            m_varType = type;
            InputRank = rank;
            ObjectName = name;
        }

        public void SetIdx(int idx)
        {
            m_Idx = idx;
        }

        public virtual string GetName()
        {
            return "uninitial variant";
        }

        public virtual XmlVisitor GetVariantXmlNode()
        {
            XmlVisitor variant1 = XmlVisitor.Create("Variant", null);
            return variant1;
        }
        
    }

    public class VariantSignal: Variant
    {
        private OriginSignal m_signal;

        public VariantSignal(OriginSignal signal) : base(VAR_TYPE.E_SIGNAL, signal.m_ibbmIn.RANK, signal.SignalInfo.Name)
        {
            m_signal = signal;
        }
        public override XmlVisitor GetVariantXmlNode()
        {
            XmlVisitor variant1 = XmlVisitor.Create("Variant", null);

            variant1.AppendChild("Index", m_Idx);
            variant1.AppendChild("Type", "SIGNAL");
            variant1.AppendChild("Object_name", m_signal.SignalInfo.Name);
            variant1.AppendChild("Input_rank", InputRank);

            return variant1;
        }
        public override string GetName()
        {
            return m_signal.GetName();
        }
    }

    public class VariantPoint: Variant
    {
        private int value;
        private PointInfo m_pt;
        private string pointVariantPos;
        
        public bool check(string name)
        {//BMGR-0031
            if (-1 == InputRank && -1 == value)
            {
                TraceMethod.RecordInfo($"Error: Point[{m_pt.Info}] not in beacon[{name}] IBBM input, please check!");
                return false;
            }
            return true;
        }
        public string PointVariantPos
        {
            get
            {
                return pointVariantPos;
            }
        }
        public override XmlVisitor GetVariantXmlNode()
        {
            XmlVisitor variant1 = XmlVisitor.Create("Variant", null);

            {//BMGR-0040 //BMGR-0030
                variant1.AppendChild("Index", m_Idx);
                variant1.AppendChild("Type", string.Format("POINT_{0}", PointVariantPos[0]));
                variant1.AppendChild("Object_name", string.Format("{0}_{1}", m_pt.Point.Name, PointVariantPos[0]));
            }

            //BMGR-0041 //BMGR-0031
            if (-1 != InputRank)
            {
                variant1.AppendChild("Input_rank", InputRank);
            }
            else if (-1 != value)
            {
                variant1.AppendChild("Value", value);
            }
            else
            {
                TraceMethod.RecordInfo($"Point[{m_pt.Info}] not in IBBM input and can't calculate value, log Error!");
            }
            return variant1;
        }
        public VariantPoint(PointInfo point, string variantPos, string switchPos) : base(VAR_TYPE.E_POINT, -1, string.Format("{0}_{1}", point.Point.Name, variantPos[0]))
        {
            if (Sys.Normal != variantPos && Sys.Reverse != variantPos)
            {
                throw new Exception($"create Variant Error: invalid variant pos[{variantPos}]!");
            }

            pointVariantPos = variantPos;
            m_pt = point;
            
            value = point.GetVariantValue(variantPos, switchPos);
        }

        public override string GetName()
        {
            return m_pt.Point.Name;
        }

        public string GetPointVarPos()
        {
            return PointVariantPos[0].ToString();
        }
    }
}
