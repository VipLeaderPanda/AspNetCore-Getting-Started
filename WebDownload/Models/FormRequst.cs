using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebDownload.Models
{
    public class FormRequst
    {
        public string url
        {
            get;set;
        }
        public int minute
        {
            get;set;
        }
        public bool isCancel
        {
            get;set;
        }
    }
}
