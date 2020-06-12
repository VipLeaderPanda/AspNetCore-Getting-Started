using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebDownload.Models
{
    public class FileOperate
    {
        #region 相应单位转换常量

        private const double KBCount = 1024;
        private const double MBCount = KBCount * 1024;
        private const double GBCount = MBCount * 1024;
        private const double TBCount = GBCount * 1024;

        #endregion

        #region 获取适应大小

        /// <summary>
        /// 得到适应大小
        /// </summary>
        /// <param name="size">字节大小</param>
        /// <param name="roundCount">保留小数(位)</param>
        /// <returns></returns>
        public static string GetAutoSizeString(double size, int roundCount)
        {
            if (KBCount > size) return Math.Round(size, roundCount) + "B";
            else if (MBCount > size) return Math.Round(size / KBCount, roundCount) + "KB";
            else if (GBCount > size) return Math.Round(size / MBCount, roundCount) + "MB";
            else if (TBCount > size) return Math.Round(size / GBCount, roundCount) + "GB";
            else return Math.Round(size / TBCount, roundCount) + "TB";
        }

        #endregion
    }
}
