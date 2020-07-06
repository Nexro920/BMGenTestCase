using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MetaFly.Summer.IO;

namespace BMGenTool.Generate
{
    public class IDataGen
    {
        //this is used to log input file and tool info in output files
        static public string sydbFile;
        static public string toolVer;

        //this is used to control the main fram progress bar
        public delegate void GenProess(int total, int current);
        public event GenProess genPro;
        
        public virtual bool Generate(object outputpath)
        {
            return true;
        }

        public void UpdateProgressBar(int total, int cur)
        {
            if (this.genPro != null)
            {
                genPro(total, cur);
            }
        }

        //todo will delete
        public void AddLogHead(ref XmlFileHelper xmlFile)
        {
            //增加注释头
            List<string> comments = new List<string>();
            comments.Add(string.Format("Input SYDB file: {0}", sydbFile));
            comments.Add(string.Format("Data of generation: {0}", toolVer));
            xmlFile.InsertFirstComment(comments);
        }

        public List<string> AddLogHead()
        {
            //增加注释头
            List<string> comments = new List<string>();
            comments.Add(string.Format("Input SYDB file: {0}", sydbFile));
            comments.Add(string.Format("Data of generation: {0}", toolVer));
            return comments;
        }
    }
}
