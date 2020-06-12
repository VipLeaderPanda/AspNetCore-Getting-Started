using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VideoDownLoad
{
    public class SynFileInfo
    {
        public string DocID { get; set; }
        public string DocName { get; set; }
        public long FileSize { get; set; }
        public string SynSpeed { get; set; }
        public string SynProgress { get; set; }
        public string DownPath { get; set; }
        public string SavePath { get; set; }
        public DataGridViewRow RowObject { get; set; }
        public bool Async { get; set; }
        public DateTime LastTime { get; set; }

        public SynFileInfo(object[] objectArr)
        {
            int i = 0;
            DocID = objectArr[i].ToString(); i++;
            DocName = objectArr[i].ToString(); i++;
            FileSize = Convert.ToInt64(objectArr[i]); i++;
            SynSpeed = objectArr[i].ToString(); i++;
            SynProgress = objectArr[i].ToString(); i++;
            DownPath = objectArr[i].ToString(); i++;
            SavePath = objectArr[i].ToString(); i++;
            Async = Convert.ToBoolean(objectArr[i]); i++;
            RowObject = (DataGridViewRow)objectArr[i];
        }
    }
}
