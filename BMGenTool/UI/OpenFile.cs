using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BMGenTest.UI
{
    public class OpenFile
    {
        private OpenFileDialog file;
        public String fileName { get; set; }
        public OpenFile(String path, String filter)
        {
            file = new OpenFileDialog();
            file.Filter = filter;
            file.InitialDirectory = path;
        }

        public void Show(TextBox txtBox)
        {
            if (null == file)
            {
                return;
            }
            if (file.ShowDialog() == DialogResult.OK)
            {
                this.fileName = file.FileName;
                txtBox.Text = this.fileName;
            }
        }

    }
}
