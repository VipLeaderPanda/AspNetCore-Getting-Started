using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using WebDownload.Models;

namespace WebDownload.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<ChatHub> _chatHub;
       
        public HomeController(IHubContext<ChatHub> chatHub)
        {
            _chatHub = chatHub;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult About(bool cancel)
        {

            if (cancel)
            {
              


                if (GLOBAL.wbs !=null)
                {
                  
                       
                        for (int i=0;i<GLOBAL.wbs.Count;i++)
                        {
                            GLOBAL.wbs[i].CancelAsync();
                        }
                    
                }
            }
            else
            {
                string path = System.IO.Directory.GetCurrentDirectory() + @"\downloads";
                // 如果不存在就创建file文件夹
                if (!Directory.Exists(path))
                {
                    if (path != null) Directory.CreateDirectory(path);
                }
                string author_id = HttpContext.Session.GetString("author_id");
                string headfile = string.Format(@"{0}\\downloads\{1}\{2}", System.IO.Directory.GetCurrentDirectory(), author_id, "head.txt");
                if (System.IO.File.Exists(headfile))
                {
                    System.IO.File.Delete(headfile);
                }
                //判断网络连接是否正常


                //设置最大活动线程数以及可等待线程数
                ThreadPool.SetMaxThreads(3, 3);
                List<SynFileInfo> m_SynFileInfoList = JsonConvert.DeserializeObject<List<SynFileInfo>>(HttpContext.Session.GetString("m_SynFileInfoList"));

                //判断是否还存在任务
                if (m_SynFileInfoList.Count <= 0) return View("downstart");
                using (WebClient client = new WebClient())
                {
                    string savefile = string.Format(@"{0}\downloads\{1}\{2}", System.IO.Directory.GetCurrentDirectory(), author_id, "head.jpg");
                    string author_image = HttpContext.Session.GetString("author_image");
                    client.DownloadFile(new Uri(author_image), savefile);
                }
                GLOBAL.wbs = new List<WebClient>();
              

                foreach (SynFileInfo m_SynFileInfo in m_SynFileInfoList)
                {


                    //启动下载任务
                    StartDownLoad(m_SynFileInfo);
                }
               
            }
            return View("downstart");
        }
        #region 使用WebClient下载文件
        void StartDownLoad(SynFileInfo m_SynFileInfo)
        {
           
               
                //再次new 避免WebClient不能I/O并发 
                WebClient client = new WebClient();

                GLOBAL.wbs.Add(client);

             
                    //异步下载
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);

                    client.DownloadFileAsync(new Uri(m_SynFileInfo.DownPath), m_SynFileInfo.SavePath, m_SynFileInfo);

              

        }
        /// <summary>
        /// 下载进度条
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            SynFileInfo m_SynFileInfo = (SynFileInfo)e.UserState;
            m_SynFileInfo.SynProgress = e.ProgressPercentage + "%";
          
           
            _chatHub.Clients.All.SendAsync("ReceiveMessage", "ProgressChanged", m_SynFileInfo.id+":"+ m_SynFileInfo.name+":"+ e.ProgressPercentage + "%");
        }

        /// <summary>
        /// 下载完成调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //到此则一个文件下载完毕

            SynFileInfo m_SynFileInfo = (SynFileInfo)e.UserState;
            try
            {
                List<SynFileInfo> m_SynFileInfoList = JsonConvert.DeserializeObject<List<SynFileInfo>>(HttpContext.Session.GetString("m_SynFileInfoList"));
                m_SynFileInfoList.Remove(m_SynFileInfo);
                HttpContext.Session.SetString("m_SynFileInfoList", JsonConvert.SerializeObject(m_SynFileInfoList));
                if (m_SynFileInfoList.Count <= 0)
                {
                    //此时所有文件下载完毕
                    _chatHub.Clients.All.SendAsync("ReceiveMessage", "wjh", "全部完成");

                }
            }
            catch
            {

            }
            if (!e.Cancelled)
            {
                string content = string.Empty;
                string author_id = HttpContext.Session.GetString("author_id");
                string savefile = string.Format(@"{0}\\downloads\{1}\{2}", System.IO.Directory.GetCurrentDirectory(), author_id, "head.txt");
                Dictionary<string, Head> heads = JsonConvert.DeserializeObject<Dictionary<string, Head>>(HttpContext.Session.GetString("heads"));
                content = JsonConvert.SerializeObject(heads[m_SynFileInfo.id]);
                try
                {
                    using (FileStream fs = new FileStream(savefile, FileMode.Append))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                            sw.WriteLine(content);
                        fs.Close();
                    }
                }
                catch
                {

                }
                _chatHub.Clients.All.SendAsync("ReceiveMessage", "ProgressChanged", m_SynFileInfo.id + ":" + m_SynFileInfo.name + ":100%");
            }
           
        }

        #endregion

        [HttpPost]
        public IActionResult Index([FromBody]FormRequst model)
        {
            List<SynFileInfo> m_SynFileInfoList = new List<SynFileInfo>();
            try
            {
                string url = model.url;
                Encoding encoding = Encoding.UTF8;
                Dictionary<string, Head> heads = new Dictionary<string, Head>();
                
                using (WebClient wc = new WebClient())
                {
                    using (Stream resStream = wc.OpenRead(url))
                    {
                        using (StreamReader sr = new StreamReader(resStream, encoding))
                        {
                            string result_text = sr.ReadToEnd();
                            result_text = result_text.Substring(result_text.IndexOf("author_info") - 2);
                            result_text = result_text.Substring(0, result_text.IndexOf("logid") + 20);

                            VideoURLInfo info = Newtonsoft.Json.JsonConvert.DeserializeObject<VideoURLInfo>(result_text);
                            //添加列表(建立多个任务)
                            List<ArrayList> arrayListList = new List<ArrayList>();
                            string  author_image = info.author_info.avatar;
                            HttpContext.Session.SetString("author_image", author_image);
                            foreach (var item in info.video.results)
                            {
                                string path = System.IO.Directory.GetCurrentDirectory() + @"\downloads\" + info.author_info.id;

                                // 如果不存在就创建file文件夹
                                if (!Directory.Exists(path))
                                {
                                    if (path != null) Directory.CreateDirectory(path);
                                }
                                int pl_set = model.minute;
                                int pl_get = int.Parse(item.content.duration.Split(':')[0]);
                                if (pl_set < pl_get)
                                {
                                    continue;
                                }
                                string video_src = item.content.video_src;
                                if (video_src.Contains("?"))
                                {
                                    video_src = video_src.Substring(0, video_src.IndexOf('?'));
                                }
                                string savefile = string.Format(@"{0}\downloads\{1}\{2}", System.IO.Directory.GetCurrentDirectory(), info.author_info.id, Path.GetFileName(video_src));
                                if (item.content.displaytype_exinfo != null)
                                {
                                    m_SynFileInfoList.Add(new SynFileInfo()
                                    {
                                    
                                        id = item.content.pvid,
                                        name = Path.GetFileName(video_src),//文件名称
                                       
                                        SavePath = savefile,
                                       
                                        DownPath = item.content.video_src,//远程服务器下载地址
                                        SynProgress = "0%",//下载进度
                                    });
                                   
                                }
                                else
                                {

                                    m_SynFileInfoList.Add(new SynFileInfo()
                                    {
                                       
                                        id = item.content.pvid,
                                        name = Path.GetFileName(video_src),//文件名称
                                      
                                        SavePath = savefile,
                                       
                                        DownPath = item.content.video_src,//远程服务器下载地址
                                        SynProgress = "0%",//下载进度
                                    });
                               
                                }
                                heads.Add(item.content.pvid, new Head() { vid = item.content.vid, author = item.content.author, publish_time = item.content.publish_time, title = item.content.title, fileName = Path.GetFileName(video_src) });
                            }
                            
                           string author_id = info.author_info.id;
                            HttpContext.Session.SetString("author_id", author_id);
                            string url2 = string.Format("https://haokan.baidu.com/author/{0}?_format=json&rn=16&ctime={1}&_api=1", info.author_info.id, info.video.ctime);
                            getVideoInfo2(url2, info.author_info.id,model.minute,heads,m_SynFileInfoList);

                            HttpContext.Session.SetString("m_SynFileInfoList", JsonConvert.SerializeObject(m_SynFileInfoList));
                            HttpContext.Session.SetString("heads", JsonConvert.SerializeObject(heads));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                View(ex.Message);
            }
           
            return Json(m_SynFileInfoList);
        }
        private void getVideoInfo2(string url, string author_id,int minute, Dictionary<string, Head> heads, List<SynFileInfo> m_SynFileInfoList)
        {

            Encoding encoding = Encoding.UTF8;
            using (WebClient wc = new WebClient())
            {
                using (Stream resStream = wc.OpenRead(url))
                {
                    using (StreamReader sr = new StreamReader(resStream, encoding))
                    {
                        string result_text = sr.ReadToEnd();


                        VideoURLInfo2 info = Newtonsoft.Json.JsonConvert.DeserializeObject<VideoURLInfo2>(result_text);
                        if (info.data.response.results.Count == 0)
                        {
                            return;
                        }
                      
                        foreach (var item in info.data.response.results)
                        {
                            int pl_set = minute;
                            int pl_get = int.Parse(item.content.duration.Split(':')[0]);
                            if (pl_set < pl_get)
                            {
                                continue;
                            }
                            string video_src = item.content.video_src;
                            if (video_src.Contains("?"))
                            {
                                video_src = video_src.Substring(0, video_src.IndexOf('?'));
                            }
                            string savefile = string.Format(@"{0}\downloads\{1}\{2}", System.IO.Directory.GetCurrentDirectory(), author_id, Path.GetFileName(video_src));
                            if (item.content.displaytype_exinfo != null)
                            {
                                m_SynFileInfoList.Add(new SynFileInfo()
                                {
                                  
                                    id = item.content.pvid,
                                    name = Path.GetFileName(video_src),//文件名称
                                   
                                    SavePath = savefile,
                                    
                                    DownPath = item.content.video_src,//远程服务器下载地址
                                    SynProgress = "0%",//下载进度
                                });

                            }
                            else
                            {

                                m_SynFileInfoList.Add(new SynFileInfo()
                                {
                                  
                                    id = item.content.pvid,
                                    name = Path.GetFileName(video_src),//文件名称
                                 
                                    SavePath = savefile,
                                    
                                    DownPath = item.content.video_src,//远程服务器下载地址
                                    SynProgress = "0%",//下载进度
                                });

                            }
                            heads.Add(item.content.pvid, new Head() { vid = item.content.vid, author = item.content.author, publish_time = item.content.publish_time, title = item.content.title, fileName = Path.GetFileName(video_src) });
                        }
                       
                        string url2 = string.Format("https://haokan.baidu.com/author/{0}?_format=json&rn=16&ctime={1}&_api=1", author_id, info.data.response.ctime);
                        getVideoInfo2(url2, author_id,minute,heads,m_SynFileInfoList);
                    }
                }
            }
        }
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
