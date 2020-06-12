using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WebDownload
{
    public class Download
    {

        public async Task<string> DownLoadFileTaskAsync(string url,string file)
        {
           
            WebClient wc = new WebClient();
            await wc.DownloadFileTaskAsync(url,file);

            return "";
        }
    }
}
