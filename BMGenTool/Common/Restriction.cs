using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using MetaFly.Summer.IO;
using BMGenTool.Info;
using System.Xml;
using MetaFly.Summer.Generic;
using System.IO;

namespace BMGenTool.Common
{
    public struct IntRestriction {
        public int max;
        public int min;

        public bool validate(string value)
        {
            int data = int.Parse(value);
            if (data >= min && data <= max)
            {
                return true;
            }
            return false;
        }
    };
    /// <summary>
    /// deal restriction file of xmlformat
    /// read <INT MAX="16383" MIN="1"/> to IntRestriction
    /// </summary>
    public class Restriction
    {
        public Restriction(string xmlfullname)
        {
            XmlFileHelper f = XmlFileHelper.CreateFromFile(xmlfullname);
            root = f.GetRoot();
        }

        private XmlVisitor root = null;

        /// <summary>
        /// 将parent重设为restriction.xml根节点
        /// </summary>
        public void ResetParent()
        {
            IXmlVisitorBase fileroot = root.Parent;
            while (fileroot != null)
            {
                root = (XmlVisitor)fileroot;
                fileroot = root.Parent;
            }
        }

        private IntRestriction CreatIntResriction(IXmlVisitorBase node)
        {
            IntRestriction res = new IntRestriction();
            try
            {
                if (node.Name == "INT")
                {
                    res.max = int.Parse(node.GetAttribute("MAX"));
                    res.min = int.Parse(node.GetAttribute("MIN"));
                    return res;
                }
                throw new Exception();
            }
            catch
            {
                throw new Exception($"{node.ToString()} is invalide. IntRestriction should be <INT MAX=\"16383\" MIN=\"1\"/>");
            }
        }
        /// <summary>
        /// 设置到restrition file的某父级结点，以便调用 public bool Validate(string value, string childname) 检查各childname
        /// </summary>
        /// <param name="xpath">XPath格式为:名称1.子名称2.子名称3</param>
        /// <returns></returns>
        public bool SetParentPath(string xpath)
        {
            if (root.ChildrenByPath(xpath) == null)
            {
                return false;
            }
            root = root.ChildrenByPath(xpath).First();
            return root.HasChildren;
        }

        /// <summary>
        ///  通过xpath直接获取对应限制信息进行验证
        /// </summary>
        /// <param name="value"></param>
        /// <param name="xpath">XPath格式为:名称1.子名称2.子名称3</param>
        /// <returns></returns>
        public bool ValidateByXpath(string value, string xpath)
        {
            return ValidateValue(value, root.ChildrenByPath(xpath));
        }
        /// <summary>
        /// 通过childname的限制信息验证value。
        /// 与public bool SetParentPath(string xpath)配合使用xpath到childname的父节点
        /// </summary>
        /// <param name="value"></param>
        /// <param name="childname"></param>
        /// <returns></returns>
        public bool Validate(string value, string childname)
        {
            foreach (var n in root.Children())
            {
                if (n.Name == childname)
                {
                    return ValidateValue(value, n.Children());
                }
            }
            TraceMethod.Record(TraceMethod.TraceKind.WARNING,
                            $"Get no restriction info of {childname}, lack check of {value}");
            return false;
        }

        private bool ValidateValue(string value, IEnumerable<XmlVisitor> resnodes)
        {
            string log = "";
            foreach (var res in resnodes)
            {
                if (res.Name == "INT")
                {
                    IntRestriction intrange = CreatIntResriction(res);
                    if (true == intrange.validate(value))
                    {
                        return true;
                    }
                    log += res.ToString();
                }
                else
                {
                    throw new Exception($"Restriction {res.ToString()} is unknown.");
                }
            }
            TraceMethod.Record(TraceMethod.TraceKind.WARNING,
                            $"{value} is invalid of restriction {log}");
            return false;
        }
    }
}
