using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BMGenTest.UI
{
    public class OpenFolder
    {
        private FolderBrowserDialog folder;
        public String pathName;
        public OpenFolder(String path)
        {
            folder = new FolderBrowserDialog();
            folder.ShowNewFolderButton = true;
            folder.SelectedPath = path;
        }

        public void Show(TextBox txtBox)
        {
            if (null == folder)
            {
                return;
            }
            if (folder.ShowDialog() == DialogResult.OK)
            {
                this.pathName = folder.SelectedPath;
                txtBox.Text = this.pathName;
            }
        }
    }
}
